using UnityEngine;

public static class PracticeRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePracticeRuntimeModules()
    {
        PlayerHealth player = Object.FindObjectOfType<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerVision>() == null)
        {
            player.gameObject.AddComponent<PlayerVision>();
        }
    }
}
