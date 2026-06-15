using UnityEngine;

public class BallInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;

    [Header("Throw")]
    [SerializeField] private float minThrowForce = 15.0f;
    [SerializeField] private float maxThrowForce = 30.0f;
    [SerializeField] private float timeToMaxCharge = 1.5f;



    private Basketball heldBall;
    private float currentThrowForce;
    private float currentChargeTime;

    private void Start()
    {
        currentThrowForce = minThrowForce;
    }

    private void Update()
    {
        /* Clears interaction state after a manual reset so releasing left click
           does not throw a previously held ball */
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearHeldBall();
            return;
        }


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
        // Resetting the throw force if dribbling
        if (Input.GetMouseButton(1))
        {
            currentThrowForce = minThrowForce;
            currentChargeTime = 0f;
            return;
        }

        if (!Input.GetMouseButton(0))
            return;

        currentChargeTime += Time.deltaTime;
        currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, timeToMaxCharge);

        float chargePercent = currentChargeTime / timeToMaxCharge;

        currentThrowForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            chargePercent
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
        currentChargeTime = 0f;
    }


    /* Resetting the held ball correctly when for an example having a foul */
    public void ClearHeldBall()
    {
        heldBall = null;
        currentThrowForce = minThrowForce;
    }
}