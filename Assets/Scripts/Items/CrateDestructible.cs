using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CrateDestructible : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Shard Settings")]
    public bool spawnShards = true;
    public List<GameObject> shardPrefabs = new List<GameObject>();
    public int shardCount = 5;

    private Collider2D crateCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        currentHealth = maxHealth;
        crateCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Optional: auto push sortingOrder to back
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = -1; // Player should be 0 or higher
        }
    }

    public void TakeDamage(float damage, float attackerDirection = 0f, GameObject attacker = null, bool isProjectile = false)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Explode();
    }

    private void Explode()
    {
        if (spawnShards && shardPrefabs != null && shardPrefabs.Count > 0)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;

                GameObject chosenPrefab = shardPrefabs[Random.Range(0, shardPrefabs.Count)];

                GameObject shard = Instantiate(chosenPrefab, transform.position, Quaternion.identity);
                if (shard.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    float force = Random.Range(2f, 5f);
                    rb.AddForce(dir * force, ForceMode2D.Impulse);
                }
            }
        }

        var drop = GetComponent<DropTable>();
        if (drop != null)
            drop.DropLoot();
        else
            Debug.LogError("No DropTable found!");

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryIgnoreCollisionWithTag(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryIgnoreCollisionWithTag(other);
    }

    private void TryIgnoreCollisionWithTag(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(crateCollider, other, true);
        }
    }
}
