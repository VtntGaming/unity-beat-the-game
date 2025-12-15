using System.Collections;
using UnityEngine;

public class MovementAI : MonoBehaviour
{
    [Header("Zone Limits")]
    public Transform limitLeft;
    public Transform limitRight;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;

    [Header("Jump Settings")]
    public float jumpDuration = 1.0f;
    public float jumpThreshold = 1.5f;

    [Header("Teleport Settings")]
    public float teleportDropHeight = 4.0f;
    public LayerMask obstacleLayer;

    [Header("Teleport Effect (New)")]
    public GameObject teleportFlamePrefab; // Kéo Prefab lửa vào đây
    public Transform teleportEffectCenter; // Kéo cái Empty Object (tâm lửa) vào đây
    public float teleportDamage = 25f;     // Sát thương của lửa
    public float castTime = 0.5f;          // Thời gian Boss đứng gồng (Animation) trước khi biến mất
    // ------------------

    [Header("Smart Detection (QUAN TRỌNG)")]
    public float bodyCollisionRadius = 0.6f; // Bán kính thân người để check va chạm
    public int trajectorySteps = 30;         // Số bước vẽ đường đạn
    public float maxWalkableDrop = 0.5f;
    public int gapCheckSamples = 10;

    [Header("Ground Detection (2 Points)")]
    // Xóa biến cũ: public Transform groundCheck;
    public Transform groundCheckLeft;  // Chân trái
    public Transform groundCheckRight; // Chân phải

    public float groundCheckRadius = 0.1f; // Giảm nhỏ lại vì check 2 điểm chính xác hơn
    public LayerMask groundLayer;

    [Header("References")]
    public Transform[] patrolPoints;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 initScale;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isBusy = false;
    [HideInInspector] public bool isMoving = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        initScale = transform.localScale;

        foreach (Transform point in patrolPoints)
        {
            if (point.parent == transform) point.SetParent(null);
        }
    }

    void Update()
    {
        // Check chân trái
        bool leftGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, groundCheckRadius, groundLayer);
        // Check chân phải
        bool rightGrounded = Physics2D.OverlapCircle(groundCheckRight.position, groundCheckRadius, groundLayer);

        // Chỉ cần 1 trong 2 chạm đất là OK
        isGrounded = leftGrounded || rightGrounded;
        if (animator != null)
        {
            animator.SetBool("IsLanded", isGrounded);
            animator.SetFloat("AirSpeed", rb.linearVelocity.y); // Unity 6
        }
    }

    // --- CÁC HÀM HÀNH ĐỘNG ---

    public IEnumerator Action_WalkTo(Vector3 rawDestination)
    {
        isBusy = true;
        Vector3 destination = ClampToZone(rawDestination);

        if (animator != null) animator.SetBool("IsWalking", true);

        // Timer để giới hạn số lần check hố (đỡ lag, đỡ lỗi)
        float checkGapTimer = 0;

        while (Mathf.Abs(transform.position.x - destination.x) > 0.2f)
        {
            Vector2 targetPos = new Vector2(destination.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            FlipFace(destination);

            // --- THAY ĐỔI TẠI ĐÂY ---
            // Thay vì check mỗi frame, ta chỉ check mỗi 0.2s
            checkGapTimer += Time.deltaTime;
            if (checkGapTimer > 0.2f)
            {
                checkGapTimer = 0;
                // Check hố thường (strictMode = false)
                if (CheckForGap(transform.position, destination, false))
                {
                    // Debug.Log("Walk: Phát hiện hố -> Dừng.");
                    break;
                }
            }
            // ------------------------

            yield return null;
        }

        if (animator != null) animator.SetBool("IsWalking", false);
        isBusy = false;
    }

    public IEnumerator Action_HighJump(Vector3 rawDestination)
    {
        isBusy = true;
        Vector3 destination = ClampToZone(rawDestination);

        FlipFace(destination);
        if (animator != null) animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.2f);

        Vector2 velocity = CalculateProjectileVelocity(transform.position, destination, jumpDuration);
        rb.linearVelocity = velocity;

        yield return new WaitForSeconds(0.1f);
        while (!isGrounded) yield return null;

        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(destination.x, transform.position.y, transform.position.z);

        isBusy = false;
    }

    //public IEnumerator Action_Teleport(Vector3 rawDestination)
    //{
    //    isBusy = true;
    //    Vector3 destination = ClampToZone(rawDestination);

    //    // 1. Dừng mọi chuyển động vật lý
    //    rb.linearVelocity = Vector2.zero;

    //    // 2. Kích hoạt Animation (Gồng/Chuẩn bị nhảy)
    //    if (animator != null) animator.Play("Idle"); // Hoặc đổi thành "TeleportStart" nếu có anim riêng

    //    // 3. TRIỆU HỒI LỬA (NGAY VỊ TRÍ CŨ)
    //    if (teleportFlamePrefab != null && teleportEffectCenter != null)
    //    {
    //        // Sinh ra lửa tại vị trí của teleportEffectCenter
    //        GameObject flame = Instantiate(teleportFlamePrefab, teleportEffectCenter.position, Quaternion.identity);

    //        // Setup damage cho lửa
    //        TeleportFlame flameScript = flame.GetComponent<TeleportFlame>();
    //        if (flameScript != null) flameScript.Setup(teleportDamage);
    //    }

    //    // 4. CHỜ ANIMATION (Để người chơi thấy lửa bùng lên và Boss biến mất hợp lý)
    //    yield return new WaitForSeconds(castTime);

    //    // ====================================================
    //    // BẮT ĐẦU DỊCH CHUYỂN (Logic cũ)
    //    // ====================================================

    //    // Tính toán vị trí đáp (Spawn từ trên trời rơi xuống)
    //    Vector3 spawn = destination;
    //    RaycastHit2D hit = Physics2D.Raycast(destination, Vector2.up, teleportDropHeight + 1f, obstacleLayer);

    //    if (hit.collider != null) spawn.y = hit.point.y - 1.5f; // Nếu đụng trần thì spawn dưới trần 1 chút
    //    else spawn.y += teleportDropHeight; // Nếu không thì spawn trên cao theo setting

    //    // Dịch chuyển tức thời tới vị trí mới
    //    transform.position = spawn;

    //    // Reset vận tốc lần nữa cho chắc
    //    rb.linearVelocity = Vector2.zero;

    //    // 5. CHỜ RƠI XUỐNG ĐẤT
    //    // Đợi 1 frame để vật lý cập nhật
    //    yield return new WaitForSeconds(0.1f);

    //    // Vòng lặp chờ chạm đất
    //    while (!isGrounded) yield return null;

    //    // 6. KẾT THÚC
    //    rb.linearVelocity = Vector2.zero;

    //    // (Tùy chọn) Chỉnh lại vị trí Y chính xác theo đích đến để tránh bị trôi
    //    transform.position = new Vector3(destination.x, transform.position.y, transform.position.z);

    //    isBusy = false;
    //}
    // Mới
    public IEnumerator Action_Teleport(Vector3 rawDestination)
    {
        isBusy = true;

        // 1. TÍNH TOÁN VỊ TRÍ ĐÍCH NGAY LẬP TỨC (Đưa lên đầu hàm)
        // Để biết chỗ mà thả lửa bên kia
        Vector3 destination = ClampToZone(rawDestination);

        // Tính toán điểm Boss sẽ xuất hiện (Trên trời rơi xuống)
        Vector3 spawnPos = destination;
        RaycastHit2D hit = Physics2D.Raycast(destination, Vector2.up, teleportDropHeight + 1f, obstacleLayer);

        if (hit.collider != null) spawnPos.y = hit.point.y - 1.5f; // Đụng trần
        else spawnPos.y += teleportDropHeight; // Không đụng trần

        // 2. DỪNG BOSS & ANIMATION
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.Play("Idle");

        // 3. TRIỆU HỒI LỬA (Ở CẢ 2 NƠI)
        if (teleportFlamePrefab != null)
        {
            // A. Lửa tại chỗ cũ (Dùng teleportEffectCenter nếu có, không thì lấy chân Boss)
            Vector3 startFirePos = (teleportEffectCenter != null) ? teleportEffectCenter.position : transform.position;
            GameObject flameStart = Instantiate(teleportFlamePrefab, startFirePos, Quaternion.identity);

            // B. Lửa tại chỗ mới (Tại đích đến - destination)
            // Lưu ý: Ta dùng 'destination' (mặt đất) để lửa mọc từ đất lên, chứ không dùng 'spawnPos' (trên trời)
            GameObject flameEnd = Instantiate(teleportFlamePrefab, destination, Quaternion.identity);

            // C. Setup damage cho cả 2 ngọn lửa
            TeleportFlame script1 = flameStart.GetComponent<TeleportFlame>();
            if (script1 != null) script1.Setup(teleportDamage);

            TeleportFlame script2 = flameEnd.GetComponent<TeleportFlame>();
            if (script2 != null) script2.Setup(teleportDamage);
        }

        // 4. CHỜ DIỄN HOẠT (Lúc này người chơi thấy 2 cột lửa bùng lên)
        yield return new WaitForSeconds(castTime);

        // ====================================================
        // BẮT ĐẦU DỊCH CHUYỂN
        // ====================================================

        // Dịch chuyển tức thời tới vị trí trên trời đã tính ở bước 1
        transform.position = spawnPos;

        // Reset vận tốc
        rb.linearVelocity = Vector2.zero;

        // 5. CHỜ RƠI XUỐNG ĐẤT
        yield return new WaitForSeconds(0.1f);
        while (!isGrounded) yield return null;

        // 6. KẾT THÚC
        rb.linearVelocity = Vector2.zero;

        // Chỉnh lại vị trí X chuẩn xác (tránh bị trôi khi rơi)
        transform.position = new Vector3(destination.x, transform.position.y, transform.position.z);

        isBusy = false;
    }


    public void Continuous_WalkTo(Vector3 rawDestination)
    {
        // 1. Kẹp vị trí
        Vector3 destination = ClampToZone(rawDestination);

        // 2. Bật Animation (chỉ set 1 lần, Animator thông minh sẽ tự giữ trạng thái)
        if (animator != null) animator.SetBool("IsWalking", true);

        // 3. Di chuyển 1 bước nhỏ (Frame-based)
        Vector2 targetPos = new Vector2(destination.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 4. Quay mặt
        FlipFace(destination);

        // Đánh dấu là đang di chuyển (để AttackAI biết mà dừng nếu cần)
        isMoving = true;
    }

    public void StopImmediate()
    {
        StopAllCoroutines();
        isBusy = false;
        isMoving = false; // <-- Reset trạng thái
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool("IsWalking", false);
    }

    // --- LOGIC CHECK & DEBUG (ĐÃ KHÔI PHỤC) ---

    // 1. Kiểm tra xem đường nhảy có thoáng không? (Vẽ đường xanh/đỏ)
    public bool IsJumpPathClear(Vector3 startPos, Vector3 targetPos)
    {
        // Tính trước vận tốc dự kiến
        Vector2 velocity = CalculateProjectileVelocity(startPos, targetPos, jumpDuration);
        float gravity = Physics2D.gravity.y * rb.gravityScale;

        // Mô phỏng đường đạn
        for (int i = 1; i <= trajectorySteps; i++)
        {
            float t = (jumpDuration * i) / trajectorySteps;
            float x = startPos.x + velocity.x * t;
            float y = startPos.y + velocity.y * t + 0.5f * gravity * t * t;
            Vector2 checkPos = new Vector2(x, y);

            // Kiểm tra va chạm với Obstacle (Tường/Trần)
            Collider2D hit = Physics2D.OverlapCircle(checkPos, bodyCollisionRadius, obstacleLayer);

            if (hit != null)
            {
                // Nếu va chạm không phải là điểm đến (Target) -> Bị chặn
                if (Vector2.Distance(checkPos, targetPos) > 1.5f)
                {
                    Debug.DrawLine(startPos, checkPos, Color.red, 2.0f); // Vẽ đường đỏ báo lỗi
                    return false; // BỊ CHẶN
                }
            }

            // Vẽ đường xanh mô phỏng quỹ đạo
            if (i > 1)
            {
                float prevT = (jumpDuration * (i - 1)) / trajectorySteps;
                float prevX = startPos.x + velocity.x * prevT;
                float prevY = startPos.y + velocity.y * prevT + 0.5f * gravity * prevT * prevT;
                Debug.DrawLine(new Vector2(prevX, prevY), checkPos, Color.green, 2.0f);
            }
        }
        return true; // Đường thoáng
    }

    // 2. Quyết định Nhảy hay Đi bộ
    public bool ShouldJump(Vector3 targetPos)
    {
        Vector3 clampedTarget = ClampToZone(targetPos);
        float heightDiff = clampedTarget.y - transform.position.y;

        // 1. Nếu đích CAO HƠN đầu hẳn hoi (> 1.5m) -> Chắc chắn NHẢY
        if (heightDiff > jumpThreshold) return true;

        // 2. Nếu Đích THẤP HƠN hoặc NGANG BẰNG (Cùng tầng)
        if (Mathf.Abs(heightDiff) <= jumpThreshold)
        {
            // Ở cùng tầng thì ta dùng chế độ check hố "Nghiêm ngặt" (strictMode = true)
            // Nghĩa là: Chỉ nhảy khi thực sự mất đất (null), bỏ qua dốc nhỏ
            bool isStrict = true;

            // Chỉ check nếu khoảng cách X đủ xa (gần quá thì khỏi check)
            if (Mathf.Abs(clampedTarget.x - transform.position.x) > 0.6f)
            {
                if (CheckForGap(transform.position, clampedTarget, isStrict)) return true;
            }

            // Nếu không có hố sâu -> ĐI BỘ
            return false;
        }

        // Các trường hợp còn lại (đích thấp hơn nhiều) -> Check hố thường
        if (CheckForGap(transform.position, clampedTarget, false)) return true;

        return false;
    }

    // 3. Helper tính toán
    bool CheckForGap(Vector3 start, Vector3 end, bool strictMode)
    {
        if (Mathf.Abs(start.y - end.y) > jumpThreshold) return true;

        float prevY = start.y;

        // Bắt đầu check
        for (int i = 1; i <= gapCheckSamples; i++)
        {
            float t = (float)i / (gapCheckSamples + 1);
            Vector3 checkPos = Vector3.Lerp(start, end, t);

            // Bắn tia từ cao hơn chân 1 chút
            Vector3 rayOrigin = new Vector3(checkPos.x, start.y + 1.0f, 0);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 10f, groundLayer);

            // 1. Mất đất hoàn toàn
            if (hit.collider == null)
            {
                // Debug.DrawLine(rayOrigin, rayOrigin + Vector3.down * 5f, Color.red, 0.5f); // <-- Đã tắt vẽ đỏ
                return true;
            }   

            // 2. Đất sụt (Dốc/Bậc thang)
            if (!strictMode)
            {
                if (prevY - hit.point.y > maxWalkableDrop)
                {
                    // Debug.DrawLine(rayOrigin, hit.point, Color.yellow, 0.5f); // <-- Đã tắt vẽ vàng (thủ phạm gây rối mắt)
                    return true;
                }
            }

            // Vẽ đường xanh nhỏ để biết code vẫn chạy (nếu muốn tắt hẳn thì comment nốt dòng này)
            // Debug.DrawLine(new Vector3(checkPos.x, prevY, 0), hit.point, Color.green, 0.1f);

            prevY = hit.point.y;
        }
        return false;
    }

    public Vector3 ClampToZone(Vector3 targetPos)
    {
        if (limitLeft == null || limitRight == null) return targetPos;
        float minX = Mathf.Min(limitLeft.position.x, limitRight.position.x);
        float maxX = Mathf.Max(limitLeft.position.x, limitRight.position.x);
        return new Vector3(Mathf.Clamp(targetPos.x, minX, maxX), targetPos.y, targetPos.z);
    }

    public Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Length == 0) return transform;
        return patrolPoints[Random.Range(0, patrolPoints.Length)];
    }

    public Transform GetFarthestPoint(Vector3 fromPos)
    {
        if (patrolPoints.Length == 0) return transform;
        Transform best = patrolPoints[0];
        float maxDist = -1;
        foreach (var p in patrolPoints)
        {
            float d = Vector2.Distance(p.position, fromPos);
            if (d > maxDist) { maxDist = d; best = p; }
        }
        return best;
    }

    Vector2 CalculateProjectileVelocity(Vector3 s, Vector3 e, float t)
    {
        float dx = e.x - s.x; float dy = e.y - s.y; float g = Physics2D.gravity.y * rb.gravityScale;
        return new Vector2(dx / t, (dy - 0.5f * g * t * t) / t);
    }

    void FlipFace(Vector3 t)
    {
        if (t.x > transform.position.x) transform.localScale = new Vector3(Mathf.Abs(initScale.x), initScale.y, initScale.z);
        else transform.localScale = new Vector3(-Mathf.Abs(initScale.x), initScale.y, initScale.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheckLeft != null)
            Gizmos.DrawWireSphere(groundCheckLeft.position, groundCheckRadius);

        if (groundCheckRight != null)
            Gizmos.DrawWireSphere(groundCheckRight.position, groundCheckRadius);
    }
}