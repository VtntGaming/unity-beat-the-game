using UnityEngine;

public class BeeAttackZone : MonoBehaviour
{
    public BeeAttack bee;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bee.PlayerEnteredAttackZone();
            // ❌ Xóa phần này - đã xử lý trong BeeAttack
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bee.PlayerExitedAttackZone();
            // ❌ Xóa phần này - đã xử lý trong BeeAttack
        }
    }
}