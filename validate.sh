#!/bin/bash
set -e

echo "=========================================="
echo "  LiquorPOS - Validation Script"
echo "=========================================="
echo ""

echo "✓ Checking .NET SDK..."
dotnet --version

echo ""
echo "✓ Checking Node.js..."
node --version

echo ""
echo "✓ Building .NET solution..."
dotnet build

echo ""
echo "✓ Running .NET tests..."
dotnet test --no-build --verbosity quiet

echo ""
echo "✓ Building frontend..."
cd src/Frontend
npm run build > /dev/null 2>&1

echo ""
echo "✓ Running frontend tests..."
npm run test -- --run > /dev/null 2>&1

cd ../..

echo ""
echo "✓ Validating Docker Compose configuration..."
docker compose config > /dev/null 2>&1

echo ""
echo "=========================================="
echo "  ✓ All validations passed!"
echo "=========================================="
echo ""
echo "Summary:"
echo "  - Solution builds successfully"
echo "  - All .NET tests pass"
echo "  - Frontend builds successfully"
echo "  - All frontend tests pass"
echo "  - Docker Compose configuration is valid"
echo ""
echo "To start the services:"
echo "  docker compose up --build"
echo ""
