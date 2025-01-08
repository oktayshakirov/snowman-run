using UnityEngine;
using UnityEngine.UI;

public class ShopCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text itemNameText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Text actionButtonText;

    private string itemName;
    private string actionType;

    public void SetupCard(string name, string action)
    {
        itemName = name;
        actionType = action;
        itemNameText.text = name;
        actionButtonText.text = action;

        actionButton.onClick.AddListener(() => PerformAction());
    }

    private void PerformAction()
    {
        if (actionType == "Upgrade")
        {
            Debug.Log($"Upgraded {itemName}");
            // Logic for upgrading the booster
        }
        else if (actionType == "Buy")
        {
            Debug.Log($"Bought {itemName}");
            // Logic for purchasing the item
        }
    }
}
