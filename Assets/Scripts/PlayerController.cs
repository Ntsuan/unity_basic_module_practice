using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float groundY = 1f;
    [SerializeField] private Vector2 roomBounds = new Vector2(27f, 27f);

    private CharacterController characterController;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
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

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 velocity = moveDirection * moveSpeed;
        characterController.Move(velocity * Time.deltaTime);

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -roomBounds.x, roomBounds.x);
        position.y = groundY;
        position.z = Mathf.Clamp(position.z, -roomBounds.y, roomBounds.y);
        transform.position = position;
    }
}
