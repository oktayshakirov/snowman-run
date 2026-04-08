using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grants wallet coins for consumable purchases using RevenueCat transaction IDs (deduped in PlayerPrefs).
/// </summary>
public static class CoinPurchaseGrant
{
    private const string PrefsKey = "RcGrantedConsumableTxIds";

    public static void ProcessCustomerInfo(Purchases.CustomerInfo customerInfo)
    {
        if (customerInfo?.NonSubscriptionTransactions == null)
            return;

        HashSet<string> granted = LoadGranted();
        bool changed = false;

        foreach (Purchases.StoreTransaction tx in customerInfo.NonSubscriptionTransactions)
        {
            if (tx == null || string.IsNullOrEmpty(tx.TransactionIdentifier))
                continue;
            if (granted.Contains(tx.TransactionIdentifier))
                continue;
            if (!CoinIapCatalog.TryGetCoins(tx.ProductIdentifier, out int coins))
                continue;

            WalletManager.AddCoins(coins);
            granted.Add(tx.TransactionIdentifier);
            changed = true;
            Debug.Log($"[CoinIAP] Granted {coins} coins for product {tx.ProductIdentifier} (tx {tx.TransactionIdentifier}).");
        }

        if (changed)
            SaveGranted(granted);
    }

    private static HashSet<string> LoadGranted()
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        string raw = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(raw))
            return set;
        foreach (string part in raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            set.Add(part);
        return set;
    }

    private static void SaveGranted(HashSet<string> granted)
    {
        PlayerPrefs.SetString(PrefsKey, string.Join("|", granted));
        PlayerPrefs.Save();
    }
}
