using UnityEngine;

public class BallInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;

    [Header("Throw")]
    [SerializeField] private float minThrowForce = 0.1f;
    [SerializeField] private float maxThrowForce = 0.5f;
    [SerializeField] private float chargeSpeed = 0.1f;

    private Basketball heldBall;
    private float currentThrowForce;

    private void Start()
    {
        currentThrowForce = minThrowForce;
    }

    private void Update()
    {
        if (heldBall == null)
        {
            TryPickUpBall();
            return;
        }

        ChargeThrow();
        ReleaseThrow();
    }


    /* Detection for picking up the ball by using a Ray*/
    private void TryPickUpBall()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            return;

        Basketball ball = hit.collider.GetComponent<Basketball>();

        if (ball == null)
            return;

        heldBall = ball;
        currentThrowForce = minThrowForce;

        heldBall.PickUp(holdPoint);
    }

    /* Charging the ball to throw adding charge speed and having a minimum and maximum force for throwning the ball*/
    private void ChargeThrow()
    {
        if (!Input.GetMouseButton(0))
            return;

        currentThrowForce += chargeSpeed * Time.deltaTime;
        currentThrowForce = Mathf.Clamp(
            currentThrowForce,
            minThrowForce,
            maxThrowForce
        );
    }


    /* Releasing the Ball logic*/
    private void ReleaseThrow()
    {
        if (!Input.GetMouseButtonUp(0))
            return;

        Vector3 throwDirection = playerCamera.transform.forward + Vector3.up * 0.25f;

        heldBall.Throw(throwDirection, currentThrowForce);

        heldBall = null;
        currentThrowForce = minThrowForce;
    }


    /* Resetting the held ball correctly when for an example having a foul */
    public void ClearHeldBall()
    {
        heldBall = null;
        currentThrowForce = minThrowForce;
    }
}