using UnityEngine;
using UnityEngine.UI;

public class MotionBlurControl : MonoBehaviour
{
    public Image edgeBlurImage;
    public float minSpeedForEffect = 5f;
    public float maxSpeedForEffect = 10f;
    public float minAlpha = 0f;
    public float maxAlpha = 0.5f;

    private GameObject player;
    private Rigidbody playerRigidbody;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (playerRigidbody)
        {
            float playerSpeed = playerRigidbody.velocity.magnitude;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (playerSpeed - minSpeedForEffect) / (maxSpeedForEffect - minSpeedForEffect));
            alpha = Mathf.Clamp(alpha, minAlpha, maxAlpha);

            Color newColor = edgeBlurImage.color;
            newColor.a = alpha;
            edgeBlurImage.color = newColor;
        }
    }
}
