using UnityEngine;

public static class WalletManager
{
    private const string TotalCoinsKey = "TotalCoins";

    public static int GetTotalCoins()
    {
        return PlayerPrefs.GetInt(TotalCoinsKey, 0);
    }

    public static void AddCoins(int amount)
    {
        int totalCoins = GetTotalCoins() + amount;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
    }
}