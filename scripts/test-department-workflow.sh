#!/bin/bash

echo "🏢 TESTING DEPARTMENT FEATURES"

# Login as admin
ADMIN_TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin2@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

echo "✅ Admin logged in"

# List departments (should show seeded departments)
echo ""
echo "📋 Listing departments..."
curl -k -s -X GET https://acme.localhost:5000/api/departments \
  -H "Authorization: Bearer$ADMIN_TOKEN" | jq '.'

# Create custom department
echo ""
echo "🏢 Creating custom department..."
DEPT_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/departments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d '{
    "name":"Cloud Architecture",
    "description":"Designs and implements cloud infrastructure and solutions"
  }')

echo "$DEPT_RESPONSE" | jq '.'

DEPT_ID=$(echo "$DEPT_RESPONSE" | jq -r '.departmentId')
echo "✅ Department created:$DEPT_ID"

# Assign user to department (via SQL for now)
echo ""
echo "💡 To assign users to departments, run this SQL:"
echo "UPDATE [shared].[Users] SET DepartmentId = '$DEPT_ID' WHERE Email = 'developer@acme.com';"

echo ""
echo "🎉 Department tests complete!"