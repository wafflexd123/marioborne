using UnityEngine;

public class MoveSpeedLimit : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            PlayerMovement playerMovement = player.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentState.values.YdragXvelocity.minY = 10;//this probably wont work properly
                playerMovement.currentState.values.YdragXvelocity.maxY = 10;
            }
            else
            {
                Debug.LogError("No PlayerMovement component found on the player object");
            }
        }
    }
}
