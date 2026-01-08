# Get token
TOKEN=$(curl -k -s -X POST https://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@acme.com","password":"SecureAdminPass123!"}' \
  | jq -r '.token')

# Create & submit change request
CR_ID=$(curl -k -s -X POST https://localhost:5000/api/change-requests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title":"Email Test",
    "description":"Testing notifications",
    "changeType":"Normal",
    "priority":"High",
    "riskLevel":"Medium",
    "impactAssessment":"Test impact",
    "rollbackPlan":"Test rollback",
    "affectedSystems":"Test System"
  }' | jq -r '.changeRequestId')

# Submit (triggers email!)
curl -k -s -X POST https://localhost:5000/api/change-requests/$CR_ID/submit \
  -H "Authorization: Bearer $TOKEN"