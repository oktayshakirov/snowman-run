using UnityEngine;

[CreateAssetMenu(fileName = "New Booster", menuName = "Booster")]
public class BoosterData : ScriptableObject
{
    public string boosterName;
    public Sprite boosterImage;
    public int basePrice;
    public float upgradeIncrement;
    public int maxUpgrades;
    public BoosterType boosterType;
    public string description;

    public enum BoosterType
    {
        GogglesDuration,
        GogglesFogReduction,
        MaxSpeed,
        /// <summary>
        /// No Ads — use maxUpgrades = 1 for a single purchase.
        /// Suggested description text: "Removes the full-screen ad shown after game over, while keeping rewarded video ads available so you can still earn coins."
        /// </summary>
        NoAds
    }

    public string GetBoosterState(int upgradeLevel, bool isNext = false)
    {
        switch (boosterType)
        {
            case BoosterType.GogglesDuration:
                return isNext ? $"{5 + (5 * (upgradeLevel + 1))} seconds" : $"{5 + (5 * upgradeLevel)} seconds";
            case BoosterType.GogglesFogReduction:
                return isNext ? $"{50 + (10 * (upgradeLevel + 1))}%" : $"{50 + (10 * upgradeLevel)}%";
            case BoosterType.MaxSpeed:
                return isNext ? $"{40 + (5 * (upgradeLevel + 1))} km/h" : $"{40 + (5 * upgradeLevel)} km/h";
            case BoosterType.NoAds:
                return isNext
                    ? "Ads disabled"
                    : (upgradeLevel > 0 ? "Ads disabled" : "Ads enabled");
            default:
                return "Unknown";
        }
    }


}
