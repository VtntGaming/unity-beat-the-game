using NUnit.Framework.Interfaces;
using System.Collections;
using System.Threading;
using UnityEngine;

public class ShopkeeperController : MonoBehaviour
{
    // ==============================
    // 📦 FIELD KHAI BÁO
    // ==============================

    [Header("Setup")]
    public Transform cloudContainer;
    public Transform dropPoint;
    public float dropForce = 3f;

    [Header("Player Detection")]
    public float interactDistance = 3f;
    public Transform player;
    public string playerTag = "Player";

    [Header("Cloud Slots (3 slots bán item)")]
    public GameObject[] cloudSlots; // CloudSlot1, 2, 3
    public bool hideCloudsByDefault = true;

    [Header("Shop display properties")]
    private bool isShopActive = false;
    private const float fade_time = 0.5f;   // seconds
    private float shopOpacity = 0.0f;

    [Header("Shopkeeper sprite")]
    [SerializeField] private GameObject shopKeeperSprite;
    [SerializeField] private Animator shopKeeperAnimator;

    [Header("Visual Colors")]
    public Color highlightColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float colorFlashTime = 0.25f;

    [Header("Reroll Slot")]
    public Transform rerollSlot;           // CloudSlot4
    public GameObject rerollPrefab;        // Prefab icon reroll
    [Range(0, 100)] public int baseRerollCost = 2;
    public float rerollMultiplier = 1.5f;  // Hệ số tăng giá
    private int rerollCount = 0;
    private GameObject rerollDisplay;

    // ------------------------------
    private SellItem sellManager;
    private PlayerController playerController;
    private bool playerInRange = false;

    // ==============================
    // 🧩 STARTUP (INIT)
    // ==============================

    private void Start()
    {
        // Lấy SellItem logic từ cloudContainer
        if (cloudContainer != null)
            sellManager = cloudContainer.GetComponent<SellItem>();

        // Tìm player nếu chưa gán
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
                player = p.transform;
        }

        // Lấy PlayerController
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        // Ẩn cloud nếu cần
        if (hideCloudsByDefault && cloudSlots != null)
            ShowClouds(false);

        // Shopkeeper luôn nằm sau player
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = -1;
    }

    // ==============================
    // 🧭 UPDATE LOOP
    // ==============================

    private void Update()
    {
        if (player == null) return;

        // Kiểm tra khoảng cách
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactDistance && !playerInRange)
        {
            playerInRange = true;
            ShowClouds(true);
        }
        else if (distance > interactDistance && playerInRange)
        {
            playerInRange = false;
            ShowClouds(false);
        }

        updateOpacity();

        if (!playerInRange) return;

        // Bấm phím mua item
        if (Input.GetKeyDown(KeyCode.Alpha1)) HandleSlotInteraction(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) HandleSlotInteraction(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) HandleSlotInteraction(2);

        // Bấm R để reroll thủ công
        if (Input.GetKeyDown(KeyCode.R)) TryReroll();
    }

    // ==============================
    // 💬 SLOT INTERACTION
    // ==============================

    public void HandleSlotInteraction(int index)
    {
        if (index < 0 || cloudSlots == null || index >= cloudSlots.Length || cloudSlots[index] == null)
            return;

        StartCoroutine(FlashColor(cloudSlots[index], highlightColor));

        bool success = TryBuy(index);
        StartCoroutine(FlashColor(cloudSlots[index], success ? successColor : failColor));

        // Nếu tất cả item đã mua hết => auto reroll free
        if (sellManager != null && sellManager.AllItemsSold())
        {
            Debug.Log("✨ Tất cả vật phẩm đã bán — tự động reroll miễn phí!");
            TryReroll(autoFree: true);
        }
    }

    private IEnumerator FlashColor(GameObject target, Color color)
    {
        if (target == null) yield break;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        sr.color = color;
        yield return new WaitForSeconds(colorFlashTime);
        sr.color = original;
    }

    // ==============================
    // ☁ CLOUD DISPLAY LOGIC
    // ==============================

    float easeInSine(float x) {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }
    private void updateOpacity() {
        float dt = Time.deltaTime;
        float _change = dt / fade_time * ((isShopActive)?1:-1);
        shopOpacity = Mathf.Clamp01(shopOpacity + _change);
        float progress = easeInSine(shopOpacity);
        float size = Mathf.Lerp(0.5f, 1f, progress);
        bool fullyActive = shopOpacity > 0;
        foreach (var cloud in cloudSlots)
            if (cloud != null)
            {
                cloud.SetActive(fullyActive);
                cloud.transform.localScale = new Vector3(size, size, 1);
                cloud.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, shopOpacity);
            }
        if (rerollSlot != null)
        {
            rerollSlot.gameObject.SetActive(fullyActive);
            rerollSlot.transform.localScale = new Vector3(size, size, 1);
            rerollSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, shopOpacity);
        }
        sellManager.SetOpacity(shopOpacity);
    }

    private void ShowClouds(bool show)
    {
        if (cloudSlots == null) return;

        isShopActive = show;

        //foreach (var cloud in cloudSlots)
        //    if (cloud != null)
        //        cloud.SetActive(show);

        //if (rerollSlot != null)
        //    rerollSlot.gameObject.SetActive(show);

        Debug.Log(shopKeeperAnimator);
        shopKeeperAnimator.SetBool("IsNear", show);

        if (show)
        {
            if (sellManager != null)
                sellManager.SpawnItems();

            SpawnRerollIcon();
        }
        else
        {
            if (rerollDisplay != null)
                Destroy(rerollDisplay);
        }
    }

    // ==============================
    // 💰 BUY ITEM LOGIC
    // ==============================

    private IEnumerator waitDropItem(ShopItemData Item)
    {
        yield return new WaitForSeconds(0.75f);
        // Thả item ra
        DropItem(Item.prefab);
    }

    private bool TryBuy(int index)
    {
        if (sellManager == null) return false;

        var itemData = sellManager.GetItemByIndex(index);
        if (itemData == null)
        {
            Debug.Log("❌ Slot trống hoặc chưa có item.");
            return false;
        }

        int cost = itemData.price;
        int playerCoins = playerController != null ? playerController.getCoin() : 0;

        if (playerCoins < cost)
        {
            Debug.Log($"❌ Không đủ tiền để mua {itemData.itemName}! Cần {cost}, có {playerCoins}.");
            return false;
        }

        // Trừ tiền
        playerController.AddCoin(-cost);
        Debug.Log($"✅ Mua {itemData.itemName} thành công! Còn {playerController.getCoin()} coin.");

        StartCoroutine(waitDropItem(itemData));

        shopKeeperAnimator.SetTrigger("Purchase");
        waitDropItem(itemData);

        // Xóa item khỏi slot
        sellManager.ClearItem(index);

        return true;
    }

    private void DropItem(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        Transform dropTransform = dropPoint != null ? dropPoint : transform;
        GameObject dropped = Instantiate(itemPrefab, dropTransform.position, Quaternion.identity);

        Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>();
        if (rb == null) rb = dropped.AddComponent<Rigidbody2D>();

        Vector2 randomForce = new Vector2(Random.Range(-1f, 1f), 1f) * dropForce;
        rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    // ==============================
    // 🔁 REROLL LOGIC
    // ==============================

    private void SpawnRerollIcon()
    {
        if (rerollSlot == null || rerollPrefab == null) return;

        rerollDisplay = Instantiate(rerollPrefab, rerollSlot, false);
        rerollDisplay.transform.localPosition = Vector3.zero;
        rerollDisplay.transform.localScale = rerollPrefab.transform.localScale;

        Transform priceT = rerollDisplay.transform.Find("PriceText");
        if (priceT != null)
        {
            var tmp = priceT.GetComponent<TMPro.TextMeshPro>();
            if (tmp != null)
                tmp.text = $"{GetCurrentRerollCost()} G";
        }

    }

    public void TryReroll(bool autoFree = false)
    {
        if (sellManager == null || playerController == null) return;

        int cost = autoFree ? 0 : GetCurrentRerollCost();

        if (!autoFree && playerController.getCoin() < cost)
        {
            Debug.Log("❌ Không đủ tiền reroll.");
            return;
        }

        if (!autoFree)
        {
            playerController.AddCoin(-cost);
            rerollCount++;
        }

        sellManager.ClearAll();
        sellManager.SpawnItems();

        UpdateRerollDisplay();
        Debug.Log($"🔄 Đã reroll! Lần reroll thứ {rerollCount}, giá tiếp theo: {GetCurrentRerollCost()}G");
    }

    private void UpdateRerollDisplay()
    {
        if (rerollDisplay == null) return;

        Transform priceT = rerollDisplay.transform.Find("PriceText");
        if (priceT != null)
        {
            var tmp = priceT.GetComponent<TMPro.TextMeshPro>();
            if (tmp != null)
                tmp.text = $"{GetCurrentRerollCost()} G";
        }
    }

    private int GetCurrentRerollCost()
    {
        return Mathf.RoundToInt(baseRerollCost * Mathf.Pow(rerollMultiplier, rerollCount));
    }
}
