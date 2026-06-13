using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Basketball : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider ballCollider;

    [Header("Held Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float ballRadius = 0.35f;
    [SerializeField] private float groundOffset = 0.05f;

    [Header("Dribble")]
    [SerializeField] private float dribbleHeight = 0.9f;
    [SerializeField] private float dribbleSpeed = 10f;

    [Header("Foul")]
    [SerializeField] private float foulTimeLimit = 3f;
    [SerializeField] private Transform resetPoint;
    [SerializeField] private GameObject foulPopup;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private Transform playerResetPoint;

    private float timeWithoutDribble;

    public bool IsHeld { get; private set; }
    public bool WasThrown { get; private set; }
    public bool TouchedRim { get; private set; }

    private Transform holdPoint;
    private BallInteractor currentInteractor;


    /* Makes sure that the ball object is existant in the scene */
    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (ballCollider == null)
            ballCollider = GetComponent<Collider>();
    }

    /* Updating the hold position of the ball, avoiding the ball to faze through the ground, handling dribbling timer */
    private void Update()
    {

        // Reset Player Position and resets ball position if its lost or stuck or too far away
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayerAndBallPosition();
        }

        if (!IsHeld || holdPoint == null)
            return;


        if (HandleFoulTimer())
            return;


        currentInteractor = FindFirstObjectByType<BallInteractor>();

        Vector3 targetPosition = holdPoint.position;
        bool isDribbling = Input.GetMouseButton(1);

        if (isDribbling)
        {
            timeWithoutDribble = 0f;
        }
        
        if (Input.GetMouseButton(1))
        {
            float bounce = Mathf.PingPong(Time.time * dribbleSpeed, 1f);

            bounce = Mathf.SmoothStep(0f, 1f, bounce);

            targetPosition = holdPoint.position + Vector3.down * (bounce * dribbleHeight);
        }

        if (Physics.Raycast(
            targetPosition + Vector3.up,
            Vector3.down,
            out RaycastHit hit,
            2f,
            groundLayer))
        {
            float minimumY = hit.point.y + ballRadius + groundOffset;

            if (targetPosition.y < minimumY)
                targetPosition.y = minimumY;
        }

        transform.position = targetPosition;
        transform.rotation = holdPoint.rotation;
    }

    /* Pickup Checks see if the player is holding the ball or was thrown, etc. */
    public void PickUp(Transform newHoldPoint)
    {
        IsHeld = true;
        WasThrown = false;
        TouchedRim = false;
        holdPoint = newHoldPoint;
        timeWithoutDribble = 0f;


        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
    }


    /* Throwing the Ball by adding force and torque/spin  */
    public void Throw(Vector3 direction, float force)
    {
        IsHeld = false;
        WasThrown = true;
        holdPoint = null;

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction.normalized * force, ForceMode.Impulse);

        Vector3 spinAxis = Vector3.Cross(direction.normalized, Vector3.up);
        rb.AddTorque(spinAxis * force * 0.35f, ForceMode.Impulse);
    }

    /* Checks if the Rim is touched by the ball */
    public void MarkRimTouched()
    {
        TouchedRim = true;
    }


    /* Resets the ball position and States */
    public void ResetBall(Vector3 position)
    {
        IsHeld = false;
        WasThrown = false;
        TouchedRim = false;
        holdPoint = null;

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = position;
        transform.rotation = Quaternion.identity;
    }

    /* Resets the ball position and States And the Player's Position*/ 
    public void ResetPlayerAndBallPosition()
    {
        ResetBall(resetPoint.position);

        if (player != null)
        {
            player.position = playerResetPoint.position;
            player.rotation = playerResetPoint.rotation;
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }

    /* Handles the Foul Timer and triggers the Foul logic*/
    private bool HandleFoulTimer()
    {
        bool isDribbling = Input.GetMouseButton(1);

        bool isMoving =
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0f;

        if (isDribbling || !isMoving)
        {
            timeWithoutDribble = 0f;
            return false;
        }

        timeWithoutDribble += Time.deltaTime;

        if (timeWithoutDribble >= foulTimeLimit)
        {
            TriggerFoul();
            return true;
        }

        return false;
    }


    /* When the ball is not dribbled for a certain amount of time */
    private void TriggerFoul()
    {

        Debug.Log("FOUL!");

        timeWithoutDribble = 0f;

        if (currentInteractor != null)
            currentInteractor.ClearHeldBall();

        if (foulPopup != null)
        {
            foulPopup.SetActive(true);
            StartCoroutine(HideFoulPopup());
        }
        else
            Debug.LogWarning("Foul popup is not assigned.");

        if (resetPoint != null)
            ResetBall(resetPoint.position);
        else
            Debug.LogWarning("Reset point is not assigned.");
    }


    /* Timer for the text to go away on the Foul*/
    private IEnumerator HideFoulPopup()
    {
        yield return new WaitForSeconds(3f);

        if (foulPopup != null)
            foulPopup.SetActive(false);
    }


}