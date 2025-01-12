using UnityEngine;
using System;

public static class WalletManager
{
    private const string TotalCoinsKey = "TotalCoins";
    public static event Action<int> OnCoinsChanged;

    public static int GetTotalCoins()
    {
        return PlayerPrefs.GetInt(TotalCoinsKey, 0);
    }

    public static void AddCoins(int amount, Action<int> callback = null)
    {
        if (amount <= 0) return;
        int totalCoins = GetTotalCoins() + amount;
        SaveCoins(totalCoins, $"Added {amount} coins. Total: {totalCoins}", callback);
    }

    public static bool SpendCoins(int amount, Action<int> callback = null)
    {
        if (amount <= 0) return false;

        int totalCoins = GetTotalCoins();
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            SaveCoins(totalCoins, $"Spent {amount} coins. Remaining: {totalCoins}", callback);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coins to complete the transaction.");
            return false;
        }
    }

    public static void ResetCoins(int amount = 0, Action<int> callback = null)
    {
        amount = Mathf.Max(0, amount);
        SaveCoins(amount, $"Wallet reset. Total coins: {amount}", callback);
    }

    public static bool HasEnoughCoins(int amount)
    {
        return GetTotalCoins() >= amount;
    }

    private static void SaveCoins(int amount, string debugMessage, Action<int> callback = null)
    {
        PlayerPrefs.SetInt(TotalCoinsKey, amount);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(amount);
        callback?.Invoke(amount);
        Debug.Log(debugMessage);
    }
}
