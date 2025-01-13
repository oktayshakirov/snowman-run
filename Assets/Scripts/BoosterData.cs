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

    public enum BoosterType
    {
        GogglesDuration,
        GogglesFogReduction,
        MaxSpeed
    }
}
