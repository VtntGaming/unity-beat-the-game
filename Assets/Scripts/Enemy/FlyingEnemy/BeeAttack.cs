using UnityEngine;
using System.Collections;
public class BeeAttack : MonoBehaviour
{
    public Transform player;
    public float prepareTime = 1f;
    public float attackSpeed = 8f;
    public Collider2D beeDamageZone; // ⭐ Thêm reference

    private Rigidbody2D rb;
    [HideInInspector] public bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ⭐ TẮT damage zone ban đầu
        if (beeDamageZone != null)
            beeDamageZone.enabled = false;
    }

    public void PlayerEnteredAttackZone()
    {
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    public void PlayerExitedAttackZone()
    {
        isAttacking = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // ⭐ TẮT damage zone khi thoát
        if (beeDamageZone != null)
            beeDamageZone.enabled = false;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(prepareTime);

        // ⭐ BẬT damage zone khi bắt đầu lao
        if (beeDamageZone != null)
            beeDamageZone.enabled = true;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * attackSpeed;

        yield return new WaitForSeconds(0.6f);

        // ⭐ TẮT damage zone sau khi lao xong
        if (beeDamageZone != null)
            beeDamageZone.enabled = false;

        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
    }
}