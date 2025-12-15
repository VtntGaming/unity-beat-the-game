using UnityEngine;

public class FireHandTrap : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 25f;
    public bool canDamage = true;
    private bool hasDealtDamage = false;

    // --- THÊM BIẾN NÀY ---
    private Animator anim;
    // ---------------------

    // --- THÊM HÀM START ---
    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Bật OneTime = true để nó chạy luồng: Start -> End (Bỏ qua Loop)
            anim.SetBool("One Time", true);
        }
    }
    // ----------------------

    // Giữ nguyên hàm Setup xịn xò (đã có xử lý hướng + scale)
    public void Setup(float dmg, float facingDirection)
    {
        this.damage = dmg;

        // Xử lý Scale và Hướng như đã bàn ở bước trước
        Vector3 currentScale = transform.localScale;
        float sizeX = Mathf.Abs(currentScale.x);

        if (facingDirection >= 0) currentScale.x = sizeX;  // Hướng phải
        else currentScale.x = -sizeX; // Hướng trái

        transform.localScale = currentScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage || hasDealtDamage) return;

        if (other.CompareTag("Player"))
        {
            Entity player = other.GetComponent<Entity>();
            if (player != null)
            {
                float dir = other.transform.position.x < transform.position.x ? -1 : 1;
                player.TakeDamage(damage, dir, gameObject, true, false);
                hasDealtDamage = true;
            }
        }
    }

    public void DestroyHand()
    {
        Destroy(gameObject);
    }

    public void EnableDamage()
    {
        canDamage = true;
    }
}