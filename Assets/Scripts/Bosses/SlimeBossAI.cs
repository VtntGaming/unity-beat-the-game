using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(SlimeBossAbility))]
public class SlimeBossAI : MonoBehaviour
{
    [Header("Zone")]
    public Transform zonePointA;
    public Transform zonePointB;

    [Header("Movement")]
    public float runSpeed = 3f;
    public float jumpForce = 0f;
    public float jumpHorizontalMultiplier = 1.5f;
    public float jumpRange = 1.5f;

    [Header("Health Buffs")]
    [Range(0f, 1f)] public float firstThreshold = 0.3f;
    [Range(0f, 1f)] public float secondThreshold = 0.1f;
    [Range(0f, 1f)] public float firstBuff = 0.2f;
    [Range(0f, 1f)] public float secondBuff = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public int damage = 1;
    public string playerTag = "Player";
    public int flashCount = 2;
    public float flashDuration = 0.1f;
    public float hitCooldown = 2f;

    [Header("DropLoot")]
    public Transform chestSpawnPoint;
    public GameObject chestPrefab;  // Gán prefab rương ở Inspector


    [Header("Auto Slam")]
    public float autoSlamDelay = 5f; // nếu sau khoảng này chưa hit player, tự động slam

    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D col;
    private Entity health;
    private SpriteRenderer spriteRenderer;
    public Transform player;
    private SlimeBossAbility ability;

    private bool isGrounded;
    private float baseRunSpeed;
    private float baseJumpMult;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private float lastHealth;
    private bool deathHandled;
    private bool isEnraged;
    public bool canMove = true;
    private bool canHit = true;
    private float lastHitTime;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        health = GetComponent<Entity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ability = GetComponent<SlimeBossAbility>();

        baseRunSpeed = runSpeed;
        baseJumpMult = jumpHorizontalMultiplier;
        originalColor = spriteRenderer.color;
        lastHealth = health.currentHealth;

        GameObject p = GameObject.FindWithTag(playerTag);
        player = p?.transform;
        if (!player) Debug.LogError($"No GameObject with tag '{playerTag}' found.");

        lastHitTime = Time.time;
    }

    void Update()
    {
        if (!player || health.dead)
            return;

        UpdateGrounded();
        HandleHealthState();
        FlashIfDamaged();

        // Auto slam if not hit for a while
        if (canMove && Time.time - lastHitTime >= autoSlamDelay)
        {
            ability.TriggerJumpToZone(zonePointA, zonePointB, player.position);
            lastHitTime = Time.time;
        }

        if (!canMove)
            return;

        if (!IsInZone(player.position) || !isGrounded)
        {
            StopMoving();
            return;
        }

        MoveTowardPlayer();
    }

    void UpdateGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    void HandleHealthState()
    {
        float pct = health.currentHealth / health.maxHealth;

        if (!isEnraged && pct <= firstThreshold)
        {
            isEnraged = true;
            spriteRenderer.color = Color.red;
        }

        if (pct <= secondThreshold) ApplyBuff(secondBuff);
        else if (pct <= firstThreshold) ApplyBuff(firstBuff);
        else ResetBuff();

        if (health.dead && !deathHandled)
        {
            deathHandled = true;
            StartCoroutine(HandleDeath());
        }
    }

    void ApplyBuff(float buff)
    {
        runSpeed = baseRunSpeed * (1 + buff);
        jumpHorizontalMultiplier = baseJumpMult * (1 + buff);
    }

    void ResetBuff()
    {
        runSpeed = baseRunSpeed;
        jumpHorizontalMultiplier = baseJumpMult;
    }

    void FlashIfDamaged()
    {
        if (health.currentHealth < lastHealth)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashColor());
            lastHealth = health.currentHealth;
        }
    }

    void MoveTowardPlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        float distance = Mathf.Abs(player.position.x - transform.position.x);

        if (distance <= jumpRange && isGrounded)
            Jump(dir);
        else
            Run(dir);
    }

    void Run(float dir)
    {
        animator.SetBool("isRunning", true);
        animator.ResetTrigger("jump");
        Flip(dir);
        rb.linearVelocity = new Vector2(dir * runSpeed, rb.linearVelocity.y);
    }

    void Jump(float dir)
    {
        animator.SetBool("isRunning", false);
        animator.SetTrigger("jump");
        Flip(dir);
        rb.linearVelocity = new Vector2(dir * runSpeed * jumpHorizontalMultiplier, jumpForce);
    }

    void StopMoving()
    {
        animator.SetBool("isRunning", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !health.dead && canHit)
        {
            // Gây sát thương
            Entity targetHealth = collision.gameObject.GetComponent<Entity>();
            if (targetHealth != null)
            {
                float attackDir = Mathf.Sign(collision.transform.position.x - transform.position.x);
                targetHealth.TakeDamage(damage, attackDir, gameObject, false);

                // Trigger jump-to-zone ability (no mana cost)
                ability.TriggerJumpToZone(zonePointA, zonePointB, player.position);

                // Bắt đầu cooldown hit
                StartCoroutine(HitCooldownRoutine(collision.collider));
            }
        }
    }

    private IEnumerator HitCooldownRoutine(Collider2D playerCollider)
    {
        canHit = false;
        // Ignore collisions
        Physics2D.IgnoreCollision(col, playerCollider, true);
        yield return new WaitForSeconds(hitCooldown);
        Physics2D.IgnoreCollision(col, playerCollider, false);
        canHit = true;
    }

    IEnumerator FlashColor()
    {
        Color flashColor = Color.Lerp(originalColor, Color.red, 0.5f);
        Color finalColor = isEnraged ? Color.red : originalColor;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = finalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        spriteRenderer.color = finalColor;
        flashCoroutine = null;
    }

    IEnumerator HandleDeath()
    {
        StopMoving();
        animator.Play("Death");
        col.enabled = false;
        GameObject portal = GameObject.FindWithTag("Portal");
        if (portal)
            portal.SetActive(true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    public bool IsInZone(Vector3 pos)
    {
        if (!zonePointA || !zonePointB) return false;

        float xMin = Mathf.Min(zonePointA.position.x, zonePointB.position.x);
        float xMax = Mathf.Max(zonePointA.position.x, zonePointB.position.x);
        float yMin = Mathf.Min(zonePointA.position.y, zonePointB.position.y);
        float yMax = Mathf.Max(zonePointA.position.y, zonePointB.position.y);

        return pos.x >= xMin && pos.x <= xMax && pos.y >= yMin && pos.y <= yMax;
    }

    void Flip(float direction)
    {
        if (direction == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (zonePointA && zonePointB)
        {
            Gizmos.color = Color.red;
            Vector3 center = (zonePointA.position + zonePointB.position) * 0.5f;
            Vector3 size = new Vector3(
                Mathf.Abs(zonePointA.position.x - zonePointB.position.x),
                Mathf.Abs(zonePointA.position.y - zonePointB.position.y),
                1f);
            Gizmos.DrawWireCube(center, size);
        }
    }

    public void SpawnChest()
    {
        if (chestPrefab != null && chestSpawnPoint != null)
        {
            Instantiate(chestPrefab, chestSpawnPoint.position, Quaternion.identity);
            Debug.Log("Spawn chest at " + chestSpawnPoint.position);
        }
        else
        {
            Debug.LogWarning("chestPrefab hoặc chestSpawnPoint chưa gán!");
        }
    }
}
