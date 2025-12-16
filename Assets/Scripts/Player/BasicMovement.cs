using UnityEngine;
using System.Collections;

public class BasicMovement : MonoBehaviour
{
    // ===================== Movement Parameters =====================
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 8f;                // Horizontal movement speed
    [SerializeField] public float jumpForce = 10f;               // Force for a normal jump
    [SerializeField] public float fastFallSpeed = 20f;           // Speed applied when fast falling
    [SerializeField] public float fallMultiplier = 3f;           // Multiplier for gravity when falling
    [SerializeField] public float lowJumpMultiplier = 2f;        // Multiplier for gravity on a low jump

    // ===================== Wall Interaction =====================
    [Header("Wall Interaction")]
    [SerializeField] public float wallJumpForce = 5.7f;          // Increased to jump over 2 units
    [SerializeField] public float wallSlideSpeed = 3f;           // Speed when sliding on a wall

    // ===================== Glide Settings =====================
    [Header("Glide Settings")]
    [SerializeField] public float glideFallSpeed = 2f;           // Fall speed while gliding
    [SerializeField] public float glideMoveSpeed = 5f;           // Horizontal move speed while gliding

    // ===================== Dash Settings =====================
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 7f;            // Distance to dash horizontally
    [SerializeField] private float dashDuration = 0.2f;          // Duration of dash movementperform
    [SerializeField] public float dashCooldownTime = 5f;         // Cooldown time for ground dash
    [SerializeField] public float minCooldown = 0.5f;           // Minimum cooldown after buff
    public float dashCooldown;                              // Current cooldown state
    public bool hasDashCooldownBuff = false;  // Biến theo dõi buff giảm cooldown
    public float originalDashCooldownTime;  // Lưu giá trị cooldown gốc

    // ===================== Roll Settings =====================
    [Header("Roll Settings")]
    [SerializeField] private float rollDuration = 0.5f;          // Duration of the roll
    [SerializeField] private float rollSpeed = 10f;              // Speed of roll
    private bool isRolling = false;                              // Is the player currently rolling

    // ===================== Environment Layers =====================
    [Header("Environment Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // ===================== Components & States =====================
    [Header("Component References")]
    private Animator anim;
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Entity playerHealth;
    private BasicBlocking basicBlocking;

    // ===== THAM CHIẾU BUFFMANAGER =====
    private BuffManager buffManager;

    [Header("Player State")]
    private bool isWallJumping = false;
    private bool isDashing = false;
    private bool canDash = true;

    // ===================== Double Jump =====================
    [Header("Double Jump")]
    public bool hasDoubleJumpUpgrade = false;
    private bool hasDoubleJumped = false;

    // ===================== Wall Jump Timing =====================
    [Header("Wall Jump Grace Timing")]
    private float lastWallTouchedX = float.MinValue;
    //private float lastWallJumpTime = 0f;
    private float lastTouchWallTime = 0f;
    private const float wallJumpGraceTime = 0.2f;

    [Header("Elemental Buffs")]
    public GameObject fireTrailPrefab;    // gán Prefab vệt lửa trong Inspector

    // ===================== Sound =====================
    [Header("Sound")]
    AudioManager audioManager;
    private bool wasFootstepPlaying = false;

    public bool IsDashing => isDashing;

    //[HideInInspector] public bool canMove = true; // mặc định được phép di chuyển

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerHealth = GetComponent<Entity>();
        rb.freezeRotation = true;
        basicBlocking = GetComponent<BasicBlocking>();
        originalDashCooldownTime = dashCooldownTime;  // Lưu giá trị cooldown ban đầu khi game bắt đầu
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        buffManager = GetComponent<BuffManager>();
        if (buffManager == null)
        {
            Debug.LogError("BuffManager not found on Player!");
        }
    }

    void Update()
    {
        //if (!canMove)
        //{
        //    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // giữ y velocity để player không rơi ngay lập tức
        //    return; // không xử lý input
        //}

        if (!playerHealth.dead)
        {
            Move();
            Jump();

            WallSlideAndGlide();

            if (!IsGround())
                FastFall();
            if (IsGround())
            {
                isWallJumping = false;
                lastWallTouchedX = float.MinValue; // Reset vị trí X của tường
                hasDoubleJumped = false; // Reset khi chạm đất
            }

            CheckDashInput();
            CheckRollInput();
            if (basicBlocking != null && basicBlocking.BlockingMovementLock)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // stop horizontal movement
                return; // skip movement input
            }
        }
        updateDashCDTime();
    }

    void updateDashCDTime()
    {
        if (hasDashCooldownBuff)
        {
            // Kiểm tra nếu có buff giảm cooldown, giảm thời gian theo logic của buff manager
            float currentCooldownMax = Mathf.Max(dashCooldownTime, minCooldown);  // Dùng giá trị giảm cooldown nếu có buff
            dashCooldown = Mathf.Clamp(dashCooldown - Time.deltaTime, 0f, currentCooldownMax);
        }
        else
        {
            // Bình thường khi không có buff
            dashCooldown = Mathf.Clamp(dashCooldown - Time.deltaTime, 0f, dashCooldownTime);
        }
    }


    // --- Movement Handling ---
    void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float moveInputForVelocity = moveInput; // Biến riêng để điều chỉnh vận tốc

        bool isRunning = Mathf.Abs(moveInput) > 0.01f;
        bool isGrounded = IsGround();

        if (IsTouchingWall() && CurrentWallX() != float.MinValue )
        {
            float wallX = CurrentWallX();
            // Nếu wall ở bên trái của player và người dùng nhấn D (moveInput > 0), vô hiệu hóa input đó
            if (wallX > transform.position.x && moveInput > 0)
            {
                moveInputForVelocity = 0;
            }
            // Nếu wall ở bên phải của player và người dùng nhấn A (moveInput < 0), vô hiệu hóa input đó
            else if (wallX < transform.position.x && moveInput < 0)
            {
                moveInputForVelocity = 0;
            }
        }

        // Sử dụng moveInputForVelocity để điều chỉnh vận tốc
        if (!isWallJumping)
            rb.linearVelocity = new Vector2(moveInputForVelocity * moveSpeed, rb.linearVelocity.y);

        // Sử dụng moveInput gốc để xoay chiều nhân vật
        if (moveInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (moveInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        anim.SetBool("Run", moveInput != 0);

        // — FOOTSTEP LOOP CONTROL — start only if both running & grounded; stop otherwise
        bool shouldPlayFootsteps = isRunning && isGrounded;

        if (shouldPlayFootsteps && !wasFootstepPlaying)
        {
            AudioManager.FootstepsStart();
        }
        else if (!shouldPlayFootsteps && wasFootstepPlaying)
        {
            AudioManager.FootstepsStop();
        }

        wasFootstepPlaying = shouldPlayFootsteps;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            if (IsGround())
            {
                NormalJump();
                hasDoubleJumped = false; // Reset lại
            }
            else
            {
                bool canWallJump = IsTouchingWall() && CurrentWallX() != float.MinValue;
                if (!canWallJump && (Time.time - lastTouchWallTime) < wallJumpGraceTime)
                {
                    canWallJump = true;
                }

                if (canWallJump)
                {
                    if (lastWallTouchedX == float.MinValue || Mathf.Abs(CurrentWallX() - lastWallTouchedX) > 0.1f)
                    {
                        WallJump();
                        hasDoubleJumped = false; // Reset lại khi wall jump
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
                    }
                }
                else if (hasDoubleJumpUpgrade && !hasDoubleJumped)
                {
                    NormalJump(); // Dùng NormalJump nhưng khi đang ở trên không
                    hasDoubleJumped = true;
                }
            }
        }

        anim.SetBool("Grounded", IsGround());
        anim.SetFloat("AirSpeedY", rb.linearVelocity.y);

        // Điều chỉnh trọng lực cho jump mượt
        if (rb.linearVelocity.y < 0 || isWallJumping)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }


    void NormalJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }


    void WallJump()
    {
        //float jumpDir = (CurrentWallX() < transform.position.x) ? 1f : -1f;
        //float angleRad = Mathf.Deg2Rad * 60f;
        //float k = 2.2f;
        //float vx = wallJumpForce * k * Mathf.Cos(angleRad) * jumpDir;
        //float vy = wallJumpForce * k * Mathf.Sin(angleRad);
        //rb.linearVelocity = new Vector2(vx, vy);
        //lastWallTouchedX = CurrentWallX();
        //lastWallJumpTime = Time.time;
        //if (CurrentWallX() < transform.position.x)
        //{
        //    transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        //}
        //else
        //{
        //    transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        //}
        //isWallJumping = true;
        //anim.SetTrigger("Jump");
    }


    void WallSlideAndGlide()
    {
        // Nếu đang chạm tường và đang rơi xuống
        if (IsTouchingWall() && rb.linearVelocity.y < 0) // Bỏ điều kiện !IsGround()
        {
            // Đặt lại isWallJumping để cho phép điều khiển ngang khi trượt
            isWallJumping = false;

            // Cập nhật thời gian chạm tường
            lastTouchWallTime = Time.time;

            // Bắt đầu trạng thái Wall Slide
            anim.SetBool("WallSlide", true);

            // Gán vận tốc rơi chậm cơ bản khi WallSlide
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

            // Glide logic: Nếu glide đang kích hoạt
            if (/* điều kiện kích hoạt Glide */ true)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -glideFallSpeed);

                // Di chuyển ngang khi đang Glide
                if (CurrentWallX() != float.MinValue)
                {
                    if (CurrentWallX() < transform.position.x && Input.GetKey(KeyCode.D))
                    {
                        rb.linearVelocity = new Vector2(glideMoveSpeed, rb.linearVelocity.y);
                        anim.SetBool("WallSlide", false);  // Thoát Glide
                        anim.Play("Fall");  // Chuyển ngay sang hoạt ảnh Fall
                        return;  // Ngừng xử lý WallSlide
                    }
                    else if (CurrentWallX() >= transform.position.x && Input.GetKey(KeyCode.A))
                    {
                        rb.linearVelocity = new Vector2(-glideMoveSpeed, rb.linearVelocity.y);
                        anim.SetBool("WallSlide", false);  // Thoát Glide
                        anim.Play("Fall");
                        return;
                    }
                }
            }
        }
        else
        {
            // Không đủ điều kiện wall slide hoặc đang chạm đất => dừng hoạt ảnh WallSlide
            anim.SetBool("WallSlide", false);
        }

        // Nếu chạm đất trong lúc Glide, thoát khỏi trạng thái Glide/WallSlide
        if (IsGround())
        {
            anim.SetBool("WallSlide", false);
        }
    }

    // --- Fast Fall Handling ---
    void FastFall()
    {
        if (!IsGround() && Input.GetKey(KeyCode.S))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }
    }

    // ======== Hàm kiểm tra Dash Input ========
    void CheckDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
        {
            StartCoroutine(Dash());
            anim.SetTrigger("Dash");
            AudioManager.Sfx(Sound.Dash);
        }
    }

    // ---- Coroutine Dash() chung cho GroundDash và AirDash ----
    IEnumerator Dash()
    {
        dashCooldown = dashCooldownTime;
        isDashing = true;
        canDash = false;

        Vector2 startPos = rb.position;
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 targetPos = IsGround()
            ? new Vector2(startPos.x + dashDistance * direction, startPos.y)
            : startPos + new Vector2(dashDistance * direction, 0);

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.right * direction, dashDistance, wallLayer);
        if (hit.collider != null)
        {
            float offset = 0.1f;
            targetPos = hit.point + hit.normal * offset;
        }

        float elapsed = 0f;
        float spawnTimer = 0f;
        while (elapsed < dashDuration)
        {
            if (Physics2D.Raycast(rb.position, Vector2.right * direction, 0.1f, wallLayer)) break;

            rb.position = Vector2.Lerp(startPos, targetPos, elapsed / dashDuration);

            // Only spawn fire if both buffs are active
            if (hasDashCooldownBuff && buffManager.hasFireBuff && fireTrailPrefab != null)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= 0.05f)
                {
                    // spawn fire trail slightly above the player’s feet so you can see it
                    Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                    Instantiate(fireTrailPrefab, spawnPos, Quaternion.identity);
                    spawnTimer = 0f;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.position = targetPos;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    void CheckRollInput()
    {
        // Nếu người chơi nhấn Ctrl, đang giữ S, chưa Roll, và đang ở trên mặt đất
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.S)) && !isRolling && IsGround())
        {
            StartCoroutine(PerformRoll());  // Bắt đầu Roll với Coroutine
            anim.SetTrigger("Roll");       // Kích hoạt trigger Roll trong Animator
            AudioManager.Sfx(Sound.Roll);
        }
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;  // Set rolling state
        float initialGravity = rb.gravityScale;  // Save initial gravity scale
        rb.gravityScale = 0;  // Disable gravity during the roll

        float rollDirection = transform.localScale.x;  // Determine roll direction (1 = right, -1 = left)
        float rollTimer = 0f;  // Timer to control roll duration
        capsuleCollider.size = new Vector2(0.75f, 0.75f);
        capsuleCollider.offset = new Vector2(0, 0.425f);

        while (rollTimer < rollDuration)
        {
            // Apply roll movement
            rb.linearVelocity = new Vector2(rollDirection * rollSpeed, rb.linearVelocity.y);

            // Check if character is still on the ground
            if (!IsGround())
            {
                anim.SetBool("Grounded", false);  // Trigger fall if not grounded
                rb.gravityScale = initialGravity;   // Restore gravity
                isRolling = false;                  // Exit rolling state
                capsuleCollider.size = new Vector2(0.75f, 1.3f);
                capsuleCollider.offset = new Vector2(0, 0.7f);
                yield break;                        // End the coroutine
            }

            rollTimer += Time.deltaTime;  // Increment timer
            yield return null;  // Wait for the next frame
        }

        capsuleCollider.size = new Vector2(0.75f, 1.3f);
        capsuleCollider.offset = new Vector2(0, 0.7f);

        // Reset gravity and exit rolling state after roll ends
        rb.gravityScale = initialGravity;
        isRolling = false;
    }

    public bool IsGround()
    {
        float minY = capsuleCollider.bounds.min.y;
        Vector2 checkPos = new Vector2(capsuleCollider.bounds.center.x, minY - 0.1f);
        Vector2 checkSize = new Vector2(capsuleCollider.bounds.size.x * 0.9f, 0.2f);
        Collider2D hit = Physics2D.OverlapBox(checkPos, checkSize, 0f, groundLayer);
        return hit != null;
    }

    private bool IsTouchingWall()
    {
        Vector2 leftPos = (Vector2)capsuleCollider.bounds.center + Vector2.left * 0.5f; // Tăng từ 0.5f lên 0.6f
        Vector2 rightPos = (Vector2)capsuleCollider.bounds.center + Vector2.right * 0.5f;
        Vector2 checkSize = new Vector2(0.01f, capsuleCollider.bounds.size.y * 0.6f);
        bool leftHit = Physics2D.OverlapBox(leftPos, checkSize, 0f, wallLayer) != null;
        bool rightHit = Physics2D.OverlapBox(rightPos, checkSize, 0f, wallLayer) != null;
        return (leftHit || rightHit);
    }

    private float CurrentWallX()
    {
        Vector2 leftPos = (Vector2)capsuleCollider.bounds.center + Vector2.left * 0.6f; // Tăng từ 0.5f lên 0.6f
        Vector2 rightPos = (Vector2)capsuleCollider.bounds.center + Vector2.right * 0.6f;
        Vector2 checkSize = new Vector2(0.2f, capsuleCollider.bounds.size.y * 0.9f);
        RaycastHit2D leftHit = Physics2D.BoxCast(leftPos, checkSize, 0f, Vector2.left, 0f, wallLayer);
        RaycastHit2D rightHit = Physics2D.BoxCast(rightPos, checkSize, 0f, Vector2.right, 0f, wallLayer);

        if (leftHit.collider != null)
        {
            return leftHit.point.x;
        }
        else if (rightHit.collider != null)
        {
            return rightHit.point.x;
        }
        return float.MinValue;
    }

    public bool canAttack()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        return moveInput == 0 && IsGround() && !IsTouchingWall();
    }

    public bool canBlock()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        return moveInput == 0 && IsGround();
    }

    //SOUND SETTING IN ANIMTION EVENT
    // Call this from the Jump animation at the exact frame you want the “whoosh” or initial jump sound.
    public void OnJumpStart()
    {
        Debug.Log("🔊 OnJumpStart event triggered");
        AudioManager.Sfx(Sound.Jump);
    }


    // Call this from your Landing animation when the feet hit the ground.
    public void OnLand()
    {
        AudioManager.Sfx(Sound.Land);
    }

}