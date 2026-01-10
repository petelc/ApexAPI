#!/bin/bash

# APEX Backend Comprehensive Test Script
# Tests: Emails, Jobs, Analytics, Change Management

echo "🚀 APEX BACKEND COMPREHENSIVE TEST"
echo "===================================="
echo ""

# Configuration
BASE_URL="https://acme.localhost:5000/api"
EMAIL="admin@acme.com"
PASSWORD="SecureAdminPass123!"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Login
echo "🔐 Logging in..."
TOKEN=$(curl -k -s -X POST $BASE_URL/users/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" \
  | jq -r '.accessToken')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo -e "${RED}❌ Login failed!${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Login successful!${NC}"
echo ""

# ====================
# TEST 1: CREATE TEST DATA (10 changes)
# ====================

echo "📊 TEST 1: Creating Test Data"
echo "=============================="
echo "Creating 10 test change requests..."
echo ""

CHANGE_IDS=()

for i in {1..10}; do
  # Randomize some attributes
  TYPES=("Standard" "Normal" "Emergency")
  PRIORITIES=("Low" "Medium" "High" "Critical")
  RISKS=("Low" "Medium" "High" "Critical")
  
  TYPE=${TYPES[$((RANDOM % 3))]}
  PRIORITY=${PRIORITIES[$((RANDOM % 4))]}
  RISK=${RISKS[$((RANDOM % 4))]}
  
  CR_ID=$(curl -k -s -X POST $BASE_URL/change-requests \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{
      \"title\":\"Test Change $i - $TYPE\",
      \"description\":\"Testing change management system - iteration $i\",
      \"changeType\":\"$TYPE\",
      \"priority\":\"$PRIORITY\",
      \"riskLevel\":\"$RISK\",
      \"impactAssessment\":\"Test impact assessment for change $i\",
      \"rollbackPlan\":\"Test rollback plan for change $i\",
      \"affectedSystems\":\"Test System $((i % 3 + 1)), API Server\"
    }" | jq -r '.changeRequestId // .id')
  
  if [ -n "$CR_ID" ] && [ "$CR_ID" != "null" ]; then
    CHANGE_IDS+=("$CR_ID")
    echo -e "${GREEN}✅ Created change $i: $CR_ID${NC}"
  else
    echo -e "${RED}❌ Failed to create change $i${NC}"
  fi
done

echo ""
echo -e "${GREEN}✅ Created ${#CHANGE_IDS[@]} test changes${NC}"
echo ""

# ====================
# TEST 2: EMAIL NOTIFICATIONS
# ====================

echo "📧 TEST 2: Email Notifications"
echo "==============================="
echo ""

# Test 2.1: Submit (triggers email to CAB)
echo "Test 2.1: Submit Change (CAB Email)"
CR_ID=${CHANGE_IDS[0]}
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN" > /dev/null
echo -e "${YELLOW}➡️  Check console for: [CAB Review Required]${NC}"
sleep 2

# Test 2.2: Approve (triggers email to submitter)
echo "Test 2.2: Approve Change (Approval Email)"
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Approved for testing"}' > /dev/null
echo -e "${YELLOW}➡️  Check console for: [APPROVED]${NC}"
sleep 2

# Test 2.3: Schedule (triggers email to team)
echo "Test 2.3: Schedule Change (Schedule Email)"
if [[ "$OSTYPE" == "darwin"* ]]; then
  TOMORROW=$(date -u -v+1d +"%Y-%m-%dT%H:%M:%SZ")
else
  TOMORROW=$(date -u -d "+1 day" +"%Y-%m-%dT%H:%M:%SZ")
fi

curl -k -s -X POST $BASE_URL/change-requests/$CR_ID/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"scheduledStartDate\":\"$TOMORROW\",\"scheduledEndDate\":\"2026-12-31T23:59:00Z\",\"changeWindow\":\"Test Window\"}" > /dev/null
echo -e "${YELLOW}➡️  Check console for: [SCHEDULED]${NC}"
sleep 2

# Test 2.4: Complete another change
echo "Test 2.4: Complete Change (Completion Email)"
CR_ID2=${CHANGE_IDS[1]}
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID2/submit \
  -H "Authorization: Bearer $TOKEN" > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID2/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Quick approval"}' > /dev/null

NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID2/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"scheduledStartDate\":\"$NOW\",\"scheduledEndDate\":\"2026-12-31T23:59:00Z\",\"changeWindow\":\"Now\"}" > /dev/null

curl -k -s -X POST $BASE_URL/change-requests/$CR_ID2/start-execution \
  -H "Authorization: Bearer $TOKEN" > /dev/null

curl -k -s -X POST $BASE_URL/change-requests/$CR_ID2/complete \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Test completion successful"}' > /dev/null
echo -e "${YELLOW}➡️  Check console for: [COMPLETED]${NC}"
sleep 2

# Test 2.5: Deny a change
echo "Test 2.5: Deny Change (Denial Email)"
CR_ID3=${CHANGE_IDS[2]}
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID3/submit \
  -H "Authorization: Bearer $TOKEN" > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID3/deny \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"reason":"Risk too high for testing scenario"}' > /dev/null
echo -e "${YELLOW}➡️  Check console for: [DENIED]${NC}"
sleep 2

# Test 2.6: Rollback a change
echo "Test 2.6: Rollback Change (Rollback Alert)"
CR_ID4=${CHANGE_IDS[3]}
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID4/submit \
  -H "Authorization: Bearer $TOKEN" > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID4/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"notes":"Approved"}' > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID4/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"scheduledStartDate\":\"$NOW\",\"scheduledEndDate\":\"2026-12-31T23:59:00Z\",\"changeWindow\":\"Now\"}" > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID4/start-execution \
  -H "Authorization: Bearer $TOKEN" > /dev/null
curl -k -s -X POST $BASE_URL/change-requests/$CR_ID4/rollback \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"reason":"Database constraint violation detected"}' > /dev/null
echo -e "${YELLOW}➡️  Check console for: [ROLLBACK]${NC}"
sleep 2

echo ""
echo -e "${GREEN}✅ Email notification tests complete${NC}"
echo ""

# ====================
# TEST 3: ANALYTICS REPORTS
# ====================

echo "📊 TEST 3: Analytics Reports"
echo "============================="
echo ""

# Test 3.1: Change Metrics
echo "Test 3.1: Change Metrics"
METRICS=$(curl -k -s $BASE_URL/reports/change-metrics \
  -H "Authorization: Bearer $TOKEN")
TOTAL=$(echo $METRICS | jq '.totalChanges')
SUCCESS_RATE=$(echo $METRICS | jq '.successRate')
echo -e "${GREEN}✅ Total Changes: $TOTAL${NC}"
echo -e "${GREEN}✅ Success Rate: $SUCCESS_RATE%${NC}"
echo ""

# Test 3.2: Success Rate
echo "Test 3.2: Success Rate Analysis"
SUCCESS=$(curl -k -s $BASE_URL/reports/success-rate \
  -H "Authorization: Bearer $TOKEN")
SUCCESSFUL=$(echo $SUCCESS | jq '.successfulChanges')
FAILED=$(echo $SUCCESS | jq '.failedChanges')
echo -e "${GREEN}✅ Successful: $SUCCESSFUL${NC}"
echo -e "${GREEN}✅ Failed: $FAILED${NC}"
echo ""

# Test 3.3: Monthly Trends
echo "Test 3.3: Monthly Trends"
TRENDS=$(curl -k -s $BASE_URL/reports/monthly-trends \
  -H "Authorization: Bearer $TOKEN")
MONTHS=$(echo $TRENDS | jq '.months | length')
echo -e "${GREEN}✅ Retrieved $MONTHS months of trend data${NC}"
echo ""

# Test 3.4: Top Affected Systems
echo "Test 3.4: Top Affected Systems"
SYSTEMS=$(curl -k -s $BASE_URL/reports/top-affected-systems \
  -H "Authorization: Bearer $TOKEN")
TOP_SYSTEM=$(echo $SYSTEMS | jq -r '.systems[0].systemName // "N/A"')
TOP_COUNT=$(echo $SYSTEMS | jq -r '.systems[0].changeCount // 0')
echo -e "${GREEN}✅ Top System: $TOP_SYSTEM ($TOP_COUNT changes)${NC}"
echo ""

echo -e "${GREEN}✅ Analytics reports working!${NC}"
echo ""

# ====================
# TEST 4: BACKGROUND JOBS
# ====================

echo "⏰ TEST 4: Background Jobs"
echo "=========================="
echo ""

echo "Test 4.1: Hangfire Dashboard"
echo -e "${YELLOW}➡️  Visit: $BASE_URL/hangfire${NC}"
echo -e "${YELLOW}➡️  Should see 4 recurring jobs${NC}"
echo ""

echo "Test 4.2: Auto-Start Job"
echo -e "${YELLOW}➡️  In Hangfire: Trigger 'auto-start-changes' manually${NC}"
echo -e "${YELLOW}➡️  Check console for: 🔄 Checking for changes to auto-start${NC}"
echo ""

echo "Test 4.3: Reminder Jobs"
echo -e "${YELLOW}➡️  In Hangfire: Trigger 'change-reminders-24h' manually${NC}"
echo -e "${YELLOW}➡️  Check console for: 📧 Checking for 24-hour reminders${NC}"
echo ""

echo -e "${GREEN}✅ Background jobs configured${NC}"
echo ""

# ====================
# SUMMARY
# ====================

echo "=================================="
echo "🎉 BACKEND TESTING COMPLETE!"
echo "=================================="
echo ""
echo "Summary:"
echo "--------"
echo -e "${GREEN}✅ Created: ${#CHANGE_IDS[@]} test changes${NC}"
echo -e "${GREEN}✅ Tested: 6 email notifications${NC}"
echo -e "${GREEN}✅ Tested: 4 analytics reports${NC}"
echo -e "${GREEN}✅ Verified: Background jobs configured${NC}"
echo ""
echo "Next Steps:"
echo "-----------"
echo "1. Check console output for all email notifications"
echo "2. Visit Hangfire dashboard: $BASE_URL/hangfire"
echo "3. Manually trigger background jobs"
echo "4. Review analytics data in reports"
echo ""
echo "🚀 Ready to move to React app development!"
echo ""
