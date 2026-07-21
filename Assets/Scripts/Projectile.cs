using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private float spawnHitRadius = 0.45f;

    private float lifeTimer;
    private Vector3 previousPosition;
    private PlayerShooting owner;
    private bool checkedSpawnOverlap;
    private Collider projectileCollider;

    public void Initialize(PlayerShooting projectileOwner)
    {
        owner = projectileOwner;
        previousPosition = transform.position;
    }

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (!checkedSpawnOverlap)
        {
            checkedSpawnOverlap = true;
            if (TryDamageOverlappingTarget())
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 travel = transform.forward * speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + travel;
        RaycastHit[] hits = Physics.SphereCastAll(previousPosition, hitRadius, transform.forward, travel.magnitude);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (IsSelfCollider(hit.collider))
            {
                continue;
            }

            if (IsOwnerCollider(hit.collider))
            {
                continue;
            }

            if (hit.collider.GetComponentInParent<PickupItem>() != null)
            {
                continue;
            }

            if (TryDamageCollider(hit.collider))
            {
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject);
            return;
        }

        transform.position = nextPosition;
        previousPosition = transform.position;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private bool IsOwnerCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }

        PlayerShooting hitOwner = hitCollider.GetComponentInParent<PlayerShooting>();
        return hitOwner != null && hitOwner == owner;
    }

    private bool TryDamageOverlappingTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, spawnHitRadius);
        foreach (Collider hit in hits)
        {
            if (IsSelfCollider(hit) || IsOwnerCollider(hit) || hit.GetComponentInParent<PickupItem>() != null)
            {
                continue;
            }

            if (TryDamageCollider(hit))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryDamageCollider(Collider hitCollider)
    {
        TargetDummy targetDummy = hitCollider.GetComponentInParent<TargetDummy>();
        if (targetDummy != null)
        {
            targetDummy.TakeHit();
            return true;
        }

        SimpleEnemy simpleEnemy = hitCollider.GetComponentInParent<SimpleEnemy>();
        if (simpleEnemy != null && !simpleEnemy.IsDefeated)
        {
            simpleEnemy.TakeDamage(1);
            return true;
        }

        return false;
    }

    private bool IsSelfCollider(Collider hitCollider)
    {
        return hitCollider != null
            && (hitCollider == projectileCollider || hitCollider.GetComponentInParent<Projectile>() == this);
    }
}
