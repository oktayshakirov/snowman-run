using UnityEngine;
using System;

public class PlayerCustomization : MonoBehaviour
{
    [Header("Customization Slots")]
    [SerializeField] private Transform hatSlot;
    [SerializeField] private Transform gogglesSlot;
    [SerializeField] private Transform rideSlot;

    [Header("Default Items")]
    [SerializeField] private GameObject defaultHat;
    [SerializeField] private GameObject defaultGoggles;
    [SerializeField] private GameObject defaultRide;

    private GameObject currentHat;
    private GameObject currentGoggles;
    private GameObject currentRide;

    public static event Action<string, string, string> OnEquipmentChanged;

    public string CurrentHatName => currentHat != null ? currentHat.name : null;
    public string CurrentGogglesName => currentGoggles != null ? currentGoggles.name : null;
    public string CurrentRideName => currentRide != null ? currentRide.name : null;

    public void EquipHat(GameObject hat)
    {
        currentHat = hat ?? defaultHat;
        SetActiveItem(hatSlot, currentHat);
        SaveItem("SelectedHat", currentHat);
        OnEquipmentChanged?.Invoke(CurrentHatName, CurrentGogglesName, CurrentRideName);
    }

    public void EquipGoggles(GameObject goggles)
    {
        currentGoggles = goggles ?? defaultGoggles;
        SetActiveItem(gogglesSlot, currentGoggles);
        SaveItem("SelectedGoggles", currentGoggles);
        OnEquipmentChanged?.Invoke(CurrentHatName, CurrentGogglesName, CurrentRideName);
    }

    public void EquipRide(GameObject ride)
    {
        currentRide = ride ?? defaultRide;
        SetActiveItem(rideSlot, currentRide);
        SaveItem("SelectedRide", currentRide);
        OnEquipmentChanged?.Invoke(CurrentHatName, CurrentGogglesName, CurrentRideName);
    }

    public void SaveCustomization()
    {
        SaveItem("SelectedHat", currentHat);
        SaveItem("SelectedGoggles", currentGoggles);
        SaveItem("SelectedRide", currentRide);
    }

    public void LoadCustomization()
    {
        currentHat = FindChildByName(hatSlot, PlayerPrefs.GetString("SelectedHat", defaultHat.name)) ?? defaultHat;
        currentGoggles = FindChildByName(gogglesSlot, PlayerPrefs.GetString("SelectedGoggles", defaultGoggles.name)) ?? defaultGoggles;
        currentRide = FindChildByName(rideSlot, PlayerPrefs.GetString("SelectedRide", defaultRide.name)) ?? defaultRide;

        SetActiveItem(hatSlot, currentHat);
        SetActiveItem(gogglesSlot, currentGoggles);
        SetActiveItem(rideSlot, currentRide);

        OnEquipmentChanged?.Invoke(CurrentHatName, CurrentGogglesName, CurrentRideName);
    }


    private void SaveItem(string key, GameObject item)
    {
        if (item != null)
        {
            PlayerPrefs.SetString(key, item.name);
        }
        else
        {
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
    }

    private void SetActiveItem(Transform parent, GameObject itemToActivate)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(false);
        }

        if (itemToActivate != null)
        {
            itemToActivate.SetActive(true);
        }
    }

    private GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
