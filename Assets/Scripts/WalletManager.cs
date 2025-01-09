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


    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        int totalCoins = GetTotalCoins() + amount;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(totalCoins);
        Debug.Log($"Added {amount} coins. Total: {totalCoins}");
    }


    public static bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;

        int totalCoins = GetTotalCoins();
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(totalCoins);
            Debug.Log($"Spent {amount} coins. Remaining: {totalCoins}");
            return true;
        }
        else
        {
            Debug.Log("Not enough coins to complete the transaction.");
            return false;
        }
    }


    public static void ResetCoins(int amount = 0)
    {
        amount = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(TotalCoinsKey, amount);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(amount);
        Debug.Log($"Wallet reset. Total coins: {amount}");
    }


    public static bool HasEnoughCoins(int amount)
    {
        return GetTotalCoins() >= amount;
    }
}
