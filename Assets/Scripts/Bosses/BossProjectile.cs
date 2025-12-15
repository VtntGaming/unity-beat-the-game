using UnityEngine;

/// <summary>
/// Projectile used by SlimeBossAbility. Inherits damage logic from EnemyDamage.
/// Spawns, travels horizontally, persists until lifetime expires, and damages Player on touch.
/// Plays Idle animation on launch.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : EnemyDamage
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public float resetTime = 5f;

    private float lifetime;
    private Collider2D coll;
    private Rigidbody2D rb;
    private Animator anim;
    private int direction = 1;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        coll.isTrigger = true;
        rb.gravityScale = 0f;
    }

    /// <summary>
    /// Activate and launch the projectile.
    /// dir: -1 = left, +1 = right.
    /// </summary>
    public void Activate(int dir)
    {
        lifetime = 0f;
        direction = dir;
        gameObject.SetActive(true);
        coll.enabled = true;

        // Play Idle animation if exists
        if (anim != null)
            anim.Play("Idle");
    }

    void Update()
    {
        // Move horizontally
        rb.linearVelocity = new Vector2(direction * speed, 0f);

        // Lifetime expire
        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            Deactivate();
    }

    private new void OnTriggerEnter2D(Collider2D collision)
    {
        // ignore effect zones and self
        if (collision.CompareTag("EffectZone")) return;
        if (collision.GetComponent<BossProjectile>() != null) return;

        // damage target
        if (collision.CompareTag("Player"))
        {
            base.OnTriggerEnter2D(collision); // apply damage & knockback
        }

        // do not destroy, keep moving
    }

    /// <summary>
    /// Deactivate projectile and reset velocity.
    /// </summary>
    public void Deactivate()
    {
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }
}