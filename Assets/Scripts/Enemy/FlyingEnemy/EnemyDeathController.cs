using UnityEngine;
using System.Collections;

public class EnemyDeathController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private FlyingEnemy fe;
    private EnemyDamageDot dot;
    private Entity entity;
    private SpriteRenderer sr;

    public float disappearDelay = 2f;
    private bool handled = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        fe = GetComponent<FlyingEnemy>();
        dot = GetComponent<EnemyDamageDot>();
        entity = GetComponent<Entity>();
        sr = GetComponent<SpriteRenderer>();  // ⚡ cần để nhấp nháy
    }

    void Update()
    {
        if (entity.dead && !handled)
        {
            HandleDeath();
        }
    }

    void HandleDeath()
    {
        handled = true;

        // Tắt AI
        if (fe != null) fe.enabled = false;

        // Tắt DOT và Collider DOT
        if (dot != null)
        {
            dot.enabled = false;
            Collider2D dotCol = dot.GetComponent<Collider2D>();
            if (dotCol != null) dotCol.enabled = false;
        }

        // Animation chết
        anim.SetBool("dead", true);

        // Làm rơi xuống
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(0, -2f);

        // Bắt đầu nhấp nháy
        StartCoroutine(BlinkRoutine());

        // Bắt đầu routine biến mất
        StartCoroutine(DeathRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true) // nhấp nháy cho đến khi object bị Destroy
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.1f);

            sr.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DeathRoutine()
    {
        // Chờ rơi xuống (velocity gần 0 = chạm đất)
        yield return new WaitUntil(() => Mathf.Abs(rb.linearVelocity.y) < 0.1f);

        yield return new WaitForSeconds(disappearDelay);

        Destroy(gameObject);
    }
}
