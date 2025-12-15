using UnityEngine;
using System.Collections;

public class FireZone : MonoBehaviour
{
    [Header("Movement Settings")]
    public float fallSpeed = 15f;       // Tốc độ rơi
    public float existDuration = 5f;    // Thời gian lửa cháy dưới đất

    [Header("Visual Fix")]
    [Tooltip("Khoảng cách nhích lên khi chạm đất để không bị chìm vào nền.")]
    public float groundOffset = 0.5f;   // <-- MỚI: Chỉnh cái này để nâng sprite lên

    [Header("DOT Settings (Sát thương cháy)")]
    [SerializeField] private string dotType = "BossFire"; // Tên loại DOT
    public float burnDamagePerTick = 10f; // Sát thương mỗi nhịp
    public float tickInterval = 1.0f;     // Thời gian giữa các nhịp
    public float burnDuration = 3f;       // Thời gian bị cháy duy trì trên người

    [Header("References")]
    public LayerMask groundLayer;
    private Animator anim;
    private bool isGrounded = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        // Tự hủy sau 10s để tránh rác bộ nhớ nếu lỡ rơi ra ngoài map
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // GIAI ĐOẠN 1: RƠI TỰ DO
        if (!isGrounded)
        {
            // Di chuyển xuống
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // Bắn tia Raycast xuống để check đất
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.2f, groundLayer);

            if (hit.collider != null)
            {
                LandOnGround(hit.point);
            }
        }
    }

    void LandOnGround(Vector2 hitPoint)
    {
        isGrounded = true;

        // --- VISUAL FIX: CỘNG THÊM OFFSET ---
        // Đặt vị trí tại điểm va chạm + nhích lên 1 đoạn (groundOffset)
        transform.position = hitPoint + (Vector2.up * groundOffset);

        // Chuyển animation sang giai đoạn cháy lan (Impact)
        // Trong Animator: Set transition từ "Falling" -> "Burning" khi có trigger "Impact"
        if (anim != null) anim.SetTrigger("Impact");

        // Bắt đầu đếm ngược để biến mất
        StartCoroutine(ZoneLifetimeRoutine());
    }

    IEnumerator ZoneLifetimeRoutine()
    {
        // Tồn tại gây dmg trong X giây
        yield return new WaitForSeconds(existDuration);

        // Hết giờ -> Chuyển sang tắt (Trigger "End")
        if (anim != null) anim.SetTrigger("End");

        // Hủy object sau khi animation tắt xong (ví dụ 1s)
        Destroy(gameObject, 0f);
    }

    // --- LIÊN KẾT VỚI HỆ THỐNG DOT CỦA BẠN ---
    void OnTriggerStay2D(Collider2D other)
    {
        // Chỉ gây hiệu ứng khi đã chạm đất (đang cháy)
        if (!isGrounded) return;

        if (other.CompareTag("Player"))
        {
            // Tìm component quản lý DOT trên người Player
            DamageOverTimeController dotController = other.GetComponent<DamageOverTimeController>();

            if (dotController != null)
            {
                // Gọi hàm ApplyDot giống hệt bên FireTrapDamage.cs
                dotController.ApplyDot(
                    dotType,
                    burnDamagePerTick,
                    tickInterval,
                    burnDuration,
                    this // Source là script này
                );
            }
            else
            {
                // Fallback: Nếu Player chưa gắn script DOT thì trừ máu thẳng
                Entity playerEntity = other.GetComponent<Entity>();
                if (playerEntity != null)
                {
                    // Dùng Time.deltaTime để trừ dần, tránh sốc dmg
                    playerEntity.TakeDamage(burnDamagePerTick * Time.deltaTime, 0, gameObject, false, false);
                }
            }
        }
    }
}