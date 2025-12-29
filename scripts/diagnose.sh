#!/bin/bash

echo "🔍 APEX DIAGNOSTIC REPORT"
echo "========================================"
echo ""

echo "1️⃣ PROJECT STRUCTURE"
echo "--------------------"
find src/Apex.API.Web/Endpoints -name "*.cs" -type f
echo ""

echo "2️⃣ ENDPOINT CONSTRUCTORS (Full)"
echo "--------------------------------"
for file in src/Apex.API.Web/Endpoints/Tenants/*.cs; do
    echo "FILE: $file"
    echo "---"
    grep -A 20 "public.*Endpoint(" "$file" || echo "No constructor found"
    echo ""
done

echo "3️⃣ PROGRAM.CS"
echo "-------------"
cat src/Apex.API.Web/Program.cs
echo ""

echo "4️⃣ DEPENDENCYINJECTION.CS"
echo "-------------------------"
cat src/Apex.API.Infrastructure/DependencyInjection.cs
echo ""

echo "5️⃣ TENANTCONTEXT.CS (Constructor only)"
echo "--------------------------------------"
grep -A 15 "public TenantContext(" src/Apex.API.Infrastructure/Identity/TenantContext.cs
echo ""

echo "6️⃣ APEXDBCONTEXT.CS (Constructor only)"
echo "--------------------------------------"
grep -A 10 "public ApexDbContext(" src/Apex.API.Infrastructure/Data/ApexDbContext.cs
echo ""

echo "7️⃣ PROJECT REFERENCES (Infrastructure)"
echo "---------------------------------------"
grep "ProjectReference" src/Apex.API.Infrastructure/Apex.API.Infrastructure.csproj
echo ""

echo "8️⃣ PROJECT REFERENCES (Web)"
echo "----------------------------"
grep "ProjectReference" src/Apex.API.Web/Apex.API.Web.csproj
echo ""

echo "9️⃣ PACKAGE REFERENCES (Infrastructure)"
echo "---------------------------------------"
grep "PackageReference" src/Apex.API.Infrastructure/Apex.API.Infrastructure.csproj
echo ""

echo "🔟 ALL CLASSES THAT INJECT APEXDBCONTEXT"
echo "-----------------------------------------"
grep -r "ApexDbContext" src/Apex.API.Infrastructure --include="*.cs" | grep "private readonly\|public.*("
echo ""

echo "1️⃣1️⃣ ALL CLASSES THAT INJECT IDBCONTEXTFACTORY"
echo "-----------------------------------------------"
grep -r "IDbContextFactory" src/Apex.API.Infrastructure --include="*.cs" | grep "private readonly\|public.*("
echo ""

echo "✅ DIAGNOSTIC COMPLETE!"
echo ""
echo "Please copy ALL of this output and share it!"
