using System.Collections;
using System.Collections.Generic; // Cần cái này để dùng List
using UnityEngine;

[System.Serializable] // Để hiện ra trên Inspector
public class BossSkillData
{
    public string skillName;       // Tên skill (để dễ nhớ)
    public string animTrigger;     // Tên Trigger trong Animator (VD: "AttackRange1", "IceStorm")
    public float weight = 10f;     // Trọng số (Càng cao càng dễ ra)
    public float castDuration = 1.5f; // Thời gian diễn hoạt ảnh

    // Bạn có thể thêm các thông số khác nếu cần (VD: range tối thiểu để dùng skill này)
}

public class AttackAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    private MovementAI movement;
    private Animator animator;

    [Header("Detection Zones")]
    public float detectionRange = 10f;
    public float attackRange = 2.0f;
    private bool justAttacked = false;

    [Header("Melee Settings (MỚI)")]
    public float meleeDamage = 35f;          // Sát thương gây ra
    public Vector2 meleeBoxSize = new Vector2(1.5f, 1.5f); // Kích thước vùng đánh (Dài x Cao)
    public Vector2 meleeBoxOffset = new Vector2(1.0f, 0f); // Vị trí vùng đánh (lệch về phía trước bao nhiêu)
    public float damageDelay = 0.4f;         // Thời gian chờ từ lúc múa kiếm đến lúc gây dmg (Wind-up)
    public LayerMask targetLayer;

    [Header("Logic Settings")]
    public float actionDelay = 1.0f;
    [Range(0, 100)] public int chaseChance = 40;
    public Vector2 chaseDurationRange = new Vector2(4f, 8f);

    [Header("AI Intelligence")]
    public float nodeCooldownDuration = 5.0f; // Thời gian cấm quay lại điểm cũ
    private Dictionary<Transform, float> nodeCooldowns = new Dictionary<Transform, float>();

    [Header("Risk System")]
    public float baseSkillChance = 20f;
    public float chanceIncreasePerSec = 10f;
    private float currentSkillChance;

    // --- PHẦN MỚI: DANH SÁCH SKILL ---
    [Header("Skill Manager")]
    public List<BossSkillData> skillList; // Kéo thả các skill vào đây trên Inspector

    [Header("Skill: Fireball (AttackRange1)")]
    public GameObject fireballPrefab;

    // --- FIRE BALL ---
    [Tooltip("Kích thước vùng xuất hiện đạn (Rộng x Cao)")]
    public Vector2 spawnAreaSize = new Vector2(2.0f, 3.0f);

    [Tooltip("Tâm của vùng xuất hiện (Lệch so với Boss)")]
    public Vector2 spawnAreaOffset = new Vector2(2.0f, 1.0f);

    [Range(1, 10)] public int minProjectiles = 1; // Số đạn tối thiểu
    [Range(1, 10)] public int maxProjectiles = 3; // Số đạn tối đa
    // --------------------

    // --- FIRE HAND ---
    [Header("Skill: Fire Hand (Attack 2)")]
    public GameObject fireHandPrefab;
    public float handDamage = 30f;
    [Tooltip("Khoảng cách chặn đầu (mét)")]
    public float interceptDistance = 3.5f;
    [Tooltip("Độ to của bàn tay (Mặc định 1, chỉnh lên 2 hoặc 3 nếu thấy bé)")]
    public Vector3 handScale = new Vector3(2f, 2f, 1f); // <--- THÊM DÒNG NÀY
    [Tooltip("Chỉnh độ cao (Dương là lên cao, Âm là chìm xuống đất)")]
    public float handHeightOffset = 0.5f;

    // --------------------

    // Bạn có thể thêm IcePrefab, LightningPrefab ở đây...
    [Header("Ultimate: Fire Rain")]
    public GameObject fireZonePrefab;
    public Transform highPoint;

    // --- THÊM 2 BIẾN NÀY ---
    [Tooltip("Kéo object đại diện độ cao Tầng 1 (để bom rơi trúng sàn tầng 1)")]
    public Transform floor1Point;

    [Tooltip("Kéo object đại diện độ cao Tầng 2 (để bom rơi trúng sàn tầng 2)")]
    public Transform floor2Point;

    // --- THÊM DÒNG NÀY ---
    [Tooltip("Độ trễ giữa mỗi quả bom (Số càng nhỏ rải càng nhanh). Mặc định 0.05s")]
    public float rainSpawnDelay = 0.05f;
    [Tooltip("Thời gian nghỉ giữa các đợt mưa (Rải xong đợt 1 -> Nghỉ X giây -> Rải đợt 2)")]
    public float waveDelay = 2.0f;
    // -----------------------

    public float rainSpacing = 2.0f;
    public Vector2Int waveCountRange = new Vector2Int(2, 4);

    // Bạn có thể thêm IcePrefab, LightningPrefab ở đây...


    void Start()
    {
        movement = GetComponent<MovementAI>();
        animator = GetComponent<Animator>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        currentSkillChance = baseSkillChance;
        StartCoroutine(BossBehaviorLoop());
    }

    IEnumerator BossBehaviorLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            // 0. CHECK ĐIỀU KIỆN
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            bool inLimit = IsPlayerInLimit();

            if (distToPlayer > detectionRange || !inLimit)
            {
                if (movement.isMoving || movement.isBusy) movement.StopImmediate();
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 1. ROLL ACTION
            int roll = Random.Range(0, 100);

            // [DEBUG] In ra xem nó roll cái gì
            string actionName = (roll < chaseChance) ? "CHASE" : "PATROL";
            Debug.Log($"<color=white>[AI LOOP]</color> Rolled: {roll} (Chase < {chaseChance}) -> <b>{actionName}</b>");

            if (roll < chaseChance)
            {
                yield return StartCoroutine(Action_ChasePlayer());
            }
            else
            {
                yield return StartCoroutine(Action_PatrolToPoint());
            }

            // 2. NGHỈ
            if (justAttacked)
            {
                Debug.Log("[AI LOOP] Vừa đánh xong -> Skip nghỉ -> Roll tiếp ngay!");
                justAttacked = false;
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(actionDelay);
            }
        }
    }

    IEnumerator Action_ChasePlayer()
    {
        float maxDuration = Random.Range(chaseDurationRange.x, chaseDurationRange.y);
        float chaseTimer = 0;
        float riskInterval = 1.0f;
        float riskTimer = 0;

        Debug.Log($"<color=orange>[CHASE]</color> Start! MaxTime: {maxDuration:F1}s");

        while (chaseTimer < maxDuration)
        {
            if (!IsPlayerInLimit()) break;
            float dist = Vector2.Distance(transform.position, player.position);

            // A. MELEE
            if (dist <= attackRange)
            {
                movement.StopImmediate();
                yield return StartCoroutine(PerformAttack1());
                yield return StartCoroutine(PerformRetreat());
                yield break;
            }

            // B. RISK SYSTEM
            // B. RISK SYSTEM (Đã đơn giản hóa)
            riskTimer += Time.deltaTime;
            if (riskTimer >= riskInterval)
            {
                riskTimer = 0;
                float dice = Random.Range(0f, 100f);
                if (dice < currentSkillChance)
                {
                    movement.StopImmediate();

                    // CHỈ CẦN GỌI HÀM NÀY LÀ XONG
                    // Nó sẽ tự chọn Fireball hay FireHand dựa trên Weight bạn cài ở Inspector
                    BossSkillData selectedSkill = PickRandomSkill();
                    
                    if (selectedSkill != null) 
                    {
                        Debug.Log($"[AI] Skill Triggered: {selectedSkill.skillName}");
                        yield return StartCoroutine(PerformSkillFromData(selectedSkill));
                    }

                    currentSkillChance = baseSkillChance;
                    yield return StartCoroutine(PerformRetreat());
                    yield break;
                }
                else currentSkillChance += chanceIncreasePerSec;
            }

            // C. DI CHUYỂN
            if (!movement.isBusy)
            {
                Vector3 targetGround = GetPredictedGroundPos(player.position);
                bool needJump = movement.ShouldJump(targetGround);
                bool isDirectlyBelow = IsTargetDirectlyBelow(player.position);

                if (needJump)
                {
                    // 1. Check nhảy trực tiếp (Nếu đường thoáng & không bị đè đầu)
                    if (!isDirectlyBelow && movement.IsJumpPathClear(transform.position, targetGround))
                    {
                        yield return StartCoroutine(movement.Action_HighJump(targetGround));
                    }
                    else
                    {
                        // 2. Đường bị chặn (Tường/Trần) -> TÌM GIẢI PHÁP KHÁC
                        if (isDirectlyBelow) Debug.LogWarning("[AI] Player below -> Detour");
                        else Debug.LogWarning("[AI] Jump Blocked -> Finding alternative...");

                        // Ưu tiên A: Tìm điểm nhảy trung gian (Detour Point - Patrol Point ở tầng khác)
                        Transform detourPoint = FindBestDetourPoint(targetGround);

                        if (detourPoint != null)
                        {
                            yield return StartCoroutine(movement.Action_HighJump(detourPoint.position));
                        }
                        else
                        {
                            // Ưu tiên B: Tìm mép sàn thoáng để đi ra nhảy (Ceiling Scan)
                            Vector3? clearSpot = FindClearPositionForJump(targetGround);

                            if (clearSpot.HasValue)
                            {
                                movement.Continuous_WalkTo(clearSpot.Value);
                            }
                            else
                            {
                                // Ưu tiên C (MỚI): Bí quá -> Đi bộ tới Node gần nhất cùng tầng để Reset vị trí
                                Transform nearbyNode = FindNearestWalkableNode();

                                if (nearbyNode != null)
                                {
                                    // Debug.Log("Bí đường -> Đi tới node gần nhất: " + nearbyNode.name);

                                    // Ghi nhớ Node này vào sổ đen (Cooldown) để không quay lại ngay
                                    if (nodeCooldowns.ContainsKey(nearbyNode))
                                        nodeCooldowns[nearbyNode] = Time.time + nodeCooldownDuration;
                                    else
                                        nodeCooldowns.Add(nearbyNode, Time.time + nodeCooldownDuration);

                                    movement.Continuous_WalkTo(nearbyNode.position);
                                }
                                else
                                {
                                    // Fallback cuối cùng: Đi ngang hú họa
                                    Vector3 sideStep = transform.position;
                                    sideStep.x += (transform.position.x > player.position.x) ? 2.0f : -2.0f;
                                    movement.Continuous_WalkTo(sideStep);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Cùng tầng -> Đi bộ tới Player
                    movement.Continuous_WalkTo(targetGround);
                }
            }

            chaseTimer += Time.deltaTime;
            yield return null;
        }

        movement.StopImmediate();
        Debug.Log("<color=grey>[CHASE]</color> Timeout.");
    }

    // --- HÀNH ĐỘNG 2: ĐI TUẦN ---
    IEnumerator Action_PatrolToPoint()
    {
        Transform targetPoint = movement.GetRandomPatrolPoint();

        bool needJump = movement.ShouldJump(targetPoint.position);

        if (needJump)
        {
            // Kiểm tra vật cản trước khi nhảy tuần tra
            if (movement.IsJumpPathClear(transform.position, targetPoint.position))
            {
                yield return StartCoroutine(movement.Action_HighJump(targetPoint.position));
            }
            else
            {
                // --- PHẦN FIX LỖI: ---
                // Nếu đường nhảy bị chặn, ĐỪNG thoát ngay.
                // Hãy đứng chờ 1 chút coi như đang suy nghĩ, để không bị skip lượt quá nhanh
                Debug.Log($"[PATROL] Đường tới {targetPoint.name} bị chặn -> Đứng chờ.");
                yield return new WaitForSeconds(1.0f);
            }
            // Nếu bị chặn thì thôi, bỏ qua action này (tự động hết actionDelay sẽ chọn điểm khác)
        }
        else
        {
            Coroutine walkJob = StartCoroutine(movement.Action_WalkTo(targetPoint.position));
            while (movement.isBusy)
            {
                if (IsPlayerInLimit() && Vector2.Distance(transform.position, player.position) <= attackRange)
                {
                    movement.StopImmediate();
                    StopCoroutine(walkJob);
                    yield return StartCoroutine(PerformAttack1());
                    yield return StartCoroutine(PerformRetreat());
                    yield break;
                }
                yield return null;
            }
        }
    }

    bool IsTargetDirectlyBelow(Vector3 targetPos)
    {
        float xDiff = Mathf.Abs(transform.position.x - targetPos.x);
        float yDiff = transform.position.y - targetPos.y; // Dương nếu Boss ở trên, Âm nếu Boss ở dưới

        // Nếu lệch X rất nhỏ (< 1.0f) VÀ Boss đang ở cao hơn Player (> 1.5f)
        // -> Đang đứng trên đầu -> Cần Detour
        if (xDiff < 1.0f && yDiff > 1.5f)
        {
            return true;
        }
        return false;
    }

    // --- LOGIC TÌM ĐƯỜNG VÒNG (DETOUR) ---
    // Hàm này tìm 1 điểm Patrol Point nào đó:
    // 1. Gần Player nhất.
    // 2. Mà đường nhảy từ Boss tới đó phải THOÁNG (IsJumpPathClear = true).
    Transform FindBestDetourPoint(Vector3 targetPos)
    {
        if (movement.patrolPoints.Length == 0) return null;

        Transform bestPoint = null;
        // Điểm khởi đầu max distance cực lớn để tìm min, 
        // nhưng nếu bí quá ta sẽ lấy điểm bất kỳ miễn là nhảy được.
        float minDistanceToTarget = Mathf.Infinity;

        // Lấy độ cao của Boss để ưu tiên tìm điểm thấp hơn
        float bossY = transform.position.y;

        foreach (Transform point in movement.patrolPoints)
        {
            // 1. QUAN TRỌNG: Bỏ qua điểm Boss đang đứng (bán kính 1.0f)
            if (Vector2.Distance(transform.position, point.position) < 1.0f) continue;

            // 2. Kiểm tra đường nhảy có thoáng không?
            if (movement.IsJumpPathClear(transform.position, point.position))
            {
                float distToTarget = Vector2.Distance(point.position, targetPos);

                // Ưu tiên điểm có độ cao thấp hơn hoặc bằng Boss (để đi xuống)
                // Nếu điểm đó thấp hơn Boss 0.5m trở lên -> Ưu tiên đặc biệt
                if (point.position.y < bossY - 0.5f)
                {
                    distToTarget *= 0.5f; // Giảm trọng số khoảng cách để ưu tiên chọn
                }

                if (distToTarget < minDistanceToTarget)
                {
                    minDistanceToTarget = distToTarget;
                    bestPoint = point;
                }
            }
        }

        return bestPoint;
    }

    // --- CÁC HÀM PHỤ TRỢ ---

    // --- CÁC HÀM COMBAT ĐÃ NÂNG CẤP ---

    // Hàm quay mặt nhanh (Instant Flip) để ngắm cho chuẩn trước khi đánh
    BossSkillData PickRandomSkill()
    {
        if (skillList == null || skillList.Count == 0) return null;

        float totalWeight = 0;
        foreach (var s in skillList) totalWeight += s.weight;

        float randomValue = Random.Range(0, totalWeight);
        float cursor = 0;

        foreach (var s in skillList)
        {
            cursor += s.weight;
            if (randomValue < cursor) return s;
        }
        return skillList[0]; // Fallback
    }

    IEnumerator PerformSkillFromData(BossSkillData skill)
    {
        // 1. XỬ LÝ ĐẶC BIỆT: Skill Mưa Lửa (FireRain)
        if (skill.skillName == "FireRain" && highPoint != null)
        {
            // A. Teleport lên trần nhà (Đợi teleport xong)
            yield return StartCoroutine(movement.Action_Teleport(highPoint.position));

            // B. Bật Animation múa may
            animator.SetTrigger(skill.animTrigger);

            // C. Gọi hàm rải bom và ĐỢI NÓ XONG (Thêm yield return ở đây)
            // --- SỬA DÒNG NÀY ---
            yield return StartCoroutine(FireRainRoutine());
        }
        // 2. XỬ LÝ ĐẶC BIỆT: Skill Bàn Tay Lửa (FireHand)
        else if (skill.skillName == "FireHand")
        {
            FaceTargetInstant(player.position);

            // [TRIGGER LẦN 1 - ĐÚNG]
            animator.SetTrigger(skill.animTrigger);

            yield return new WaitForSeconds(0.4f);

            if (fireHandPrefab != null)
            {
                // A. Tính toán vị trí
                float directionToPlayer = (player.position.x > transform.position.x) ? 1f : -1f;
                Vector3 targetPos = player.position + new Vector3(directionToPlayer * interceptDistance, 0, 0);
                Vector3 spawnPos = GetPredictedGroundPos(targetPos);

                // Cộng thêm độ cao (Offset)
                spawnPos.y += handHeightOffset;

                // B. Sinh ra bàn tay
                GameObject handObj = Instantiate(fireHandPrefab, spawnPos, Quaternion.identity);

                // C. Set Scale to
                handObj.transform.localScale = handScale;

                // D. Setup damage & hướng
                FireHandTrap handScript = handObj.GetComponent<FireHandTrap>();
                if (handScript != null)
                {
                    float faceDir = (spawnPos.x > player.position.x) ? -1f : 1f;
                    handScript.Setup(handDamage, faceDir);
                }
            }

            // Chờ nốt thời gian còn lại
            yield return new WaitForSeconds(Mathf.Max(0, skill.castDuration - 0.4f));
        }
        // 3. CÁC SKILL THƯỜNG KHÁC (Fireball...)
        else
        {
            // Logic mặc định cho các skill chưa được định nghĩa riêng
            FaceTargetInstant(player.position);

            // [TRIGGER CHO SKILL THƯỜNG]
            animator.SetTrigger(skill.animTrigger);
            yield return new WaitForSeconds(skill.castDuration);
        }

    }


    void FaceTargetInstant(Vector3 targetPos)
    {
        Vector3 scale = transform.localScale;
        // Nếu mục tiêu ở bên phải -> Scale X dương. Ở bên trái -> Scale X âm.
        if (targetPos.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    IEnumerator PerformAttack1()
    {
        // 1. Quay mặt ngay lập tức
        FaceTargetInstant(player.position);

        // 2. Kích hoạt Animation
        animator.SetTrigger("Attack1");

        // 3. Chờ Animation kết thúc (Ví dụ animation dài 1.0s)
        // LƯU Ý: Không còn chờ damageDelay ở đây nữa vì Animation Event sẽ lo việc đó
        yield return new WaitForSeconds(1.0f);
    }

    public void TriggerMeleeDamage()
    {
        // Debug.Log("Animation Event: Chém trúng!");

        // Xác định hướng mặt
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // Tính tâm vùng Hitbox
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(meleeBoxOffset.x * direction, meleeBoxOffset.y);

        // Quét va chạm
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, meleeBoxSize, 0f, targetLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Debug.Log("<color=red>HIT PLAYER!</color>");

                Entity playerEntity = hit.GetComponent<Entity>();
                if (playerEntity != null)
                {
                    playerEntity.TakeDamage(meleeDamage, direction, gameObject, true, false);
                }
            }
        }
    }

    public void CastFireball()
    {
        if (fireballPrefab == null) return;

        // 1. Xác định hướng mặt của Boss
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 2. Tính tâm của vùng Spawn (Offset theo hướng mặt)
        Vector2 areaCenter = (Vector2)transform.position + new Vector2(spawnAreaOffset.x * direction, spawnAreaOffset.y);

        // 3. Random số lượng đạn (từ min đến max)
        // Lưu ý: Random.Range(int, int) thì tham số thứ 2 là Exclusive (không lấy), nên phải +1
        int count = Random.Range(minProjectiles, maxProjectiles + 1);

        // 4. Vòng lặp Spawn đạn
        for (int i = 0; i < count; i++)
        {
            // Random vị trí X, Y trong vùng hình chữ nhật
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomY = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);

            Vector3 spawnPos = areaCenter + new Vector2(randomX, randomY);

            // Tạo đạn
            GameObject fireballObj = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

            // Bắn đạn
            WizardProjectile projScript = fireballObj.GetComponent<WizardProjectile>();
            if (projScript != null)
            {
                Vector2 shootDir = new Vector2(direction, 0); // Bắn thẳng về phía trước
                projScript.Launch(shootDir);
            }
        }
    }

    // =========================================================
    // HÀM GỌI TỪ ANIMATION EVENT (Của clip CastSpell/Ulti)
    // =========================================================
    // =========================================================
    // HÀM 2: MƯA LỬA (Đã sửa logic chọn tầng)
    // =========================================================
    public void CastFireRain()
    {
        // Kiểm tra null để tránh lỗi
        if (fireZonePrefab == null) return;
        
        // Gọi Coroutine chính (Hàm này tự lo logic chọn tầng và rải thảm)
        StartCoroutine(FireRainRoutine());
    }

    // Coroutine này chứa toàn bộ logic rải thảm
    IEnumerator FireRainRoutine()
    {
        // 1. Roll số lượng đợt rải bom (Wave)
        int waves = Random.Range(waveCountRange.x, waveCountRange.y + 1);
        Debug.Log($"<color=red>[ULTI]</color> Mưa Lửa: {waves} đợt!");

        for (int w = 0; w < waves; w++)
        {
            // --- [LOGIC MỚI: CHỌN TẦNG CHÍNH XÁC] ---
            float spawnY = transform.position.y; // Fallback: Nếu quên kéo cả 2 tầng thì lấy vị trí Boss

            // B1: Tạo danh sách chứa những tầng HỢP LỆ (Đã kéo vào Inspector)
            List<Transform> validFloors = new List<Transform>();

            if (floor1Point != null) validFloors.Add(floor1Point);
            if (floor2Point != null) validFloors.Add(floor2Point);

            // B2: Bốc thăm
            if (validFloors.Count > 0)
            {
                // Random.Range với int là exclusive (không lấy cận trên), nên dùng 0 đến Count là chuẩn
                int randomIndex = Random.Range(0, validFloors.Count);
                spawnY = validFloors[randomIndex].position.y;

                // Debug.Log($"Wave {w+1}: Chọn tầng {validFloors[randomIndex].name}");
            }
            // ----------------------------------------

            // 3. Rải thảm từ Limit Left đến Limit Right
            if (movement.limitLeft != null && movement.limitRight != null)
            {
                float startX = Mathf.Min(movement.limitLeft.position.x, movement.limitRight.position.x);
                float endX = Mathf.Max(movement.limitLeft.position.x, movement.limitRight.position.x);

                // Rải từ trái qua phải
                for (float x = startX; x <= endX; x += rainSpacing)
                {
                    Vector3 pos = new Vector3(x, spawnY, 0);
                    Instantiate(fireZonePrefab, pos, Quaternion.identity);

                    // Rải rất nhanh (theo biến rainSpawnDelay)
                    yield return new WaitForSeconds(rainSpawnDelay);
                }
            }

            // Nghỉ giữa các đợt (theo biến waveDelay)
            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator PerformRetreat()
    {
        // --- BỔ SUNG DÒNG NÀY ---
        justAttacked = true;
        // ------------------------

        Transform safeSpot = movement.GetFarthestPoint(player.position);
        yield return StartCoroutine(movement.Action_Teleport(safeSpot.position));
    }

    bool IsPlayerInLimit()
    {
        if (movement.limitLeft == null || movement.limitRight == null) return true;
        float minX = Mathf.Min(movement.limitLeft.position.x, movement.limitRight.position.x);
        float maxX = Mathf.Max(movement.limitLeft.position.x, movement.limitRight.position.x);
        return (player.position.x >= minX && player.position.x <= maxX);
    }

    void OnDrawGizmosSelected()
    {
        // 1. Vòng Detection (Màu Vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 2. Vòng Kích hoạt Đánh (Màu Đỏ Nhạt - Dây)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Xác định hướng mặt (Trái/Phải)
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 3. Vùng Đánh Cận Chiến - Attack 1 (Màu Đỏ Đậm - Khối)
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 meleeCenter = transform.position + new Vector3(meleeBoxOffset.x * direction, meleeBoxOffset.y, 0);
        Gizmos.DrawCube(meleeCenter, meleeBoxSize);

        // --- VẼ VÙNG SPAWN FIREBALL (MÀU XANH) ---
        Gizmos.color = new Color(0, 1, 1, 0.4f); // Màu Cyan bán trong suốt
        Vector3 spawnCenter = transform.position + new Vector3(spawnAreaOffset.x * direction, spawnAreaOffset.y, 0);
        Gizmos.DrawCube(spawnCenter, spawnAreaSize);
        // -----------------------------------------------
    }

    // Hàm mới: Bắn tia từ Player xuống đất để lấy tọa độ sàn nhà
    // Giúp Boss không bị "ngáo" nhắm bắn lên trời khi Player nhảy
    Vector3 GetPredictedGroundPos(Vector3 targetPos)
    {
        // Bắn tia từ cao hơn vị trí Player một chút (0.5m) để tránh Start Inside Collider
        Vector3 rayOrigin = targetPos + Vector3.up * 0.5f;

        // Bắn tia xuống dưới 10.5m (10m + 0.5m offset)
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 10.5f, movement.groundLayer);

        if (hit.collider != null)
        {
            // hit.point là mặt trên của sàn (tọa độ Y chính xác)
            return new Vector3(targetPos.x, hit.point.y, targetPos.z);
        }

        // Trường hợp Player đang bay qua vực thẳm
        return targetPos;
    }

    // Hàm tìm vị trí thoáng (không bị trần nhà chặn) để di chuyển ra đó rồi nhảy
    Vector3? FindClearPositionForJump(Vector3 targetPos)
    {
        float scanStep = 1.0f; // Mỗi bước quét 1 mét
        int maxSteps = 6;      // Quét tối đa 6 mét sang mỗi bên

        // Ưu tiên quét về phía Player đang đứng
        float direction = (targetPos.x > transform.position.x) ? 1f : -1f;

        // Quét 2 bên: Hướng về Player trước, sau đó hướng ngược lại
        for (int side = 0; side < 2; side++)
        {
            float dir = (side == 0) ? direction : -direction;

            for (int i = 1; i <= maxSteps; i++)
            {
                // Vị trí check dưới chân (sang ngang i mét)
                Vector3 checkPos = transform.position + new Vector3(dir * i * scanStep, 0, 0);

                // Kẹp vào trong map limit
                if (movement.limitLeft != null && checkPos.x < movement.limitLeft.position.x) continue;
                if (movement.limitRight != null && checkPos.x > movement.limitRight.position.x) continue;

                // 1. Check đất: Phải có đất để đi bộ tới đó
                RaycastHit2D groundHit = Physics2D.Raycast(checkPos + Vector3.up * 0.5f, Vector2.down, 2f, movement.groundLayer);
                if (groundHit.collider == null) continue; // Hố -> Bỏ qua

                // 2. CHECK TRẦN NHÀ (QUAN TRỌNG)
                // Bắn tia từ độ cao ngang đầu (y + 1.5f) lên trời
                Vector3 ceilingRayOrigin = checkPos + Vector3.up * 1.5f;

                // Check cả Obstacle (Tường) và Ground (Sàn tầng trên)
                // Lưu ý: obstacleLayer trong MovementAI phải bao gồm layer của sàn tầng 2
                RaycastHit2D ceilingHit = Physics2D.Raycast(ceilingRayOrigin, Vector2.up, 3.0f, movement.obstacleLayer | movement.groundLayer);

                // Vẽ debug để bạn thấy nó đang quét
                // Debug.DrawLine(ceilingRayOrigin, ceilingRayOrigin + Vector3.up * 3.0f, Color.yellow, 0.1f);

                if (ceilingHit.collider == null)
                {
                    // KHÔNG CÓ TRẦN -> Đây là mép sàn hoặc lỗ hổng!
                    // Debug.DrawLine(transform.position, checkPos, Color.green, 1.0f);
                    return checkPos;
                }
            }
        }

        return null; // Không tìm thấy chỗ nào thoáng
    }

    Transform FindNearestWalkableNode()
    {
        if (movement.patrolPoints == null) return null;

        Transform bestNode = null;
        float minDistance = Mathf.Infinity;
        float currentY = transform.position.y;

        foreach (Transform point in movement.patrolPoints)
        {
            // 1. Kiểm tra Cooldown: Nếu điểm này chưa hết giờ phạt -> Bỏ qua
            if (nodeCooldowns.ContainsKey(point))
            {
                if (Time.time < nodeCooldowns[point]) continue;
            }

            // 2. Chỉ lấy điểm cùng tầng (Chênh lệch độ cao < 1.0f)
            if (Mathf.Abs(point.position.y - currentY) > 1.0f) continue;

            // 3. Bỏ qua điểm đang đứng (Khoảng cách < 1.0f)
            float dist = Vector2.Distance(transform.position, point.position);
            if (dist < 1.0f) continue;

            // 4. Tìm điểm gần nhất
            if (dist < minDistance)
            {
                minDistance = dist;
                bestNode = point;
            }
        }
        return bestNode;
    }
}