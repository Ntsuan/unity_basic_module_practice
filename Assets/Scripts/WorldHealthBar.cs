using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float worldHeightOffset = 2.75f;
    [SerializeField] private int width = 42;
    [SerializeField] private int height = 4;
    [SerializeField] private Color fillColor = new Color(0.2f, 1f, 0.3f);
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.8f);

    private PlayerHealth playerHealth;
    private SimpleEnemy simpleEnemy;
    private GUIStyle labelStyle;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }

        playerHealth = GetComponent<PlayerHealth>();
        simpleEnemy = GetComponent<SimpleEnemy>();
    }

    private void OnGUI()
    {
        if (target == null || Camera.main == null || !TryGetHealth(out int current, out int max))
        {
            return;
        }

        if (simpleEnemy != null && simpleEnemy.IsDefeated)
        {
            return;
        }

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(
            target.position + Vector3.up * worldHeightOffset
        );
        if (screenPosition.z <= 0f)
        {
            return;
        }

        float x = screenPosition.x - width * 0.5f;
        float y = Screen.height - screenPosition.y;
        float fillWidth = width * Mathf.Clamp01((float)current / max);

        Color previousColor = GUI.color;
        GUI.color = backgroundColor;
        GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(x, y, fillWidth, height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 8,
                normal = { textColor = Color.white }
            };
        }

        GUI.Label(new Rect(x - 5f, y - 12f, width + 10f, 11f), $"{current} / {max}", labelStyle);
    }

    private bool TryGetHealth(out int current, out int max)
    {
        if (playerHealth != null)
        {
            current = playerHealth.CurrentHealth;
            max = playerHealth.MaxHealth;
            return max > 0;
        }

        if (simpleEnemy != null)
        {
            current = simpleEnemy.CurrentHealth;
            max = simpleEnemy.MaxHealth;
            return max > 0;
        }

        current = 0;
        max = 0;
        return false;
    }
}
