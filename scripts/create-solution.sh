#!/bin/bash
# Script to create Apex.API.sln and add all projects

echo "🔧 Creating Apex.API solution file..."

# Create new solution file
dotnet new sln -n Apex.API --force

echo "✅ Solution file created"
echo ""
echo "📦 Adding projects to solution..."

# Add Core project
echo "  Adding Apex.API.Core..."
dotnet sln Apex.API.sln add src/Apex.API.Core/Apex.API.Core.csproj

# Add UseCases project
echo "  Adding Apex.API.UseCases..."
dotnet sln Apex.API.sln add src/Apex.API.UseCases/Apex.API.UseCases.csproj

# Add Infrastructure project
echo "  Adding Apex.API.Infrastructure..."
dotnet sln Apex.API.sln add src/Apex.API.Infrastructure/Apex.API.Infrastructure.csproj

# Add Web project
echo "  Adding Apex.API.Web..."
dotnet sln Apex.API.sln add src/Apex.API.Web/Apex.API.Web.csproj

# Add ServiceDefaults project
echo "  Adding Apex.API.ServiceDefaults..."
dotnet sln Apex.API.sln add src/Apex.API.ServiceDefaults/Apex.API.ServiceDefaults.csproj

# Add AspireHost project (if exists)
if [ -d "src/Apex.API.AspireHost" ]; then
    echo "  Adding Apex.API.AspireHost..."
    dotnet sln Apex.API.sln add src/Apex.API.AspireHost/Apex.API.AspireHost.csproj
fi

echo ""
echo "🧪 Adding test projects..."

# Add UnitTests
echo "  Adding Apex.API.UnitTests..."
dotnet sln Apex.API.sln add tests/Apex.API.UnitTests/Apex.API.UnitTests.csproj

# Add IntegrationTests
if [ -d "tests/Apex.API.IntegrationTests" ]; then
    echo "  Adding Apex.API.IntegrationTests..."
    dotnet sln Apex.API.sln add tests/Apex.API.IntegrationTests/Apex.API.IntegrationTests.csproj
fi

# Add FunctionalTests
if [ -d "tests/Apex.API.FunctionalTests" ]; then
    echo "  Adding Apex.API.FunctionalTests..."
    dotnet sln Apex.API.sln add tests/Apex.API.FunctionalTests/Apex.API.FunctionalTests.csproj
fi

echo ""
echo "✅ Solution created successfully!"
echo ""
echo "📋 Solution contains:"
dotnet sln Apex.API.sln list

echo ""
echo "🎯 Next steps:"
echo "1. Close VS Code"
echo "2. Run: code ."
echo "3. VS Code will now use Apex.API.sln"
echo ""
