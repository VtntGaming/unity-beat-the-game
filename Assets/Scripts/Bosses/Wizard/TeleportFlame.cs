using UnityEngine;

public class TeleportFlame : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 20f;
    public float lifeTime = 1.0f; // Thời gian tồn tại của lửa

    private bool hasDealtDamage = false; // Chỉ gây dmg 1 lần (nếu muốn lửa cháy liên tục thì bỏ biến này)

    void Start()
    {
        // Tự hủy sau X giây (để không làm rác game)
        Destroy(gameObject, lifeTime);
    }

    // Hàm để MovementAI truyền chỉ số damage sang (nếu muốn chỉnh từ Boss)
    public void Setup(float dmg)
    {
        this.damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu đã gây dmg rồi thì thôi (tránh dính dmg liên tục trong 1 frame)
        if (hasDealtDamage) return;

        if (other.CompareTag("Player"))
        {
            Entity player = other.GetComponent<Entity>();
            if (player != null)
            {
                // Hướng knockback: Đẩy Player ra xa khỏi tâm ngọn lửa
                float dir = other.transform.position.x < transform.position.x ? -1 : 1;

                // Gây damage
                player.TakeDamage(damage, dir, gameObject, true, false); // isProjectile=false

                hasDealtDamage = true;
            }
        }
    }
}