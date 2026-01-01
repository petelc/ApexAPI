#!/bin/bash

echo "🎯 TESTING PROJECT CONVERSION WORKFLOW"
echo ""

# Login as developer
TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"developer@acme.com","password":"SecurePass123!"}' \
  | jq -r '.accessToken')

echo "✅ Developer logged in"

# Create ProjectRequest
PR_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/project-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{
    "title":"E-Commerce Platform Rebuild",
    "description":"Complete rebuild of our e-commerce platform using modern microservices architecture with React frontend",
    "priority":"High",
    "dueDate":"2026-12-31T00:00:00Z"
  }')

PROJECT_REQUEST_ID=$(echo "$PR_RESPONSE" | jq -r '.projectRequestId')
echo "✅ ProjectRequest created:$PROJECT_REQUEST_ID"

# Submit
curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID/submit \
  -H "Authorization: Bearer$TOKEN" > /dev/null

echo "✅ ProjectRequest submitted"

# Login as admin
ADMIN_TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin2@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

echo "✅ Admin logged in"

# Approve
curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d '{"notes":"Approved for 2026 roadmap. Budget: $500K"}' > /dev/null

echo "✅ ProjectRequest approved"

# 🎉 CONVERT TO PROJECT (THE MAGIC MOMENT!)
echo ""
echo "🎯 Converting ProjectRequest to Project..."
PROJECT_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID/convert \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d '{
    "startDate":"2026-02-01T00:00:00Z",
    "endDate":"2026-12-31T00:00:00Z",
    "budget":500000
  }')

echo "$PROJECT_RESPONSE" | jq '.'

PROJECT_ID=$(echo "$PROJECT_RESPONSE" | jq -r '.projectId')
echo ""
echo "🎉 PROJECT CREATED:$PROJECT_ID"

# Get Project details
echo ""
echo "📋 Project Details:"
curl -k -s -X GET https://acme.localhost:5000/api/projects/$PROJECT_ID \
  -H "Authorization: Bearer$TOKEN" | jq '.'

# Verify ProjectRequest is now Converted
echo ""
echo "📋 ProjectRequest Status (should be 'Converted'):"
curl -k -s -X GET https://acme.localhost:5000/api/project-requests/$PROJECT_REQUEST_ID \
  -H "Authorization: Bearer$TOKEN" | jq '{status, projectId}'

# List all projects
echo ""
echo "📋 All Projects:"
curl -k -s -X GET https://acme.localhost:5000/api/projects \
  -H "Authorization: Bearer$TOKEN" | jq '.items[] | {id, name, status}'

echo ""
echo "🎉 CONVERSION WORKFLOW COMPLETE!"