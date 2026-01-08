#!/bin/bash

echo "🚀 Testing APEX Email Notifications + Change Management"
echo "========================================================"
echo ""

# Login and get token
echo "1️⃣ Logging in..."
TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "❌ Login failed! Check your credentials."
  exit 1
fi

echo "✅ Login successful!"
echo "Token: ${TOKEN:0:50}..."
echo ""

# Create change request
echo "2️⃣ Creating change request..."
CR_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Email Notification Test",
    "description":"Testing the email notification system for change requests",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"Medium",
    "impactAssessment":"This is a test change with minimal impact. Used to verify email notifications are working correctly.",
    "rollbackPlan":"Simple rollback - revert to previous state if needed.",
    "affectedSystems":"Test System, Notification System"
  }')

echo "Response: $CR_RESPONSE"
echo ""

CR_ID=$(echo "$CR_RESPONSE" | jq -r '.changeRequestId // .id // empty')

if [ -z "$CR_ID" ] || [ "$CR_ID" = "null" ]; then
  echo "❌ Failed to create change request!"
  echo "Response was: $CR_RESPONSE"
  exit 1
fi

echo "✅ Change request created: $CR_ID"
echo ""

# Submit for CAB review (this triggers the email!)
echo "3️⃣ Submitting for CAB review (triggering email)..."
SUBMIT_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN")

echo "Submit response: $SUBMIT_RESPONSE"
echo ""

echo "========================================================"
echo "✅ TEST COMPLETE!"
echo ""
echo "📧 CHECK YOUR CONSOLE OUTPUT FOR EMAIL!"
echo ""
echo "Look for something like:"
echo "================== EMAIL (Console Mode) =================="
echo "From: APEX Platform <noreply@apex.com>"
echo "To: Admin User <admin@acme.com>"
echo "Subject: [CAB Review Required] Email Notification Test"
echo "=========================================================="
echo ""
echo "🌐 Also check Hangfire dashboard:"
echo "https://acme.localhost:5000/hangfire"
echo ""
