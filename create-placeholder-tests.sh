#!/bin/bash
set -e

SERVICES=("Identity" "Catalog" "InventoryPurchasing" "SalesPOS" "CustomerLoyalty" "ReportingAnalytics" "Configuration")

for SERVICE in "${SERVICES[@]}"; do
    cat > "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/PlaceholderTests.cs" << 'TESTFILE'
using Xunit;

namespace LiquorPOS.Services.SERVICE_NAME.UnitTests;

public class PlaceholderTests
{
    [Fact]
    public void Placeholder_Test_Should_Pass()
    {
        Assert.True(true);
    }
}
TESTFILE
    sed -i "s/SERVICE_NAME/$SERVICE/g" "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/PlaceholderTests.cs"
done

echo "Placeholder test files created!"
