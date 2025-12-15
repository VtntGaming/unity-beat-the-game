using UnityEngine;

public class OrbPickup : MonoBehaviour
{
    public BuffType buffType;  // Loại buff sẽ được áp dụng (ví dụ: DoubleJump)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Gọi BuffManager từ PlayerController để áp dụng buff
                playerController.ApplyBuff(buffType);

                AudioManager.Sfx(Sound.PickUpBuff);
                // Xóa orb sau khi nhặt
                Destroy(gameObject);
            }
        }
    }
}
