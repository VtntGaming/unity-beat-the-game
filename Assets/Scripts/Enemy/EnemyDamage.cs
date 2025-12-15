using UnityEngine;

public class EnemyDamage : EnermyEntity
{
    [SerializeField] protected float damage;

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Entity playerHealth = collision.GetComponent<Entity>();
            if (playerHealth != null)
            {
                // Xác định hướng của quái vật
                float attackerDirection = Mathf.Sign(transform.localScale.x);

                // In ra hướng của quái vật để kiểm tra
                Debug.Log("Enemy Direction: " + attackerDirection + " (localScale.x: " + transform.localScale.x + ")");

                playerHealth.TakeDamage(damage, attackerDirection);

                // Gọi KnockbackController để áp dụng knockback cho người chơi
                //playerHealth.GetComponent<KnockbackController>().ApplyKnockback(attackerDirection);
            }
        }
    }
}
