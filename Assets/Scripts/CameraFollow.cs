using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(-16f, 36f, -16f);
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float orthographicSize = 20f;

    private Camera followCamera;

    private void Awake()
    {
        followCamera = GetComponent<Camera>();

        if (followCamera != null)
        {
            followCamera.orthographic = true;
            followCamera.orthographicSize = orthographicSize;
            followCamera.clearFlags = CameraClearFlags.SolidColor;
            followCamera.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (followCamera != null)
        {
            followCamera.orthographic = true;
            followCamera.orthographicSize = orthographicSize;
            followCamera.clearFlags = CameraClearFlags.SolidColor;
            followCamera.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
        }

        Vector3 targetPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
        transform.position = smoothedPosition + PracticeFeedback.GetCameraShakeOffset();

        transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
    }
}
