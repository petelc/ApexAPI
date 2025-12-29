#!/bin/bash

echo "🔍 FASTENDPOINTS DIAGNOSTIC"
echo "================================"
echo ""

echo "1️⃣ Checking FastEndpoints packages..."
dotnet list src/Apex.API.Web package | grep -i fast
echo ""

echo "2️⃣ Checking for GlobalUsings.cs..."
if [ -f "src/Apex.API.Web/GlobalUsings.cs" ]; then
    echo "Found GlobalUsings.cs:"
    cat src/Apex.API.Web/GlobalUsings.cs
else
    echo "No GlobalUsings.cs found"
fi
echo ""

echo "3️⃣ Checking GetTenant.cs using statements..."
head -20 src/Apex.API.Web/Endpoints/Tenants/GetTenant.cs
echo ""

echo "4️⃣ Checking for MVC conflicts..."
grep -r "using Microsoft.AspNetCore.Mvc" src/Apex.API.Web/ --include="*.cs" || echo "No MVC usings found (good!)"
echo ""

echo "5️⃣ Checking FastEndpoints DLL..."
ls -lah ~/.nuget/packages/fastendpoints/7.1.1/lib/net8.0/ 2>/dev/null || echo "FastEndpoints 7.1.1 not in cache"
echo ""

echo "6️⃣ Testing if Endpoint base class is recognized..."
grep -A 5 "class GetTenantEndpoint" src/Apex.API.Web/Endpoints/Tenants/GetTenant.cs
echo ""

echo "================================"
echo "✅ Diagnostic complete!"
