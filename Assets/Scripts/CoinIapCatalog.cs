using System.Collections.Generic;

/// <summary>
/// Maps RevenueCat / store product identifiers to in-game coin amounts.
/// </summary>
public static class CoinIapCatalog
{
    public const string Product2500 = "snowmanrun.2500";
    public const string Product5000 = "snowmanrun.5000";
    public const string Product10000 = "snowmanrun.10000";

    private static readonly Dictionary<string, int> CoinsByProductId = new Dictionary<string, int>
    {
        { Product2500, 2500 },
        { Product5000, 5000 },
        { Product10000, 10000 }
    };

    public static readonly string[] ProductIds =
    {
        Product2500,
        Product5000,
        Product10000
    };

    public static bool TryGetCoins(string productIdentifier, out int coins)
    {
        return CoinsByProductId.TryGetValue(productIdentifier, out coins);
    }
}
