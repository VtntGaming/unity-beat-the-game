using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject fileObj;

    void updateForObject(Transform container, string infoDisplay, EquipmentItem item)
    {
        // Destroy old component
        if (container.Find("Item"))
            Destroy(container.Find("Item").gameObject);

        Transform infoFrame = container.parent.Find("Info");
        TMP_Text infoTxt = infoFrame.GetComponent<TMP_Text>();

        if (item != null && item.HasItem())
        {
            GameObject obj = Instantiate(fileObj, container);
            obj.name = "Item";
            Transform quantityTrans = obj.transform.Find("Quantity");
            quantityTrans.gameObject.SetActive(false);

            Image itemImage = obj.transform.Find("ItemImage")?.GetComponent<Image>();
            itemImage.sprite = item.template?.itemSprite;

            Transform rarityFrame = obj.transform.Find("Rarity");
            Sprite raritySprite = item.rolledRarity?.frameSprite;
            rarityFrame.gameObject.SetActive(raritySprite != null);

            if (raritySprite != null)
            {
                Image frameImage = rarityFrame.GetComponent<Image>();
                frameImage.sprite = raritySprite;
            }

            // Update item info
            infoFrame.gameObject.SetActive(true);
            float finalStat = item.GetFinalStat();
            string infoText = string.Format("+{0:F0} {1}", finalStat, infoDisplay);
            infoTxt.SetText(infoText);

            obj.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        }
        else
        {
            infoFrame.gameObject.SetActive(false);
            Debug.Log("Could not update, item not found");
        }
    }

    public void UpdateEquipped()
    {
        Debug.Log("Update signal recieved");
        Transform equipFrame = transform;
        Transform weaponObject = equipFrame.Find("Weapon").Find("Object");
        Transform armourObject = equipFrame.Find("Armour").Find("Object");
        PlayerStats stats = GameObject.Find("Player").GetComponent<PlayerStats>();
        if (stats == null)
            return;

        EquipmentItem sword = stats.equippedSword;
        EquipmentItem armour = stats.equippedArmor;

        Debug.Log("try sword...");
        updateForObject(weaponObject, "ATK", sword);
        Debug.Log("try armour...");
        updateForObject(armourObject, "DEF", armour);
        Debug.Log("Update complete");
    }

    private void Awake()
    {
        // Load file object
        fileObj = Resources.Load<GameObject>("UI/Inventory/ItemObject");

    }
    void Start()
    {
        UpdateEquipped();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
