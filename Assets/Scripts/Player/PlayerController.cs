using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int coinCount = 0;
    private PlayerRespawn playerRespawn;

    private void Awake()
    {
        coinCount = 99999;

        // --- [SỬA LỖI TẠI ĐÂY] ---
        // Lệnh này PHẢI nằm ở Awake, không được để trong AddCoin
        playerRespawn = GetComponent<PlayerRespawn>();

        if (playerRespawn == null)
            Debug.LogError("❌ LỖI: Không tìm thấy script PlayerRespawn trên nhân vật!");
        else
            Debug.Log("✅ PlayerController đã kết nối thành công với PlayerRespawn.");
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        //Debug.Log("Coin collected! Total coins: " + coinCount);
    }

    public int getCoin()
    {
        return coinCount;
    }

    public void SpendCoin(int amount)
    {
        coinCount = Mathf.Max(0, coinCount - amount);
    }

    private BuffManager buffManager;

    private void Start()
    {
        // Tìm BuffManager trong scene
        buffManager = GetComponent<BuffManager>();

        if (buffManager == null)
        {
            Debug.LogError("BuffManager not found on player!");
        }
    }

    // Gọi khi nhận buff
    public void ApplyBuff(BuffType buff)
    {
        if (buffManager != null)
        {
            buffManager.ApplyBuff(buff);
        }
    }

    // Gọi khi player chết
    public void Die()
    {
        if (buffManager != null) buffManager.RemoveAllBuffs();

        Debug.Log("Player died!");

        // Thay vì chỉ log, ta gọi sang PlayerRespawn để xử lý mạng
        if (playerRespawn != null)
        {
            playerRespawn.HandleDeath(); // [MỚI] Gọi hàm này
        }
    }
}
