using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleEnemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackWindupDuration = 0.35f;
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform target;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hitMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material visionHighlightedMaterial;
    [SerializeField] private Material attackWarningMaterial;
    [SerializeField] private Transform attackRangeIndicator;
    [SerializeField] private GameObject healthDropPrefab;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.65f, 0f);
    [SerializeField] private float obstacleProbeDistance = 1.2f;
    [SerializeField] private float obstacleProbeRadius = 0.45f;
    [SerializeField] private float detourAngle = 55f;
    [SerializeField] private float blockedRetryDelay = 0.15f;
    [SerializeField] private float deathShrinkDuration = 0.22f;
    [SerializeField] private float respawnWarningDuration = 0.55f;
    [SerializeField] private float respawnPopDuration = 0.22f;
    [SerializeField] private float respawnIndicatorWorldRadius = 2.2f;
    [SerializeField] private Transform respawnIndicator;

    private int currentHealth;
    private float attackTimer;
    private float attackWindupTimer;
    private float blockedRetryTimer;
    private bool isSelected;
    private bool isVisionHighlighted;
    private bool isShowingHitFeedback;
    private bool isPreparingAttack;
    private Renderer enemyRenderer;
    private Collider enemyCollider;
    private CharacterController characterController;
    private PlayerHealth playerHealth;
    private Vector3 spawnPosition;
    private Vector3 originalScale;
    private Coroutine feedbackRoutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDefeated => currentHealth <= 0;

    public void SetVisionHighlighted(bool highlighted)
    {
        if (isVisionHighlighted == highlighted)
        {
            return;
        }

        isVisionHighlighted = highlighted;

        if (!isShowingHitFeedback)
        {
            ApplyCurrentMaterial();
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponent<Renderer>();
        enemyCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
        spawnPosition = transform.position;
        originalScale = transform.localScale;

        if (attackWarningMaterial == null)
        {
            attackWarningMaterial = CreateRuntimeMaterial("Runtime Enemy Attack Warning", new Color(1f, 0.25f, 0.12f));
        }

        if (visionHighlightedMaterial == null)
        {
            visionHighlightedMaterial = CreateRuntimeMaterial("Runtime Enemy Vision Highlight", new Color(0.15f, 1f, 0.82f));
        }

        if (attackRangeIndicator == null)
        {
            Transform foundIndicator = transform.Find("Attack Range Indicator");
            if (foundIndicator != null)
            {
                attackRangeIndicator = foundIndicator;
            }
        }

        if (attackRangeIndicator == null)
        {
            attackRangeIndicator = CreateAttackRangeIndicator();
        }

        SetAttackRangeIndicatorVisible(false);

        if (respawnIndicator == null)
        {
            Transform foundRespawnIndicator = transform.Find("Respawn Indicator");
            if (foundRespawnIndicator != null)
            {
                respawnIndicator = foundRespawnIndicator;
            }
        }

        if (respawnIndicator == null)
        {
            respawnIndicator = CreateRespawnIndicator();
        }

        SetRespawnIndicatorVisible(false);

        if (target == null)
        {
            PlayerHealth foundPlayer = FindObjectOfType<PlayerHealth>();
            if (foundPlayer != null)
            {
                target = foundPlayer.transform;
            }
        }

        if (target != null)
        {
            playerHealth = target.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (PracticeRunGate.IsWaitingToStart)
        {
            CancelAttackWindup();
            return;
        }

        UpdateSelection();

        if (IsDefeated || target == null || playerHealth == null || playerHealth.IsDown)
        {
            CancelAttackWindup();
            SetVisionHighlighted(false);
            return;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (blockedRetryTimer > 0f)
        {
            blockedRetryTimer -= Time.deltaTime;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        float distance = toTarget.magnitude;
        if (distance > attackRange)
        {
            CancelAttackWindup();
            MoveTowardTarget(toTarget.normalized);
            return;
        }

        if (isPreparingAttack)
        {
            UpdateAttackWindup(distance);
            return;
        }

        if (attackTimer <= 0f)
        {
            BeginAttackWindup();
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDefeated || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        if (IsDefeated)
        {
            CancelAttackWindup();
            PracticeFeedback.PlayEnemyDefeated();
            DropHealthPickup();
            StartCoroutine(RespawnAfterDelay());
            return;
        }

        feedbackRoutine = StartCoroutine(PlayHitFeedback());
    }

    private IEnumerator PlayHitFeedback()
    {
        isShowingHitFeedback = true;
        SetMaterial(hitMaterial);
        yield return new WaitForSeconds(0.16f);
        isShowingHitFeedback = false;
        ApplyCurrentMaterial();
        feedbackRoutine = null;
    }

    private IEnumerator RespawnAfterDelay()
    {
        CancelAttackWindup();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (enemyRenderer != null)
        {
            SetMaterial(hitMaterial);
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 hiddenScale = originalScale * 0.18f;
            while (elapsed < deathShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / deathShrinkDuration);
                transform.localScale = Vector3.Lerp(startScale, hiddenScale, t);
                yield return null;
            }

            enemyRenderer.enabled = false;
        }

        float quietDelay = Mathf.Max(0f, respawnDelay - respawnWarningDuration);
        yield return new WaitForSeconds(quietDelay);

        transform.position = spawnPosition;
        transform.localScale = originalScale * 0.22f;
        SetRespawnIndicatorVisible(true);
        yield return new WaitForSeconds(respawnWarningDuration);
        SetRespawnIndicatorVisible(false);

        currentHealth = maxHealth;
        attackTimer = attackCooldown;
        attackWindupTimer = 0f;
        isPreparingAttack = false;
        PracticeFeedback.PlayEnemyRespawn();

        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = true;
            ApplyCurrentMaterial();
        }

        float popElapsed = 0f;
        Vector3 popStartScale = transform.localScale;
        while (popElapsed < respawnPopDuration)
        {
            popElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(popElapsed / respawnPopDuration);
            float overshoot = Mathf.Sin(t * Mathf.PI) * 0.12f;
            transform.localScale = Vector3.Lerp(popStartScale, originalScale, t) + originalScale * overshoot;
            yield return null;
        }

        transform.localScale = originalScale;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
    }

    private void SetMaterial(Material material)
    {
        if (enemyRenderer != null && material != null)
        {
            enemyRenderer.sharedMaterial = material;
        }
    }

    private void UpdateSelection()
    {
        bool wasSelected = isSelected;
        isSelected = false;

        if (!IsDefeated && Camera.main != null && enemyCollider != null && enemyCollider.enabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 200f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.GetComponentInParent<SimpleEnemy>() == this)
                {
                    isSelected = true;
                    break;
                }
            }
        }

        if (wasSelected != isSelected && !isShowingHitFeedback)
        {
            ApplyCurrentMaterial();
        }
    }

    private void ApplyCurrentMaterial()
    {
        if (isPreparingAttack && attackWarningMaterial != null)
        {
            SetMaterial(attackWarningMaterial);
            return;
        }

        if (isSelected && selectedMaterial != null)
        {
            SetMaterial(selectedMaterial);
            return;
        }

        SetMaterial(isVisionHighlighted && visionHighlightedMaterial != null ? visionHighlightedMaterial : normalMaterial);
    }

    private void BeginAttackWindup()
    {
        isPreparingAttack = true;
        attackWindupTimer = attackWindupDuration;
        SetAttackRangeIndicatorVisible(true);

        if (!isShowingHitFeedback)
        {
            ApplyCurrentMaterial();
        }
    }

    private void UpdateAttackWindup(float currentDistance)
    {
        attackWindupTimer -= Time.deltaTime;

        if (currentDistance > attackRange)
        {
            CancelAttackWindup();
            return;
        }

        if (attackWindupTimer > 0f)
        {
            return;
        }

        bool targetStillInRange = playerHealth != null
            && !playerHealth.IsDown
            && GetFlatDistanceToTarget() <= attackRange;

        if (targetStillInRange)
        {
            playerHealth.TakeDamage(attackDamage, transform);
        }

        isPreparingAttack = false;
        attackTimer = attackCooldown;
        SetAttackRangeIndicatorVisible(false);

        if (!isShowingHitFeedback)
        {
            ApplyCurrentMaterial();
        }
    }

    private float GetFlatDistanceToTarget()
    {
        if (target == null)
        {
            return float.PositiveInfinity;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        return toTarget.magnitude;
    }

    private void CancelAttackWindup()
    {
        if (!isPreparingAttack && attackRangeIndicator == null)
        {
            return;
        }

        bool wasPreparingAttack = isPreparingAttack;
        isPreparingAttack = false;
        attackWindupTimer = 0f;
        SetAttackRangeIndicatorVisible(false);

        if (wasPreparingAttack && !isShowingHitFeedback)
        {
            ApplyCurrentMaterial();
        }
    }

    private void SetAttackRangeIndicatorVisible(bool visible)
    {
        if (attackRangeIndicator == null)
        {
            return;
        }

        attackRangeIndicator.gameObject.SetActive(visible);
    }

    private Transform CreateAttackRangeIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "Attack Range Indicator";
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = new Vector3(0f, -0.76f, 0f);
        indicator.transform.localScale = new Vector3(attackRange * 2f, 0.01f, attackRange * 2f);

        Collider indicatorCollider = indicator.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            indicatorCollider.enabled = false;
            Destroy(indicatorCollider);
        }

        Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            indicatorRenderer.sharedMaterial = CreateTransparentRuntimeMaterial(
                "Runtime Enemy Attack Range",
                new Color(1f, 0.08f, 0.04f, 0.32f)
            );
        }

        return indicator.transform;
    }

    private Transform CreateRespawnIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "Respawn Indicator";
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = new Vector3(0f, -0.77f, 0f);
        indicator.transform.localScale = GetRespawnIndicatorLocalScale();

        Collider indicatorCollider = indicator.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            indicatorCollider.enabled = false;
            Destroy(indicatorCollider);
        }

        Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            indicatorRenderer.sharedMaterial = CreateTransparentRuntimeMaterial(
                "Runtime Enemy Respawn Warning",
                new Color(0.72f, 0.28f, 1f, 0.36f)
            );
        }

        return indicator.transform;
    }

    private void SetRespawnIndicatorVisible(bool visible)
    {
        if (respawnIndicator == null)
        {
            return;
        }

        if (visible)
        {
            respawnIndicator.localScale = GetRespawnIndicatorLocalScale();
        }

        respawnIndicator.gameObject.SetActive(visible);
    }

    private Vector3 GetRespawnIndicatorLocalScale()
    {
        Vector3 scale = transform.lossyScale;
        float safeX = Mathf.Max(Mathf.Abs(scale.x), 0.001f);
        float safeY = Mathf.Max(Mathf.Abs(scale.y), 0.001f);
        float safeZ = Mathf.Max(Mathf.Abs(scale.z), 0.001f);
        return new Vector3(
            respawnIndicatorWorldRadius / safeX,
            0.01f / safeY,
            respawnIndicatorWorldRadius / safeZ
        );
    }

    private Material CreateRuntimeMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = materialName;
        material.color = color;
        return material;
    }

    private Material CreateTransparentRuntimeMaterial(string materialName, Color color)
    {
        Material material = CreateRuntimeMaterial(materialName, color);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    private void DropHealthPickup()
    {
        if (healthDropPrefab == null)
        {
            return;
        }

        Instantiate(healthDropPrefab, transform.position + dropOffset, Quaternion.identity);
    }

    private void MoveTowardTarget(Vector3 moveDirection)
    {
        Vector3 chosenDirection = ChooseMoveDirection(moveDirection);
        if (chosenDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(chosenDirection, Vector3.up);

        Vector3 movement = chosenDirection * moveSpeed * Time.deltaTime;

        if (characterController != null && characterController.enabled)
        {
            characterController.Move(movement);
        }
        else
        {
            transform.position += movement;
        }

        Vector3 position = transform.position;
        position.y = spawnPosition.y;
        transform.position = position;
    }

    private Vector3 ChooseMoveDirection(Vector3 desiredDirection)
    {
        if (blockedRetryTimer > 0f)
        {
            return Vector3.zero;
        }

        if (!IsDirectionBlocked(desiredDirection))
        {
            return desiredDirection;
        }

        Vector3 leftDirection = Quaternion.Euler(0f, -detourAngle, 0f) * desiredDirection;
        Vector3 rightDirection = Quaternion.Euler(0f, detourAngle, 0f) * desiredDirection;

        bool leftBlocked = IsDirectionBlocked(leftDirection);
        bool rightBlocked = IsDirectionBlocked(rightDirection);

        if (!leftBlocked && !rightBlocked)
        {
            return IsDirectionCloserToTarget(leftDirection, rightDirection) ? leftDirection.normalized : rightDirection.normalized;
        }

        if (!leftBlocked)
        {
            return leftDirection.normalized;
        }

        if (!rightBlocked)
        {
            return rightDirection.normalized;
        }

        blockedRetryTimer = blockedRetryDelay;
        return Vector3.zero;
    }

    private bool IsDirectionCloserToTarget(Vector3 firstDirection, Vector3 secondDirection)
    {
        Vector3 firstPosition = transform.position + firstDirection.normalized;
        Vector3 secondPosition = transform.position + secondDirection.normalized;

        return (target.position - firstPosition).sqrMagnitude <= (target.position - secondPosition).sqrMagnitude;
    }

    private bool IsDirectionBlocked(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            obstacleProbeRadius,
            direction.normalized,
            obstacleProbeDistance
        );

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreObstacleProbe(hit.collider))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool ShouldIgnoreObstacleProbe(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return true;
        }

        if (hitCollider == enemyCollider || hitCollider.GetComponentInParent<SimpleEnemy>() == this)
        {
            return true;
        }

        if (hitCollider.GetComponentInParent<PlayerHealth>() != null)
        {
            return true;
        }

        if (hitCollider.GetComponentInParent<PickupItem>() != null)
        {
            return true;
        }

        if (hitCollider.GetComponentInParent<Projectile>() != null)
        {
            return true;
        }

        return hitCollider.transform.name == "Floor"
            || hitCollider.transform.name.StartsWith("Grid");
    }
}
