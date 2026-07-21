using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private float pickupRadius = 1.8f;

    private int collectedCount;
    private PickupItem nearbyPickup;
    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;
    private string statusText;
    private float statusTimer;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerShooting = GetComponent<PlayerShooting>();
    }

    private void Update()
    {
        if (PracticeRunGate.IsWaitingToStart)
        {
            nearbyPickup = null;
            return;
        }

        if (playerHealth != null && playerHealth.IsDown)
        {
            nearbyPickup = null;
            return;
        }

        if (statusTimer > 0f)
        {
            statusTimer -= Time.deltaTime;
        }

        nearbyPickup = FindNearbyPickup();

        if (nearbyPickup != null)
        {
            if (!nearbyPickup.TryCollect(this, playerHealth, playerShooting))
            {
                ShowPickupStatus(GetBlockedText(nearbyPickup));
            }

            nearbyPickup = null;
        }
    }

    public bool TryCollectLoot(int amount)
    {
        collectedCount += Mathf.Max(1, amount);
        ShowPickupStatus("Loot +1");
        return true;
    }

    private PickupItem FindNearbyPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
        PickupItem closestPickup = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider hit in hits)
        {
            PickupItem pickup = hit.GetComponentInParent<PickupItem>();
            if (pickup == null || !pickup.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distanceSqr = (pickup.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestPickup = pickup;
                closestDistanceSqr = distanceSqr;
            }
        }

        return closestPickup;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20f, 20f, 220f, 28f), $"Loot: {collectedCount}");

        if (statusTimer > 0f && !string.IsNullOrEmpty(statusText))
        {
            GUI.Label(new Rect(20f, 48f, 320f, 28f), statusText);
        }
    }

    private string GetBlockedText(PickupItem pickup)
    {
        return pickup.Kind switch
        {
            PickupItem.PickupKind.Health => "HP Full",
            PickupItem.PickupKind.Ammo => "Ammo Full",
            _ => $"{pickup.DisplayName} blocked"
        };
    }

    public void ShowPickupStatus(string text)
    {
        statusText = text;
        statusTimer = 1.1f;
    }
}
