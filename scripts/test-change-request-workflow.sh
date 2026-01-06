#!/bin/bash

# Login
TOKEN=$(curl -k -s -X POST https://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"YourPassword!"}' \
  | jq -r '.token')

# 1. CREATE CHANGE REQUEST
CR_ID=$(curl -k -s -X POST https://localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{
    "title":"Upgrade SQL Server to 2025",
    "description":"Upgrade production database from SQL Server 2022 to 2025",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"High",
    "impactAssessment":"Will cause 15-minute downtime. All services affected during upgrade window.",
    "rollbackPlan":"Restore from backup if upgrade fails. Estimated rollback time: 30 minutes.",
    "affectedSystems":"Database Server, API, Web App, Mobile App",
    "scheduledStartDate":"2026-02-15T02:00:00Z",
    "scheduledEndDate":"2026-02-15T06:00:00Z",
    "changeWindow":"Saturday 2AM-6AM EST"
  }' | jq -r '.changeRequestId')

echo "✅ ChangeRequest created:$CR_ID"

# 2. SUBMIT FOR CAB REVIEW
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer$TOKEN"

echo "✅ Submitted for CAB review"

# 3. START CAB REVIEW
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/start-review \
  -H "Authorization: Bearer$TOKEN"

echo "✅ CAB review started"

# 4. APPROVE CHANGE
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{"notes":"Approved by CAB. Scheduled for Feb 15 maintenance window."}'

echo "✅ Change approved"

# 5. SCHEDULE CHANGE
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{
    "scheduledStartDate":"2026-02-15T02:00:00Z",
    "scheduledEndDate":"2026-02-15T06:00:00Z",
    "changeWindow":"Saturday 2AM-6AM EST"
  }'

echo "✅ Change scheduled"

# 6. START EXECUTION
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/start-execution \
  -H "Authorization: Bearer$TOKEN"

echo "✅ Execution started"

# 7. UPDATE IMPLEMENTATION NOTES
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/update-notes \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{"implementationNotes":"Backup completed. Starting upgrade process..."}'

echo "✅ Notes updated"

# 8. COMPLETE CHANGE
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/complete \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{"implementationNotes":"Upgrade completed successfully. All systems verified operational."}'

echo "✅ Change completed"

# View final status
curl -k -s -X GET https://localhost:5000/api/change-requests/$CR_ID \
  -H "Authorization: Bearer$TOKEN" | jq '.'

echo ""
echo "🎉 COMPLETE CHANGE MANAGEMENT WORKFLOW TESTED!"