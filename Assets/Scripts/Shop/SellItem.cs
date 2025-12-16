using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ShopItemData
{
    public string itemName;
    public GameObject prefab;   // Prefab item thật (để spawn khi mua)
    public int price = 10;      // Giá
    [HideInInspector] public Sprite icon; // Sprite hiển thị, lấy từ prefab nếu có
}

public class SellItem : MonoBehaviour
{
    [Header("Danh sách item có thể bán (kéo prefab item thật vào đây)")]
    public List<ShopItemData> possibleItems = new List<ShopItemData>();

    [Header("Các vị trí cloud slot (Cloud1, Cloud2, Cloud3...)")]
    public Transform[] cloudSlots;

    [Header("Mẫu hiển thị sprite item (ItemDisplaySprite prefab)")]
    public GameObject itemDisplayPrefab;

    private List<ShopItemData> currentItems = new List<ShopItemData>();
    private List<GameObject> itemDisplays = new List<GameObject>();
    private bool itemsSpawned = false;

    private void Start()
    {
        if (possibleItems == null || possibleItems.Count == 0)
            Debug.LogWarning("⚠️ SellItem: possibleItems trống — chưa gán item vào Inspector.");

        if (cloudSlots == null || cloudSlots.Length == 0)
            Debug.LogWarning("⚠️ SellItem: cloudSlots trống — cần 3 Transform của mây.");

        if (itemDisplayPrefab == null)
            Debug.LogWarning("⚠️ SellItem: itemDisplayPrefab chưa gán (ItemDisplaySprite).");
    }

    // Gọi từ ShopkeeperController.ShowClouds(true)
    public void SpawnItems()
    {
        if (itemsSpawned) return;

        if (possibleItems.Count == 0 || cloudSlots.Length == 0 || itemDisplayPrefab == null)
            return;

        itemsSpawned = true;
        currentItems.Clear();
        ClearDisplays();

        // Copy pool và shuffle
        List<ShopItemData> pool = new List<ShopItemData>(possibleItems);
        Shuffle(pool);

        int count = Mathf.Min(cloudSlots.Length, pool.Count);

        for (int i = 0; i < count; i++)
        {
            ShopItemData data = pool[i];
            currentItems.Add(data);

            // Nếu icon chưa có, tự lấy sprite từ prefab gốc
            if (data.icon == null && data.prefab != null)
            {
                SpriteRenderer sr = data.prefab.GetComponent<SpriteRenderer>();
                if (sr != null) data.icon = sr.sprite;
            }

            // Tạo hiển thị (sprite-based)
            Transform slot = cloudSlots[i];
            GameObject display = Instantiate(itemDisplayPrefab, slot, false);
            display.transform.localPosition = Vector3.zero;
            // ✅ KIỂM TRA TÊN ITEM ĐỂ SET CỨNG SCALE
            if (data.itemName == "Power Upgrade Orb")
            {
                display.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            }
            else if (data.itemName == "Reduce Dash Cooldown")
            {
                display.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            }
            else if (data.itemName == "Fire Orb")
            {
                display.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            }
            else if (data.itemName == "Double Jump Orb")
            {
                display.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            }
            else
            {
                display.transform.localScale = Vector3.one; // Scale mặc định là 1
            }
            display.name = $"CloudItem_{i + 1}";

            // Gán sprite icon
            SpriteRenderer srDisplay = display.GetComponent<SpriteRenderer>();
            if (srDisplay != null && data.icon != null)
                srDisplay.sprite = data.icon;

            // Gán text giá tiền
            Transform priceT = display.transform.Find("PriceText");
            if (priceT != null)
            {
                TextMeshPro tmp = priceT.GetComponent<TextMeshPro>();
                if (tmp != null)
                    tmp.text = $"{data.price} G";
            }

            // ✅ Gắn script click
            ShopItemClick clicker = display.GetComponent<ShopItemClick>();
            if (clicker == null)
                clicker = display.AddComponent<ShopItemClick>();

            clicker.slotIndex = i; // Gán index để ShopkeeperController biết slot nào

            itemDisplays.Add(display);
        }
    }

    public ShopItemData GetItemByIndex(int index)
    {
        if (index < 0 || index >= currentItems.Count) return null;
        return currentItems[index];
    }

    public void ClearItem(int index)
    {
        if (index < 0 || index >= currentItems.Count) return;

        currentItems[index] = null;

        if (index < itemDisplays.Count && itemDisplays[index] != null)
        {
            Destroy(itemDisplays[index]);
            itemDisplays[index] = null;
        }
    }

    public void ClearAll()
    {
        currentItems.Clear();
        ClearDisplays();
        itemsSpawned = false;
    }

    private void ClearDisplays()
    {
        for (int i = 0; i < itemDisplays.Count; i++)
        {
            if (itemDisplays[i] != null)
                Destroy(itemDisplays[i]);
        }
        itemDisplays.Clear();
    }

    public void SetOpacity(float opacity)
    {
        opacity = Mathf.Clamp01(opacity);
        foreach (var item in itemDisplays)
        {
            if (item)
                item.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, opacity);
        }
    }

    private void Shuffle(List<ShopItemData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    [Header("Reroll Settings")]
    public Transform rerollSlot;           // CloudSlot4
    public GameObject rerollDisplayPrefab; // Prefab icon reroll
    public int baseRerollPrice = 2;       // Giá reroll lần đầu
    public float priceMultiplier = 1.5f;   // Mức tăng giá mỗi lần reroll
    private int rerollCount = 0;

    private GameObject rerollDisplay;
    private TextMeshPro rerollText;

    public void SpawnRerollIcon()
    {
        if (rerollSlot == null || rerollDisplayPrefab == null) return;

        // Xoá cũ nếu có
        if (rerollDisplay != null)
            Destroy(rerollDisplay);

        rerollDisplay = Instantiate(rerollDisplayPrefab, rerollSlot);
        rerollDisplay.transform.localPosition = Vector3.zero;
        rerollDisplay.transform.localScale = Vector3.one;
        rerollDisplay.name = "RerollIcon";

        // Tìm text giá
        rerollText = rerollDisplay.transform.Find("PriceText")?.GetComponent<TextMeshPro>();
        UpdateRerollPriceText();

        // Gắn script click
        ShopItemClick clicker = rerollDisplay.GetComponent<ShopItemClick>();
        if (clicker == null) clicker = rerollDisplay.AddComponent<ShopItemClick>();
        clicker.slotIndex = -1; // -1 dùng riêng cho reroll
    }

    public void RerollItems()
    {
        Debug.Log("🔄 Đang reroll vật phẩm...");
        ClearAll();
        rerollCount++;
        SpawnItems();
        UpdateRerollPriceText();
    }

    public int GetCurrentRerollPrice()
    {
        // Nếu 3 slot trống (mua hết) → miễn phí
        bool allEmpty = true;
        foreach (var item in currentItems)
        {
            if (item != null)
            {
                allEmpty = false;
                break;
            }
        }

        if (allEmpty) return 0;

        return Mathf.RoundToInt(baseRerollPrice * Mathf.Pow(priceMultiplier, rerollCount));
    }

    private void UpdateRerollPriceText()
    {
        if (rerollText != null)
            rerollText.text = $"{GetCurrentRerollPrice()} G";
    }
    public bool AllItemsSold()
    {
        if (currentItems.Count == 0) return false;
        foreach (var item in currentItems)
            if (item != null)
                return false;
        return true;
    }

}
