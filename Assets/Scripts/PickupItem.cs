using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupKind
    {
        Loot,
        Health,
        Ammo
    }

    [SerializeField] private PickupKind kind = PickupKind.Loot;
    [SerializeField] private string displayName = "Loot";
    [SerializeField] private int amount = 1;

    public PickupKind Kind => kind;
    public string DisplayName => displayName;
    public int Amount => amount;

    private void Awake()
    {
        Collider pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    public bool TryCollect(PlayerPickup playerPickup, PlayerHealth playerHealth, PlayerShooting playerShooting)
    {
        bool collected = kind switch
        {
            PickupKind.Health => playerHealth != null && playerHealth.TryHeal(amount),
            PickupKind.Ammo => playerShooting != null && playerShooting.TryFillReserveAmmo(),
            _ => playerPickup != null && playerPickup.TryCollectLoot(amount)
        };

        if (!collected)
        {
            return false;
        }

        if (playerPickup != null)
        {
            playerPickup.ShowPickupStatus(GetCollectedText());
        }

        PracticeFeedback.PlayPickup();
        gameObject.SetActive(false);
        return true;
    }

    private string GetCollectedText()
    {
        return kind switch
        {
            PickupKind.Health => $"HP +{amount}",
            PickupKind.Ammo => "Ammo Refilled",
            _ => $"{displayName} +{Mathf.Max(1, amount)}"
        };
    }
}
