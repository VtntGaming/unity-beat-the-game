using UnityEngine;

public class ChaseControl : MonoBehaviour
{
    public FlyingEnemy[] enemyArray;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (FlyingEnemy enemy in enemyArray)
            {
                // ⭐ CHECK NULL trước khi dùng
                if (enemy == null) continue;

                enemy.chase = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (FlyingEnemy enemy in enemyArray)
            {
                // ⭐ CHECK NULL trước khi dùng
                if (enemy == null) continue;

                enemy.chase = false;

                BeeAttack attack = enemy.GetComponent<BeeAttack>();
                if (attack != null)
                {
                    attack.isAttacking = false;

                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                    if (rb != null)  // ⭐ An toàn hơn
                        rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }
}