using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    public float speed = 3f;
    public bool chase = false;
    public Transform startingPoint;

    private GameObject player;
    private Rigidbody2D rb;
    private Entity entity;
    private BeeAttack beeAttack;   // ⭐ thêm dòng này

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        entity = GetComponent<Entity>();
        beeAttack = GetComponent<BeeAttack>();    // ⭐ gán BeeAttack
    }

    void Update()
    {
        if (player == null) return;
        if (entity != null && entity.dead) return;

        // Nếu đang attack → không di chuyển
        if (beeAttack != null && beeAttack.isAttacking) return;

        if (chase)
        {
            Chase();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ReturnStartPoint();
        }

        Flip();
    }


    private void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position,
            player.transform.position,
            speed * Time.deltaTime);
    }

    private void ReturnStartPoint()
    {
        transform.position = Vector2.MoveTowards(transform.position,
            startingPoint.position,
            speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (player == null) return;

        if (transform.position.x > player.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}
