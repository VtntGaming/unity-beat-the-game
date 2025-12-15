using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    private Animator animator;
    private bool isPlayerNearby = false;
    private bool isOpen = false;

    private DropTable dropTable;
    private Collider2D chestCollider;
    private SpriteRenderer chestSprite;

    void Awake()
    {
        animator = GetComponent<Animator>();
        dropTable = GetComponent<DropTable>();
        chestCollider = GetComponent<Collider2D>();
        chestSprite = GetComponent<SpriteRenderer>();

        if (animator == null)
            Debug.LogError("ChestOpen: Không tìm thấy Animator!");
        if (dropTable == null)
            Debug.LogError("ChestOpen: Không tìm thấy DropTable!");
        if (chestCollider == null)
            Debug.LogError("ChestOpen: Không tìm thấy Collider2D!");
        if (chestSprite != null)
            chestSprite.sortingOrder = -1; // Luôn phía sau Player


        display = transform.Find("Interactable");
    }

    float t = 0;    // Tiến độ quá trình hiển thị [0-1]
    readonly float animateTime = 0.25f; // Thời gian hoạt ảnh
    readonly float distanceChecking = 1f; // Khoảng cách từ rương đến người chơi
    Transform display;

    void Update()
    {
        GameObject player = GameObject.Find("Player");

        Vector2 distance = player.transform.position - transform.position;

        if (distance.magnitude <= distanceChecking && !isOpen)
        {
            t += Time.deltaTime / animateTime;
            if (Input.GetKeyDown(KeyCode.F))
                OpenChest();
        }
        else
            t -= Time.deltaTime / animateTime;

        t = Mathf.Clamp(t, 0, 1);
        float t2 = Mathf.Lerp(0.75f, 1, t);
        display.GetComponent<CanvasGroup>().alpha = t;
        display.Find("Base").transform.localScale = new Vector2(t2, t2);
    }

    private void OpenChest()
    {
        isOpen = true;
        animator.SetBool("Open", true);
        AudioManager.Sfx(Sound.OpenChest);
    }

    public void SpawnLoot()
    {
        if (dropTable != null)
            dropTable.DropLoot();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            isPlayerNearby = true;

            // Cho phép đi xuyên bằng cách bỏ qua va chạm
            Physics2D.IgnoreCollision(other, chestCollider, true);

            // Nếu Player có SpriteRenderer, đảm bảo nằm trước
            SpriteRenderer otherSprite = other.GetComponent<SpriteRenderer>();
            if (otherSprite != null && chestSprite != null && otherSprite.sortingOrder <= chestSprite.sortingOrder)
            {
                otherSprite.sortingOrder = chestSprite.sortingOrder + 1;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            isPlayerNearby = false;

            // Cho phép va chạm trở lại nếu cần
            Physics2D.IgnoreCollision(other, chestCollider, false);
        }
    }
}
