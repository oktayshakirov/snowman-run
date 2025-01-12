using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [Header("Customization Slots")]
    [SerializeField] private Transform hatSlot; // Position for the hat
    [SerializeField] private Transform rideSlot; // Position for the ride

    private GameObject currentHat;
    private GameObject currentRide;

    public void EquipHat(GameObject hatPrefab)
    {
        if (currentHat != null)
        {
            Destroy(currentHat); // Remove the old hat
        }

        if (hatPrefab != null)
        {
            currentHat = Instantiate(hatPrefab, hatSlot);
            currentHat.transform.localPosition = Vector3.zero; // Adjust position
            currentHat.transform.localRotation = Quaternion.identity;
        }
    }

    public void EquipRide(GameObject ridePrefab)
    {
        if (currentRide != null)
        {
            Destroy(currentRide); // Remove the old ride
        }

        if (ridePrefab != null)
        {
            currentRide = Instantiate(ridePrefab, rideSlot);
            currentRide.transform.localPosition = Vector3.zero; // Adjust position
            currentRide.transform.localRotation = Quaternion.identity;
        }
    }
}