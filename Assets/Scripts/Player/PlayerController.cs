using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform head;
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Jump")]
    [SerializeField] private float baseJumpForce = 6f;
    [SerializeField] private float maxJumpForce = 40f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Camera Feel")]
    [SerializeField] private float baseFOV = 75f;
    [SerializeField] private float maxFOV = 100f;
    [SerializeField] private float fovSmoothSpeed = 8f;

    // Add Dunk Mechanic?

    private bool isGrounded;
    private float currentSpeed;
    private float yaw;
    private float pitch;

    private void Start()
    {
        currentSpeed = baseSpeed;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /*Handles player input, camera look, jump checks, and camera effects every frame. */
    private void Update()
    {
        HandleLook();
        CheckGround();
        UpdateSpeed();
        HandleJump();
        HandleCameraFOV();
    }

    /* Updates the movement and rotation */
    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
        Move();
    }

    /* Handles mouse camera view and rotates player accordingly */ 
    private void HandleLook()
    {
        yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;

        pitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    /* Handles the player movent using WASD keys*/
    private void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 direction = transform.right * moveX + transform.forward * moveZ;
        direction.Normalize();

        Vector3 movement = direction * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }


    /* Updates the ground state using a sphere check */ 
    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    /* Updates the current speed every frame ensuring smooth movement*/
    private void UpdateSpeed()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0f ||
                        Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0f;

        if (isRunning && isMoving)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= deceleration * Time.deltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);
    }


    /* Handles the Player jump, applies an impulse force when the player is grounded */
    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || !isGrounded)
            return;

        float speedPercent = Mathf.InverseLerp(baseSpeed, maxSpeed, currentSpeed);
        float jumpForce = Mathf.Lerp(baseJumpForce, maxJumpForce, speedPercent);

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /* Handles the Camera field of view when running to enhance sense of speed */
    private void HandleCameraFOV()
    {
        float speedPercent = Mathf.InverseLerp(baseSpeed, maxSpeed, currentSpeed);
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovSmoothSpeed * Time.deltaTime
        );
    }


    /* Function used for adding speed boost (ex. Grabbing a coin boost) */
    public void AddSpeedBoost(float amount)
    {
        currentSpeed = Mathf.Clamp(currentSpeed + amount, baseSpeed, maxSpeed);
    }


    /* Draws the ground check sphere in the Scene view for debugging purposes*/
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}