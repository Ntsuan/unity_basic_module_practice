using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hurtMaterial;
    [SerializeField] private Material downMaterial;
    [SerializeField] private float invulnerabilityDuration = 0.45f;
    [SerializeField] private float knockbackDistance = 1.1f;
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float screenFlashDuration = 0.22f;
    [SerializeField] private Color screenFlashColor = new Color(1f, 0f, 0f, 0.28f);

    private int currentHealth;
    private Renderer playerRenderer;
    private CharacterController characterController;
    private Coroutine hurtRoutine;
    private Coroutine knockbackRoutine;
    private float invulnerabilityTimer;
    private float screenFlashTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDown => currentHealth <= 0;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerRenderer = GetComponent<Renderer>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (PracticeRunGate.IsWaitingToStart)
        {
            return;
        }

        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        if (screenFlashTimer > 0f)
        {
            screenFlashTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(int amount, Transform damageSource)
    {
        if (IsDown || amount <= 0)
        {
            return;
        }

        if (IsInvulnerable)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerabilityTimer = invulnerabilityDuration;
        screenFlashTimer = screenFlashDuration;
        PracticeFeedback.PlayHurt();

        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
        }

        if (IsDown)
        {
            StopKnockback();
            SetMaterial(downMaterial);
            return;
        }

        PlayKnockback(damageSource);
        hurtRoutine = StartCoroutine(PlayHurtFeedback());
    }

    public bool TryHeal(int amount)
    {
        if (IsDown || amount <= 0 || currentHealth >= maxHealth)
        {
            return false;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        SetMaterial(normalMaterial);
        return true;
    }

    private void PlayKnockback(Transform damageSource)
    {
        if (damageSource == null || characterController == null || !characterController.enabled)
        {
            return;
        }

        Vector3 direction = transform.position - damageSource.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -transform.forward;
        }

        StopKnockback();
        knockbackRoutine = StartCoroutine(ApplyKnockback(direction.normalized));
    }

    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        float elapsed = 0f;

        while (elapsed < knockbackDuration && !IsDown)
        {
            float step = knockbackDistance * Time.deltaTime / knockbackDuration;
            characterController.Move(direction * step);
            elapsed += Time.deltaTime;
            yield return null;
        }

        knockbackRoutine = null;
    }

    private void StopKnockback()
    {
        if (knockbackRoutine == null)
        {
            return;
        }

        StopCoroutine(knockbackRoutine);
        knockbackRoutine = null;
    }

    private IEnumerator PlayHurtFeedback()
    {
        SetMaterial(hurtMaterial);
        yield return new WaitForSeconds(0.18f);
        SetMaterial(normalMaterial);
        hurtRoutine = null;
    }

    private void SetMaterial(Material material)
    {
        if (playerRenderer != null && material != null)
        {
            playerRenderer.sharedMaterial = material;
        }
    }

    private void OnGUI()
    {
        if (screenFlashTimer > 0f)
        {
            float alpha = screenFlashColor.a * Mathf.Clamp01(screenFlashTimer / screenFlashDuration);
            Color previousColor = GUI.color;
            GUI.color = new Color(screenFlashColor.r, screenFlashColor.g, screenFlashColor.b, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        GUI.Label(new Rect(20f, 76f, 220f, 28f), $"HP: {currentHealth} / {maxHealth}");
        GUI.Label(new Rect(20f, 104f, 320f, 28f), "Press H to take 1 damage");

        if (IsInvulnerable && !IsDown)
        {
            GUI.Label(new Rect(20f, 132f, 220f, 28f), "Invulnerable");
        }

        if (IsDown)
        {
            GUI.Label(new Rect(20f, 160f, 220f, 28f), "Player Down");
        }
    }
}
