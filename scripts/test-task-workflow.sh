#!/bin/bash

echo "📋 TESTING TASK WORKFLOW WITH DEPARTMENTS"

# Login
TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"developer@acme.com","password":"SecurePass123!"}' \
  | jq -r '.accessToken')

ADMIN_TOKEN=$(curl -k -s -X POST https://acme.localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin2@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.accessToken')

# Create ProjectRequest
PR_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/project-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{
    "title":"Security Audit Implementation",
    "description":"Implement comprehensive security audit system",
    "priority":"High"
  }')

PR_ID=$(echo "$PR_RESPONSE" | jq -r '.projectRequestId')

# Submit & Approve
curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PR_ID/submit \
  -H "Authorization: Bearer$TOKEN" > /dev/null

curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PR_ID/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d '{"notes":"Approved"}' > /dev/null

# Convert to Project
PROJECT_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/project-requests/$PR_ID/convert \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d '{}')

PROJECT_ID=$(echo "$PROJECT_RESPONSE" | jq -r '.projectId')
echo "✅ Project created:$PROJECT_ID"

# Create Task
TASK_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/projects/$PROJECT_ID/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{
    "title":"Code review security vulnerabilities",
    "description":"Review codebase for common security issues",
    "priority":"High",
    "estimatedHours":16,
    "dueDate":"2026-02-15T00:00:00Z"
  }')

TASK_ID=$(echo "$TASK_RESPONSE" | jq -r '.taskId')
echo "✅ Task created:$TASK_ID"

# Get Security Department ID
DEPT_ID=$(curl -k -s -X GET https://acme.localhost:5000/api/departments \
  -H "Authorization: Bearer$TOKEN" \
  | jq -r '.[] | select(.name=="Information Security") | .id')

echo "✅ Security Department ID:$DEPT_ID"

# Assign Task to Security Department
curl -k -s -X POST https://acme.localhost:5000/api/tasks/$TASK_ID/assign-to-department \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$ADMIN_TOKEN" \
  -d "{\"departmentId\":\"$DEPT_ID\"}" | jq '.'

echo "✅ Task assigned to Security Department"

# User claims task
curl -k -s -X POST https://acme.localhost:5000/api/tasks/$TASK_ID/claim \
  -H "Authorization: Bearer$TOKEN" | jq '.'

echo "✅ Task claimed by user"

# Start task
curl -k -s -X POST https://acme.localhost:5000/api/tasks/$TASK_ID/start \
  -H "Authorization: Bearer$TOKEN" | jq '.'

# Log time
curl -k -s -X POST https://acme.localhost:5000/api/tasks/$TASK_ID/log-time \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer$TOKEN" \
  -d '{"hours":4}' | jq '.'

echo "✅ Logged 4 hours"

# Complete task
curl -k -s -X POST https://acme.localhost:5000/api/tasks/$TASK_ID/complete \
  -H "Authorization: Bearer$TOKEN" | jq '.'

echo "🎉 TASK WORKFLOW COMPLETE!"