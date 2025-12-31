#!/bin/bash

echo "🔐 Testing Login..."

# Login as developer (WITH SUBDOMAIN)
TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"developer@acme.com","password":"SecurePass123!"}' \
  | jq -r '.accessToken')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo "❌ Failed to get developer token!"
  exit 1
fi

echo "✅ Developer Token received"

# # Register admin2 (WITH SUBDOMAIN - CRITICAL!)
# echo ""
# echo "📝 Registering admin3 user..."
# REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
#   -H "Content-Type: application/json" \
#   -d '{
#     "email":"admin2@acme.com",
#     "password":"SecureAdminPass123!",
#     "firstName":"Admin2",
#     "lastName":"User"
#   }')

# echo "$REGISTER_RESPONSE" | jq '.'

# # Now assign TenantAdmin role via SQL
# echo ""
# echo "⚠️  MANUAL STEP: Run this SQL to assign TenantAdmin role:"
# echo ""
# echo "INSERT INTO [shared].[UserRoles] (UserId, RoleId)"
# echo "SELECT"
# echo "  (SELECT Id FROM [shared].[Users] WHERE Email = 'admin2@acme.com'),"
# echo "  (SELECT Id FROM [shared].[Roles] WHERE Name = 'TenantAdmin');"
# echo ""
# read -p "Press Enter after running the SQL..."

# Login as admin2
echo ""
echo "🔐 Logging in as admin2..."
ADMIN_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin2@acme.com","password":"SecureAdminPass123!"}')

echo "Admin login response:"
echo "$ADMIN_RESPONSE" | jq '.'

ADMIN_TOKEN=$(echo "$ADMIN_RESPONSE" | jq -r '.accessToken')

if [ -z "$ADMIN_TOKEN" ] || [ "$ADMIN_TOKEN" == "null" ]; then
  echo "❌ Failed to get admin token!"
  exit 1
fi

echo "✅ Admin Token received: ${ADMIN_TOKEN:0:50}..."

# Continue with tests...
echo ""
echo "📝 Creating project request..."
PR_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/project-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Build Customer Portal v2",
    "description":"Create a self-service portal where customers can view orders, track shipments, and manage their account settings",
    "priority":"High",
    "dueDate":"2026-06-01T00:00:00Z"
  }')

echo "$PR_RESPONSE" | jq '.'

PROJECT_REQUEST_ID=$(echo "$PR_RESPONSE" | jq -r '.projectRequestId')
echo "✅ Project Request ID: $PROJECT_REQUEST_ID"

# Submit
echo ""
echo "📤 Submitting for review..."
curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID/submit \
  -H "Authorization: Bearer $TOKEN" | jq '.'

# Approve
echo ""
echo "✅ Approving project request..."
curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{
    "notes":"Approved for Q2 2026. Budget allocated. Ready for project creation."
  }' | jq '.'

# Verify
echo ""
echo "🔍 Verifying final status..."
curl -k -s -X GET https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID \
  -H "Authorization: Bearer $TOKEN" | jq '{status, approvalNotes, approvedByUserId}'

echo ""
echo "🎉 All tests completed!"