using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireCooldown = 0.25f;
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int startingReserveAmmo = 96;
    [SerializeField] private float reloadDuration = 1.2f;

    private float fireTimer;
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private string statusText;
    private float statusTimer;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        currentAmmo = magazineSize;
        reserveAmmo = startingReserveAmmo;
    }

    private void Update()
    {
        if (PracticeRunGate.IsWaitingToStart)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDown)
        {
            return;
        }

        if (statusTimer > 0f)
        {
            statusTimer -= Time.deltaTime;
        }

        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryReload();
        }

        if (Input.GetMouseButton(0))
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (fireTimer > 0f || isReloading || muzzlePoint == null || projectilePrefab == null)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            ShowStatus(reserveAmmo > 0 ? "Empty! Right click reload" : "No Ammo");
            return;
        }

        GameObject projectileObject = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(this);
        }

        currentAmmo--;
        fireTimer = fireCooldown;
        PracticeFeedback.PlayShoot();
    }

    private void TryReload()
    {
        if (isReloading || currentAmmo >= magazineSize)
        {
            return;
        }

        if (reserveAmmo <= 0)
        {
            ShowStatus("No Ammo");
            return;
        }

        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay()
    {
        isReloading = true;
        ShowStatus("Reloading...");

        yield return new WaitForSeconds(reloadDuration);

        if (playerHealth != null && playerHealth.IsDown)
        {
            isReloading = false;
            yield break;
        }

        int neededAmmo = magazineSize - currentAmmo;
        int loadedAmmo = Mathf.Min(neededAmmo, reserveAmmo);
        currentAmmo += loadedAmmo;
        reserveAmmo -= loadedAmmo;
        isReloading = false;
        ShowStatus("Reloaded");
        PracticeFeedback.PlayReload();
    }

    public bool TryFillReserveAmmo()
    {
        if (reserveAmmo >= startingReserveAmmo)
        {
            ShowStatus("Ammo Full");
            return false;
        }

        reserveAmmo = startingReserveAmmo;
        ShowStatus("Ammo Refilled");
        return true;
    }

    private void ShowStatus(string text)
    {
        statusText = text;
        statusTimer = 1.1f;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 220f, 20f, 200f, 28f), $"Ammo: {currentAmmo} / {reserveAmmo}");
        GUI.Label(new Rect(Screen.width - 220f, 48f, 200f, 28f), "Right click reload");

        if (isReloading)
        {
            GUI.Label(new Rect(Screen.width - 220f, 76f, 200f, 28f), "Reloading...");
        }
        else if (statusTimer > 0f && !string.IsNullOrEmpty(statusText))
        {
            GUI.Label(new Rect(Screen.width - 220f, 76f, 220f, 28f), statusText);
        }
    }
}
