using UnityEngine;

public class HoopScoreDetector : MonoBehaviour
{
    [SerializeField] private int pointsPerBasket = 2;

    private void OnTriggerEnter(Collider other)
    {
        Basketball ball = other.GetComponent<Basketball>();

        if (ball == null)
            return;

        ScoreManager.Instance.AddPoints(pointsPerBasket);
        ball.ResetPlayerAndBallPosition();
        Debug.Log("Basket scored!");
    }
}