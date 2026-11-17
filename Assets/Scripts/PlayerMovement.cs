using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxGroundSpeed = 7f;
    public float groundAcceleration = 40f;
    public float groundFriction = 8f;

    [Header("Air Movement")]
    public float airAcceleration = 12f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    [Header("Lake")]
    [Range(0f, 1f)] public float lakeSpeedMultiplier = 0.5f;

    private CharacterController controller;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float cameraPitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCamera();

        bool onLake = IsOnLake();
        float currentMaxSpeed = onLake ? maxGroundSpeed * lakeSpeedMultiplier : maxGroundSpeed;

        Vector3 inputDirection = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        inputDirection = inputDirection.normalized;

        if (controller.isGrounded)
        {
            ApplyGroundFriction();

            Accelerate(inputDirection, groundAcceleration, currentMaxSpeed);

            if (Input.GetButton("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            Accelerate(inputDirection, airAcceleration, currentMaxSpeed);

            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    bool IsOnLake()
    {
        RaycastHit hit;

        float rayDistance = controller.bounds.extents.y + 0.2f;

        if (Physics.Raycast(controller.bounds.center, Vector3.down, out hit, rayDistance))
        {
            return hit.collider.CompareTag("Lake");
        }

        return false;
    }

    void Accelerate(Vector3 direction, float acceleration, float targetSpeed)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        float currentSpeed = Vector3.Dot(horizontalVelocity, direction);
        float speedToAdd = targetSpeed - currentSpeed;

        if (speedToAdd <= 0)
        {
            return;
        }

        float accelerationAmount = acceleration * Time.deltaTime;
        accelerationAmount = Mathf.Min(accelerationAmount, speedToAdd);

        horizontalVelocity += direction * accelerationAmount;
    }

    void ApplyGroundFriction()
    {
        float speed = horizontalVelocity.magnitude;

        if (speed <= 0f)
            return;

        float newSpeed = Mathf.Max(speed - groundFriction * speed * Time.deltaTime, 0f);

        horizontalVelocity *= newSpeed / speed;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("End"))
        {
            CellularAutomataCave cave = FindFirstObjectByType<CellularAutomataCave>();

            if (cave != null)
                cave.PlayerWon(gameObject);
        }
    }
}