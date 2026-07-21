using System.Collections;
using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float respawnDelay = 1.2f;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hitMaterial;
    [SerializeField] private GameObject ammoDropPrefab;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.4f, 0f);
    [SerializeField] private bool randomizeRespawnPosition = true;
    [SerializeField] private Vector2 respawnRoomHalfSize = new Vector2(23f, 23f);
    [SerializeField] private float minimumRespawnDistance = 4f;
    [SerializeField] private int respawnPlacementAttempts = 40;

    private int currentHealth;
    private Renderer targetRenderer;
    private Collider targetCollider;
    private Vector3 spawnPosition;
    private Vector3 originalScale;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
        targetCollider = GetComponent<Collider>();
        spawnPosition = transform.position;
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    public void TakeHit()
    {
        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth--;

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(PlayHitFeedback());

        if (currentHealth <= 0)
        {
            DropAmmoPickup();
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator PlayHitFeedback()
    {
        if (targetRenderer != null && hitMaterial != null)
        {
            targetRenderer.sharedMaterial = hitMaterial;
        }

        transform.localScale = originalScale * 1.25f;
        yield return new WaitForSeconds(0.16f);
        transform.localScale = originalScale;

        if (currentHealth > 0 && targetRenderer != null && normalMaterial != null)
        {
            targetRenderer.sharedMaterial = normalMaterial;
        }

        feedbackRoutine = null;
    }

    private IEnumerator RespawnAfterDelay()
    {
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        if (targetRenderer != null)
        {
            targetRenderer.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        currentHealth = maxHealth;
        transform.position = randomizeRespawnPosition ? FindRespawnPosition() : spawnPosition;
        transform.localScale = originalScale;

        if (targetRenderer != null)
        {
            targetRenderer.sharedMaterial = normalMaterial;
            targetRenderer.enabled = true;
        }

        if (targetCollider != null)
        {
            targetCollider.enabled = true;
        }
    }

    private void DropAmmoPickup()
    {
        if (ammoDropPrefab == null)
        {
            return;
        }

        Instantiate(ammoDropPrefab, transform.position + dropOffset, Quaternion.identity);
    }

    private Vector3 FindRespawnPosition()
    {
        for (int i = 0; i < respawnPlacementAttempts; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(-respawnRoomHalfSize.x, respawnRoomHalfSize.x),
                spawnPosition.y,
                Random.Range(-respawnRoomHalfSize.y, respawnRoomHalfSize.y)
            );

            if (IsRespawnPositionClear(candidate))
            {
                return candidate;
            }
        }

        return spawnPosition;
    }

    private bool IsRespawnPositionClear(Vector3 candidate)
    {
        Collider[] hits = Physics.OverlapBox(
            candidate,
            new Vector3(1.25f, 1f, 1.25f),
            Quaternion.identity
        );

        foreach (Collider hit in hits)
        {
            if (hit == null || hit == targetCollider || ShouldIgnorePlacementCollider(hit))
            {
                continue;
            }

            return false;
        }

        TargetDummy[] targets = FindObjectsOfType<TargetDummy>();
        foreach (TargetDummy target in targets)
        {
            if (target == this || !target.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 delta = target.transform.position - candidate;
            delta.y = 0f;
            if (delta.sqrMagnitude < minimumRespawnDistance * minimumRespawnDistance)
            {
                return false;
            }
        }

        return true;
    }

    private bool ShouldIgnorePlacementCollider(Collider hit)
    {
        Transform hitTransform = hit.transform;
        return hitTransform.name == "Floor"
            || hitTransform.name.StartsWith("Grid")
            || hitTransform.GetComponentInParent<Projectile>() != null;
    }
}
