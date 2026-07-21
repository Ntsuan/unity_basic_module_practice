using UnityEngine;

public class PracticeRunGate : MonoBehaviour
{
    [SerializeField] private string buttonText = "Start Debug Run";

    private GUIStyle buttonStyle;

    public static bool IsWaitingToStart { get; private set; }

    private void Awake()
    {
        IsWaitingToStart = true;
        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (!IsWaitingToStart)
        {
            return;
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
        }

        const float width = 260f;
        const float height = 56f;
        Rect buttonRect = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        if (GUI.Button(buttonRect, buttonText, buttonStyle))
        {
            IsWaitingToStart = false;
            Time.timeScale = 1f;
        }
    }

    private void OnDisable()
    {
        IsWaitingToStart = false;
        Time.timeScale = 1f;
    }
}
