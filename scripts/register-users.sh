#!/bin/bash

echo "📝 REGISTERING USERS SCRIPT"

# Register project manager user
REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"schen@acme.com",
    "password":"SecurePass123!",
    "firstName":"Sarah",
    "lastName":"Chen"
  }')

  echo "$REGISTER_RESPONSE" | jq '.'

  # Register CMB Member user
REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"mrodriguez@acme.com",
    "password":"SecurePass123!",
    "firstName":"Michael",
    "lastName":"Rodriguez"
  }')

  echo "$REGISTER_RESPONSE" | jq '.'

# Register Developer user
REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"akim@acme.com",
    "password":"SecurePass123!",
    "firstName":"Alex",
    "lastName":"Kim"
  }')

  echo "$REGISTER_RESPONSE" | jq '.'

# Register IT Operations user
REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"jwilliams@acme.com",
    "password":"SecurePass123!",
    "firstName":"Jennifer",
    "lastName":"Williams"
  }')

  echo "$REGISTER_RESPONSE" | jq '.'

  # Register Executive user
REGISTER_RESPONSE=$(curl -k -s -X POST https://acme.localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"dpark@acme.com",
    "password":"SecurePass123!",
    "firstName":"David",
    "lastName":"Park"
  }')

  echo "$REGISTER_RESPONSE" | jq '.'