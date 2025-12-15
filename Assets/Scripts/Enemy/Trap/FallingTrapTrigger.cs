using UnityEngine;

public class FallingTrapTrigger : MonoBehaviour
{
    public FallingTrap trap;  // Gán trong Unity

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[FallingTrapTrigger] Player entered trigger zone.");
            if (trap != null)
            {
                trap.ActivateTrap();
            }
            else
            {
                Debug.LogWarning("[FallingTrapTrigger] Trap reference is missing!");
            }
        }
    }
}

