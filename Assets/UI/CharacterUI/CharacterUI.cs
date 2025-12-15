using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Xml;
public class CharacterUI : MonoBehaviour
{
    private Entity playerHealth;
    private PlayerController playerController;
    private PlayerStats playerStats;

    // Transforms
    private Transform HealthPointFrame;
    private Transform ManaPointFrame;
    private Transform ArmorFrame;
    private Transform CoinFrame;

    // HP UI
    private RectTransform HPBase;
    private RectTransform HPLeft;
    private float BaseHPFrameWidth;
    private float BaseHPFrameHeight;
    private TMP_Text HPText;

    // Mana UI
    private RectTransform ManaBase;
    private RectTransform ManaLeft;
    private float BaseMPFrameWidth;
    private float BaseMPFrameHeight;
    private TMP_Text ManaText;

    // Armor UI
    private TMP_Text ArmorText;

    // Armor UI
    private TMP_Text CoinText;

    // TargetStats
    float targetHPPercentage = 0;

    // CurrentStats
    float currentHPPercentage = 0;


    void Start()
    {
        // Tìm các thành phần UI
        HealthPointFrame = transform.Find("HPBar").Find("Base");
        ManaPointFrame = transform.Find("MPBar").Find("Base");
        ArmorFrame = transform.Find("Armor").Find("Base");
        CoinFrame = transform.Find("Coin").Find("Base");

        // Lấy thông tin từ người chơi
        BindPlayerStats();

        // HP
        HPBase = HealthPointFrame.Find("Overlay").GetComponent<RectTransform>();
        HPLeft = HealthPointFrame.Find("ValueBar").GetComponent<RectTransform>();
        HPText = HealthPointFrame.Find("Display").GetComponent<TMP_Text>();
        BaseHPFrameWidth = HPBase.rect.width;
        BaseHPFrameHeight = HPBase.rect.height;

        // Mana
        ManaBase = ManaPointFrame.Find("Overlay").GetComponent<RectTransform>();
        ManaLeft = ManaPointFrame.Find("ValueBar").GetComponent<RectTransform>();
        ManaText = ManaPointFrame.Find("Display").GetComponent<TMP_Text>();
        BaseMPFrameWidth = ManaBase.rect.width;
        BaseMPFrameHeight = ManaBase.rect.height;

        // Armor
        ArmorText = ArmorFrame.Find("Display").GetComponent<TMP_Text>();

        // Coin
        CoinText = CoinFrame.Find("Display").GetComponent<TMP_Text>();
    }

    bool BindPlayerStats()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return false;

        playerHealth = player.GetComponent<Entity>();
        playerController = player.GetComponent<PlayerController>();
        playerStats = player.GetComponent<PlayerStats>();

        return true;
    }

    void Update()
    {
        if (playerHealth == null || playerController == null || playerStats == null)
        {
            if (!BindPlayerStats()) return;
        }

        // Lấy chỉ số từ PlayerHealth
        float MaxHP = playerHealth.maxHealth;
        float HP = playerHealth.currentHealth;
        float MaxMP = playerHealth.maxMana;
        float MP = playerHealth.currentMana;
        float Armour = (playerStats != null) ? playerStats.FinalArmour : 0;
        int Coin = playerController.getCoin();

        // Cập nhật thanh HP
        HPText.text = string.Format("{0:F0}/{1:F0}", HP, MaxHP);
        if (targetHPPercentage != HP / MaxHP)
        {
            targetHPPercentage = HP / MaxHP;
        }

        // Cập nhật thanh MP
        ManaLeft.sizeDelta = new Vector2(BaseMPFrameWidth * (MP / MaxMP), BaseMPFrameHeight);
        ManaText.text = string.Format("{0:F0}/{1:F0}", MP, MaxMP);

        // Cập nhật giáp
        ArmorText.text = string.Format("{0:F0}", Armour);

        // Cập nhật coin
        CoinText.text = string.Format("{0:F0}", Coin);

        // Animate
        float dt = Time.deltaTime;
        if (currentHPPercentage != targetHPPercentage)
        {
            currentHPPercentage = Mathf.Lerp(currentHPPercentage, targetHPPercentage, 0.1f);
        }

        // Display
        HPLeft.sizeDelta = new Vector2(BaseHPFrameWidth * currentHPPercentage, BaseHPFrameHeight);

    }
}
