using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DropTable : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int minCoinAmount = 1;
    public int maxCoinAmount = 5;

    [Header("Health Potion Settings")]
    public GameObject healthPotionPrefab;
    [Range(0f, 1f)]
    public float healthPotionDropChance = 0.2f; // 20%

    [Serializable]
    public class OrbDrop
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float dropChance = 0.5f; // mặc định 50%
    }

    [Header("Orb Settings (Optional)")]
    [Tooltip("List of orb prefabs with individual drop chances. Leave empty to skip.")]
    public List<OrbDrop> orbDrops = new List<OrbDrop>();

    // ======== THAY ĐỔI CÁCH RỚT TRANG BỊ ========

    [Header("Equipment Settings (Mới)")]

    [Tooltip("Kéo Prefab 'Gói trang bị' (có script EquipmentCollectable) vào đây")]
    public GameObject collectablePrefab; // 👈 Prefab "Gói" chung

    [Tooltip("Tỉ lệ % để rớt ra 'Gói' trang bị. 1.0 = 100%")]
    [Range(0f, 1f)]
    public float equipmentPackageDropChance = 1.0f; // Mặc định là 100% rớt

    [Serializable]
    public class EquipmentDrop
    {
        [Tooltip("Kéo 'Sườn Mẫu' (file .asset) vào đây")]
        public EquipmentData itemData; // 👈 Sườn mẫu (ví dụ: WoodenSword_Data.asset)

        [Tooltip("Trọng số. Số càng cao, rớt càng nhiều. Ví dụ: Common=10, Rare=2")]
        public int weight; // 👈 Trọng số (thay cho % rắc rối)
    }

    [Tooltip("Danh sách các trang bị có thể rớt ra từ quái này")]
    public List<EquipmentDrop> equipmentDrops = new List<EquipmentDrop>();

    // Tổng trọng số (để tính tỉ lệ)
    private int totalWeight = 0;
    private bool didCalculateWeight = false;

    [Header("Drop Height")]
    [Tooltip("Vertical offset for dropped items.")]
    public float dropHeight = 1f;

    [Header("Large Drop Mode")]
    [Tooltip("If true, splits coin drops into multiple waves.")]
    public bool largeDrop = false;
    [Tooltip("Number of coin waves when in largeDrop mode.")]
    public int coinWaveCount = 3;
    [Tooltip("Delay (seconds) between each coin wave when in largeDrop mode.")]
    public float waveInterval = 0.5f;

    /// <summary>
    /// Phát sinh loot (xu, thuốc, orb) tại vị trí chest cộng thêm độ cao dropHeight
    /// </summary>
    /// 
    /// <summary>
    /// Tính tổng trọng số một lần duy nhất
    /// </summary>
    void CalculateTotalWeight()
    {
        if (didCalculateWeight) return;
        totalWeight = 0;
        foreach (EquipmentDrop drop in equipmentDrops)
        {
            totalWeight += drop.weight;
        }
        didCalculateWeight = true;
    }

    public void DropLoot()
    {
        Vector3 spawnPosition = transform.position + Vector3.up * dropHeight;

        // Tính lượng xu ngẫu nhiên
        int coinAmount = 0;
        if (coinPrefab != null)
            coinAmount = UnityEngine.Random.Range(minCoinAmount, maxCoinAmount + 1);

        // Spawn coins
        if (!largeDrop)
        {
            SpawnCoins(coinAmount, spawnPosition);
        }
        else
        {
            StartCoroutine(SpawnCoinWaves(coinAmount, spawnPosition));
        }

        // Rơi thuốc hồi phục theo tỷ lệ
        if (healthPotionPrefab != null && UnityEngine.Random.value <= healthPotionDropChance)
        {
            Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);
        }

        // Rơi orb tăng cấp sức mạnh với xác suất cho mỗi orb
        if (orbDrops != null && orbDrops.Count > 0)
        {
            foreach (OrbDrop orbDrop in orbDrops)
            {
                if (orbDrop.prefab != null && UnityEngine.Random.value <= orbDrop.dropChance)
                {
                    Instantiate(orbDrop.prefab, spawnPosition, Quaternion.identity);
                }
            }
        }

        // 1. Kiểm tra xem có gì để rớt không
        if (collectablePrefab != null && equipmentDrops != null && equipmentDrops.Count > 0 && UnityEngine.Random.value <= equipmentPackageDropChance)
        {
            CalculateTotalWeight(); // Tính tổng trọng số (nếu chưa)
            if (totalWeight == 0) return;

            // 2. Quay "xổ số" (giống ý tưởng 'common 2/3' của bạn)
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (EquipmentDrop equipDrop in equipmentDrops)
            {
                currentWeight += equipDrop.weight;
                if (roll < currentWeight)
                {
                    // 3. TRÚNG RỒI! (Rớt ra item này)

                    // 3a. Tạo ra "Gói"
                    GameObject dropObject = Instantiate(collectablePrefab, spawnPosition, Quaternion.identity);

                    // 3b. "Nhét" Sườn Mẫu (Data) vào "Gói"
                    EquipmentCollectable collectable = dropObject.GetComponent<EquipmentCollectable>();
                    if (collectable != null)
                    {
                        collectable.Initialize(equipDrop.itemData);
                    }

                    // Chỉ rớt 1 trang bị mỗi lần
                    break;
                }
            }
        }

    }

    /// <summary>
    /// Spawn số lượng coin đồng thời
    /// </summary>
    private void SpawnCoins(int amount, Vector3 position)
    {
        if (coinPrefab == null || amount <= 0) return;
        for (int i = 0; i < amount; i++)
        {
            Instantiate(coinPrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Coroutine spawn coins theo waves với delay waveInterval
    /// </summary>
    private IEnumerator SpawnCoinWaves(int totalAmount, Vector3 position)
    {
        if (coinPrefab == null || coinWaveCount <= 0)
        {
            SpawnCoins(totalAmount, position);
            yield break;
        }
        List<int> splits = SplitAmount(totalAmount, coinWaveCount);
        foreach (int waveCount in splits)
        {
            SpawnCoins(waveCount, position);
            yield return new WaitForSeconds(waveInterval);
        }
    }

    /// <summary>
    /// Chia tổng amount thành count phần gần đều nhau (có thể chênh lệch 1)
    /// </summary>
    private List<int> SplitAmount(int amount, int count)
    {
        List<int> result = new List<int>();
        if (count <= 0 || amount <= 0)
        {
            result.Add(amount);
            return result;
        }
        int baseAmount = amount / count;
        int remainder = amount % count;
        for (int i = 0; i < count; i++)
        {
            int part = baseAmount + (i < remainder ? 1 : 0);
            result.Add(part);
        }
        return result;
    }
}
