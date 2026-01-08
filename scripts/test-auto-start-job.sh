#!/bin/bash

echo "🚀 Testing Auto-Start Change Job"
echo "================================="
echo ""

# 1. Login
echo "1️⃣ Logging in..."
TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "❌ Login failed!"
  exit 1
fi

echo "✅ Login successful!"
echo ""

# 2. Create change request
echo "2️⃣ Creating change request..."
CR_ID=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Auto-Start Test",
    "description":"Testing automatic change start",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"Low",
    "impactAssessment":"Minimal - test only",
    "rollbackPlan":"Simple revert",
    "affectedSystems":"Test System"
  }' | jq -r '.changeRequestId // .id')

if [ -z "$CR_ID" ] || [ "$CR_ID" = "null" ]; then
  echo "❌ Failed to create change request!"
  exit 1
fi

echo "✅ Created change request: $CR_ID"
echo ""

# 3. Submit
echo "3️⃣ Submitting for CAB review..."
curl -k -s -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN" > /dev/null

echo "✅ Submitted"
echo ""

# 4. Approve
echo "4️⃣ Approving change..."
curl -k -s -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Approved for auto-start testing"}' > /dev/null

echo "✅ Approved"
echo ""

# 5. Schedule for NOW (1 minute ago to trigger immediately)
echo "5️⃣ Scheduling change for NOW..."

# Get current time in UTC
if [[ "$OSTYPE" == "darwin"* ]]; then
  # macOS
  PAST_TIME=$(date -u -v-1M +"%Y-%m-%dT%H:%M:%SZ")
  END_TIME=$(date -u -v+1H +"%Y-%m-%dT%H:%M:%SZ")
else
  # Linux
  PAST_TIME=$(date -u -d "-1 minute" +"%Y-%m-%dT%H:%M:%SZ")
  END_TIME=$(date -u -d "+1 hour" +"%Y-%m-%dT%H:%M:%SZ")
fi

curl -k -s -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"scheduledStartDate\":\"$PAST_TIME\",
    \"scheduledEndDate\":\"$END_TIME\",
    \"changeWindow\":\"Immediate Test\"
  }" > /dev/null

echo "✅ Scheduled for: $PAST_TIME"
echo ""

# 6. Instructions
echo "================================="
echo "✅ SETUP COMPLETE!"
echo ""
echo "📋 Change Request ID: $CR_ID"
echo "📅 Scheduled Start: $PAST_TIME (in the past - should trigger immediately)"
echo ""
echo "🎯 NEXT STEPS:"
echo ""
echo "Option A - Wait for automatic execution (5 minutes):"
echo "  1. Wait 5 minutes for next job run"
echo "  2. Check change status:"
echo "     curl -k -s https://acme.localhost:5000/api/change-requests/$CR_ID \\"
echo "       -H \"Authorization: Bearer $TOKEN\" | jq '.status'"
echo "  3. Should show 'InProgress'"
echo ""
echo "Option B - Trigger manually (instant):"
echo "  1. Visit https://acme.localhost:5000/hangfire"
echo "  2. Click 'Recurring jobs'"
echo "  3. Find 'auto-start-changes'"
echo "  4. Click 'Trigger now'"
echo "  5. Watch console output for:"
echo "     🚀 Found 1 change(s) to auto-start"
echo "     ✅ Successfully started change"
echo ""
echo "🌐 Hangfire Dashboard: https://acme.localhost:5000/hangfire"
echo "================================="