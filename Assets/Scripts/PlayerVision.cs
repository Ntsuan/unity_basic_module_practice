using System.Collections.Generic;
using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [SerializeField] private float visionRadius = 13f;
    [SerializeField] private float visionAngle = 100f;
    [SerializeField] private int segmentCount = 56;
    [SerializeField] private float groundHeight = 0.04f;
    [SerializeField] private Color visionColor = new Color(0.05f, 0.95f, 0.85f, 0.2f);

    private readonly List<SimpleEnemy> highlightedEnemies = new List<SimpleEnemy>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh visionMesh;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        EnsureVisionVisual();
    }

    private void LateUpdate()
    {
        bool shouldShowVision = !PracticeRunGate.IsWaitingToStart
            && (playerHealth == null || !playerHealth.IsDown);

        if (meshRenderer != null)
        {
            meshRenderer.enabled = shouldShowVision;
        }

        ClearEnemyHighlights();

        if (!shouldShowVision)
        {
            return;
        }

        UpdateVisionMesh();
        UpdateEnemyHighlights();
    }

    private void EnsureVisionVisual()
    {
        Transform existing = transform.Find("Vision Cone");
        GameObject visionObject = existing != null ? existing.gameObject : new GameObject("Vision Cone");
        visionObject.transform.SetParent(transform);
        visionObject.transform.localPosition = Vector3.zero;
        visionObject.transform.localRotation = Quaternion.identity;

        meshFilter = visionObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = visionObject.AddComponent<MeshFilter>();
        }

        meshRenderer = visionObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = visionObject.AddComponent<MeshRenderer>();
        }

        Material material = new Material(Shader.Find("Standard"));
        material.name = "Runtime Player Vision";
        material.color = visionColor;
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        meshRenderer.sharedMaterial = material;

        visionMesh = new Mesh();
        visionMesh.name = "Player Vision Cone Mesh";
        meshFilter.sharedMesh = visionMesh;
    }

    private void UpdateVisionMesh()
    {
        if (visionMesh == null)
        {
            return;
        }

        int safeSegmentCount = Mathf.Max(3, segmentCount);
        Vector3[] vertices = new Vector3[safeSegmentCount + 2];
        int[] triangles = new int[safeSegmentCount * 3];

        vertices[0] = new Vector3(0f, -transform.position.y + groundHeight, 0f);

        float halfAngle = visionAngle * 0.5f;
        for (int i = 0; i <= safeSegmentCount; i++)
        {
            float t = i / (float)safeSegmentCount;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            vertices[i + 1] = vertices[0] + direction * visionRadius;
        }

        for (int i = 0; i < safeSegmentCount; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
        visionMesh.RecalculateBounds();
    }

    private void UpdateEnemyHighlights()
    {
        SimpleEnemy[] enemies = FindObjectsOfType<SimpleEnemy>();
        for (int i = 0; i < enemies.Length; i++)
        {
            SimpleEnemy enemy = enemies[i];
            if (enemy == null || enemy.IsDefeated)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - transform.position;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude > visionRadius * visionRadius || toEnemy.sqrMagnitude < 0.001f)
            {
                continue;
            }

            float angleToEnemy = Vector3.Angle(transform.forward, toEnemy.normalized);
            if (angleToEnemy > visionAngle * 0.5f)
            {
                continue;
            }

            enemy.SetVisionHighlighted(true);
            highlightedEnemies.Add(enemy);
        }
    }

    private void ClearEnemyHighlights()
    {
        for (int i = 0; i < highlightedEnemies.Count; i++)
        {
            if (highlightedEnemies[i] != null)
            {
                highlightedEnemies[i].SetVisionHighlighted(false);
            }
        }

        highlightedEnemies.Clear();
    }

    private void OnDisable()
    {
        ClearEnemyHighlights();
    }
}
