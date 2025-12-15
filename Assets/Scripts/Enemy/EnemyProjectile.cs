using UnityEngine;

public class EnemyProjectile : EnemyDamage
{
    [SerializeField] private float speed;
    [SerializeField] private float resetTime;
    private float lifetime;
    private Animator anim;
    private BoxCollider2D coll;

    private bool hit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    public void ActivateProjectile()
    {
        hit = false;
        lifetime = 0;
        gameObject.SetActive(true);
        coll.enabled = true;
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            gameObject.SetActive(false);
    }

    private new void OnTriggerEnter2D(Collider2D collision)
    {
        // ✅ Bỏ qua mọi vùng hiệu ứng nếu có tag "EffectZone"
        if (collision.CompareTag("EffectZone"))
            return;

        if (collision.CompareTag("Projectile"))
            return;

        // ✅ Nếu là Player và đang Dash, thì bỏ qua va chạm
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<BasicMovement>();
            if (player != null && player.IsDashing)
                return;
        }

        // ✅ Bỏ qua projectile khác nếu muốn
        if (collision.GetComponent<EnemyProjectile>() != null)
            return;

        hit = true;
        base.OnTriggerEnter2D(collision);
        coll.enabled = false;

        if (anim != null)
            anim.SetTrigger("Explode");
        else
            gameObject.SetActive(false);

        if (collision.CompareTag("Player"))
        {
            Entity playerHealth = collision.GetComponent<Entity>();
            if (playerHealth != null)
            {
                float direction = Mathf.Sign(transform.localScale.x);
                playerHealth.TakeDamage(damage, direction, gameObject, true);

                KnockbackController kb = playerHealth.GetComponent<KnockbackController>();
                if (kb != null)
                    kb.ApplyKnockback(direction, false);
            }
        }
    }


    public void Explode()
    {
        if (hit) return; // prevent double explode
        hit = true;
        coll.enabled = false;

        if (anim != null)
            anim.SetTrigger("Explode");
        else
            gameObject.SetActive(false);
    }


    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}