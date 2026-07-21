using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float groundY = 0f;

    private Plane aimPlane;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        aimPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
    }

    private void Update()
    {
        if (PracticeRunGate.IsWaitingToStart)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDown)
        {
            return;
        }

        if (aimCamera == null)
        {
            return;
        }

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (!aimPlane.Raycast(ray, out float distance))
        {
            return;
        }

        Vector3 aimPoint = ray.GetPoint(distance);
        Vector3 lookDirection = aimPoint - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }
}
