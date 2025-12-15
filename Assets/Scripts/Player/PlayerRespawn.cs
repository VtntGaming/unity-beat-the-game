using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Lives System")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverCanvasPrefab;
    [SerializeField] private float respawnDelay = 1.5f;

    // Biến để lưu kết nối tới UI hiển thị mạng
    private LivesDisplay livesDisplayUI;

    private Transform currentCheckpoint;
    private Entity playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<Entity>();
        currentLives = maxLives;
    }

    [System.Obsolete]
    private void Start()
    {
        // 1. Logic tìm Checkpoint (Giữ nguyên cũ)
        GameObject interactObj = GameObject.Find("InteractObject");
        if (interactObj != null)
        {
            Transform spawnPoint = interactObj.transform.Find("CheckSpawnPoint")?.Find("StartCheckpoint");
            if (spawnPoint != null)
            {
                SetCheckpoint(spawnPoint);
                Respawn();
            }
        }

        // 2. [MỚI] Tự động tìm UI hiển thị mạng trong Scene
        livesDisplayUI = FindObjectOfType<LivesDisplay>();

        if (livesDisplayUI != null)
        {
            // Cập nhật hiển thị ngay lập tức khi vào game (VD: x3)
            livesDisplayUI.UpdateLives(currentLives);
        }
        else
        {
            // Không lỗi game, chỉ báo warning để biết đường thêm UI
            Debug.LogWarning("⚠️ Không tìm thấy script LivesDisplay trong Scene. Bạn đã tạo UI LivesPanel chưa?");
        }
    }

    public void HandleDeath()
    {
        currentLives--;
        Debug.Log("⬇️ Mạng còn lại: " + currentLives);

        // [MỚI] Cập nhật lên UI ngay khi bị trừ mạng
        if (livesDisplayUI != null)
        {
            livesDisplayUI.UpdateLives(currentLives);
        }

        if (currentLives > 0)
        {
            StartCoroutine(RespawnProcess());
        }
        else
        {
            StartCoroutine(GameOverProcess());
        }
    }

    // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
    private IEnumerator RespawnProcess()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private IEnumerator GameOverProcess()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (gameOverCanvasPrefab != null)
        {
            Instantiate(gameOverCanvasPrefab, Vector3.zero, Quaternion.identity);
            Time.timeScale = 0f;
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Screen");
    }

    public void Respawn()
    {
        playerHealth.Respawn();
        if (currentCheckpoint != null) transform.position = currentCheckpoint.position + new Vector3(0, 1, 0);
        AudioManager.Sfx(Sound.revive);
    }

    void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
        if (checkpoint.GetComponent<Collider2D>()) checkpoint.GetComponent<Collider2D>().enabled = false;
        if (checkpoint.GetComponent<Animator>()) checkpoint.GetComponent<Animator>().SetTrigger("Interact");
        AudioManager.Sfx(Sound.Checkpoint);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "CheckPoint") SetCheckpoint(collision.transform);
    }
}