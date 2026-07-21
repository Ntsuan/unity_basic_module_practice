using System.Collections.Generic;
using UnityEngine;

public class RandomObstacleSpawner : MonoBehaviour
{
    [SerializeField] private Material obstacleMaterial;
    [SerializeField] private int obstacleCount = 8;
    [SerializeField] private Vector2 roomHalfSize = new Vector2(24f, 24f);
    [SerializeField] private float minimumDistance = 5f;

    private readonly List<Vector3> reservedPositions = new List<Vector3>();

    private void Start()
    {
        SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        ClearExistingObstacles();
        CacheReservedPositions();

        int spawnedCount = 0;
        int attempts = 0;
        while (spawnedCount < obstacleCount && attempts < obstacleCount * 30)
        {
            attempts++;

            Vector3 position = new Vector3(
                Random.Range(-roomHalfSize.x, roomHalfSize.x),
                0.55f,
                Random.Range(-roomHalfSize.y, roomHalfSize.y)
            );

            if (!IsPositionClear(position))
            {
                continue;
            }

            Vector3 size = new Vector3(
                Random.Range(1.4f, 3.4f),
                1.1f,
                Random.Range(1.4f, 4.2f)
            );

            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = $"Random Obstacle {spawnedCount + 1}";
            obstacle.transform.SetParent(transform);
            obstacle.transform.position = position;
            obstacle.transform.localScale = size;
            obstacle.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 180f), 0f);

            Renderer obstacleRenderer = obstacle.GetComponent<Renderer>();
            if (obstacleRenderer != null && obstacleMaterial != null)
            {
                obstacleRenderer.sharedMaterial = obstacleMaterial;
            }

            reservedPositions.Add(position);
            spawnedCount++;
        }
    }

    private void ClearExistingObstacles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void CacheReservedPositions()
    {
        reservedPositions.Clear();

        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            reservedPositions.Add(player.transform.position);
        }

        SimpleEnemy[] enemies = FindObjectsOfType<SimpleEnemy>();
        foreach (SimpleEnemy enemy in enemies)
        {
            reservedPositions.Add(enemy.transform.position);
        }

        TargetDummy[] targets = FindObjectsOfType<TargetDummy>();
        foreach (TargetDummy target in targets)
        {
            reservedPositions.Add(target.transform.position);
        }
    }

    private bool IsPositionClear(Vector3 position)
    {
        foreach (Vector3 reservedPosition in reservedPositions)
        {
            Vector3 delta = reservedPosition - position;
            delta.y = 0f;
            if (delta.sqrMagnitude < minimumDistance * minimumDistance)
            {
                return false;
            }
        }

        return true;
    }
}
