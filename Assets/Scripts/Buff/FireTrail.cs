using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FireTrail : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 20f;
    public float lifetime = 2f;
    public float tickInterval = 0.9f;      // time between repeated damage ticks

    [Header("Physics Settings")]
    public float dropSpeed = 10f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool hasLanded = false;

    // track enemies standing on this trail
    private readonly List<Entity> enemiesOnTrail = new List<Entity>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * dropSpeed;

        // destroy after lifetime
        Destroy(gameObject, lifetime);

        // begin repeating damage
        StartCoroutine(DamageTick());
    }

    IEnumerator DamageTick()
    {
        // wait initial tick interval
        yield return new WaitForSeconds(tickInterval);
        while (true)
        {
            // apply damage to all enemies currently on trail
            for (int i = enemiesOnTrail.Count - 1; i >= 0; --i)
            {
                Entity hp = enemiesOnTrail[i];
                if (hp == null)
                {
                    enemiesOnTrail.RemoveAt(i);
                    continue;
                }
                // damage and no knockback (direction 0)
                hp.TakeDamage(damage, 0f, this.gameObject, false);
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu object là projectile thì bỏ qua luôn
        if (other.CompareTag("Projectile"))
            return;

        // if hits ground, stop falling
        if (!hasLanded && ((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            hasLanded = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }

        // only track enemies
        if (other.CompareTag("Enemy"))
        {
            Entity hp = other.GetComponent<Entity>();
            if (hp != null && !enemiesOnTrail.Contains(hp))
                enemiesOnTrail.Add(hp);
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Entity hp = other.GetComponent<Entity>();
            if (hp != null)
                enemiesOnTrail.Remove(hp);
        }
    }
}
