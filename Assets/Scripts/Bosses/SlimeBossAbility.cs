using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(Rigidbody2D))]
public class SlimeBossAbility : MonoBehaviour
{
    [Header("Mana & Cooldown")]
    public float manaCost = 220f;
    public float cooldown = 30f;

    [Header("Leap Settings")]
    public float leapHeight = 5f;
    public float fallSpeed = 10f;
    public float preLeapDelay = 1f;
    public float timeBetweenJumps = 8f;
    public float stunDuration = 3f;

    [Header("Landing Area")]
    public List<Transform> customJumpPoints; // nếu có, chọn ngẫu nhiên
    [Header("Box Area")]
    public Transform boxCornerA;
    public Transform boxCornerB;

    [Header("Return Settings")]
    public float returnDistanceX = 3f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform shotPointLeft;
    public Transform shotPointRight;

    private Entity healthComp;
    private Rigidbody2D rb;
    private SlimeBossAI ai;
    private Animator animator;
    private bool onCooldown = false;

    void Awake()
    {
        healthComp = GetComponent<Entity>();
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<SlimeBossAI>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!ai.IsInZone(ai.player.position))
        {
            return;
        }

        if (!onCooldown && healthComp.currentMana >= manaCost && ai.player)
            StartCoroutine(PerformAbility());
    }

    /// <summary>
    /// Skill 1: Nhảy giữa phòng, tạo shockwave, và choáng.
    /// </summary>
    private IEnumerator PerformAbility()
    {
        onCooldown = true;
        healthComp.UseMana(manaCost);
        ai.canMove = false;

        // pre-leap delay
        yield return new WaitForSeconds(preLeapDelay);

        // tính điểm trung tâm
        Vector3 center = (ai.zonePointA.position + ai.zonePointB.position) * 0.5f;
        float dirX = Mathf.Sign(center.x - transform.position.x);
        float distX = Mathf.Abs(center.x - transform.position.x);
        float apexTime = leapHeight / (-Physics2D.gravity.y * rb.gravityScale);
        float horSpeed = distX / apexTime;

        // leap
        rb.linearVelocity = new Vector2(dirX * horSpeed, leapHeight);
        yield return new WaitForSeconds(apexTime);

        // slam
        rb.linearVelocity = new Vector2(0f, -fallSpeed);
        yield return new WaitUntil(() => Physics2D.OverlapCircle(ai.groundCheck.position, ai.groundCheckRadius, ai.groundLayer));
        SpawnShockwave();

        // jumpslam lặp
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(timeBetweenJumps);
            rb.linearVelocity = new Vector2(0f, leapHeight);
            yield return new WaitForSeconds(apexTime);
            rb.linearVelocity = new Vector2(0f, -fallSpeed);
            yield return new WaitUntil(() => Physics2D.OverlapCircle(ai.groundCheck.position, ai.groundCheckRadius, ai.groundLayer));
            SpawnShockwave();
        }

        // stun
        yield return new WaitForSeconds(stunDuration);
        ai.canMove = true;

        // cooldown
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    /// <summary>
    /// Skill 2: Jump-slam tới điểm random trong customJumpPoints hoặc trong hộp boxCornerA, boxCornerB.
    /// </summary>
    public void TriggerJumpToZone(Transform zoneA, Transform zoneB, Vector3 playerPos)
    {
        StartCoroutine(JumpSlamReturn(zoneA, zoneB, playerPos));
    }

    private IEnumerator JumpSlamReturn(Transform zoneA, Transform zoneB, Vector3 playerPos)
    {
        ai.canMove = false;
        yield return new WaitForSeconds(preLeapDelay);

        // chọn điểm đáp
        Vector3 chosen;
        if (customJumpPoints != null && customJumpPoints.Count > 0)
        {
            chosen = customJumpPoints[Random.Range(0, customJumpPoints.Count)].position;
        }
        else if (boxCornerA != null && boxCornerB != null)
        {
            float xMin = Mathf.Min(boxCornerA.position.x, boxCornerB.position.x);
            float xMax = Mathf.Max(boxCornerA.position.x, boxCornerB.position.x);
            float yMin = Mathf.Min(boxCornerA.position.y, boxCornerB.position.y);
            float yMax = Mathf.Max(boxCornerA.position.y, boxCornerB.position.y);
            chosen = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), transform.position.z);
        }
        else
        {
            chosen = (zoneA.position + zoneB.position) * 0.5f;
        }

        // leap
        float dir = Mathf.Sign(chosen.x - transform.position.x);
        float dist = Mathf.Abs(chosen.x - transform.position.x);
        float apex = leapHeight / (-Physics2D.gravity.y * rb.gravityScale);
        float speed = dist / apex;
        rb.linearVelocity = new Vector2(dir * speed, leapHeight);
        yield return new WaitForSeconds(apex);

        // slam
        rb.linearVelocity = new Vector2(0f, -fallSpeed);
        yield return new WaitUntil(() => Physics2D.OverlapCircle(ai.groundCheck.position, ai.groundCheckRadius, ai.groundLayer));
        SpawnShockwave();

        // move về player
        float retDir = Mathf.Sign(playerPos.x - transform.position.x);
        rb.linearVelocity = new Vector2(retDir * speed, rb.linearVelocity.y);
        yield return new WaitForSeconds(returnDistanceX / speed);
        rb.linearVelocity = Vector2.zero;

        ai.canMove = true;
    }

    private void SpawnShockwave()
    {
        if (projectilePrefab == null) return;
        var pL = Instantiate(projectilePrefab, shotPointLeft.position, Quaternion.identity);
        pL.transform.localScale = new Vector3(Mathf.Abs(pL.transform.localScale.x), pL.transform.localScale.y, pL.transform.localScale.z);
        pL.GetComponent<BossProjectile>()?.Activate(-1);
        var pR = Instantiate(projectilePrefab, shotPointRight.position, Quaternion.identity);
        pR.transform.localScale = new Vector3(-Mathf.Abs(pR.transform.localScale.x), pR.transform.localScale.y, pR.transform.localScale.z);
        pR.GetComponent<BossProjectile>()?.Activate(1);
        AudioManager.Sfx(Sound.SlimeBossSlam);
    }
}
