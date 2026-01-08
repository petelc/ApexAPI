#!/bin/bash

echo "🚀 Testing Approval Email Notifications"
echo "================================="
echo ""

TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin2@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

# Create & submit change
CR_ID=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Approval Email Test",
    "description":"Testing approval notification",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"Low",
    "impactAssessment":"Test",
    "rollbackPlan":"Test",
    "affectedSystems":"Test"
  }' | jq -r '.changeRequestId')

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN"

# Approve (triggers email!)
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Approved for testing email system"}'

# Check console for:
# ✅ [APPROVED] Your Change Request: Approval Email Test

echo "🚀 Testing Denial Email Notifications"
echo "================================="
echo ""

# Create & submit another change
CR_ID=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Denial Email Test",
    "description":"Testing denial notification",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"High",
    "impactAssessment":"Test",
    "rollbackPlan":"Test",
    "affectedSystems":"Test"
  }' | jq -r '.changeRequestId')

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN"

# Deny (triggers email!)
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/deny \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"reason":"Risk level too high without additional mitigation plan"}'

# Check console for:
# ❌ [DENIED] Your Change Request: Denial Email Test

echo "🚀 Testing Schedule Email Notifications"
echo "================================="
echo ""

# Create, submit, approve change
CR_ID=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Schedule Email Test",
    "description":"Testing schedule notification",
    "changeType":"Normal",
    "priority":"Medium",
    "riskLevel":"Low",
    "impactAssessment":"Test",
    "rollbackPlan":"Test",
    "affectedSystems":"Production DB"
  }' | jq -r '.changeRequestId')

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN"

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Approved"}'

# Schedule (triggers email!)
TOMORROW=$(date -u -v+1d +"%Y-%m-%dT%H:%M:%SZ")  # macOS
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "scheduledStartDate":"'"$TOMORROW"'",
    "scheduledEndDate":"2026-12-31T23:59:00Z",
    "changeWindow":"Maintenance Window"
  }'

# Check console for:
# 📅 [SCHEDULED] Change Request: Schedule Email Test

echo "🚀 Testing Complete Email Notifications"
echo "================================="
echo ""

# Use existing scheduled change and complete it
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/start-execution \
  -H "Authorization: Bearer $TOKEN"

# Complete (triggers email!)
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/complete \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"All systems verified, deployment successful"}'

# Check console for:
# ✅ [COMPLETED] Change Request: Schedule Email Test

echo "🚀 Testing Rollback Email Notifications"
echo "================================="
echo ""

# Create and start a change
CR_ID=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Rollback Email Test",
    "description":"Testing rollback notification",
    "changeType":"Emergency",
    "priority":"Critical",
    "riskLevel":"High",
    "impactAssessment":"Critical system",
    "rollbackPlan":"Restore from backup",
    "affectedSystems":"Production API"
  }' | jq -r '.changeRequestId')

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN"

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Emergency approval"}'

NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "scheduledStartDate":"'"$NOW"'",
    "scheduledEndDate":"2026-12-31T23:59:00Z",
    "changeWindow":"Now"
  }'

curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/start-execution \
  -H "Authorization: Bearer $TOKEN"

# Rollback (triggers email!)
curl -k -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/rollback \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"reason":"Database migration failed - constraint violations detected"}'

# Check console for:
# 🚨 [ROLLBACK] Change Request Rolled Back: Rollback Email Test