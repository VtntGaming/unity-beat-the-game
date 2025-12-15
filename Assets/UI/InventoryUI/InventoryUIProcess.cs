using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIProcess : MonoBehaviour
{
    private GameObject fileObj;
    private Inventory playerInventory;
    void Start()
    {
        // Load file object
        fileObj = Resources.Load<GameObject>("UI/Inventory/ItemObject");
        // Load dữ liệu túi đồ
        
        playerInventory = GameObject.Find("Player").GetComponent<Inventory>();
        playerInventory.InventoryChangedEvent += UpdateInventory;
        UpdateInventory();
    }

    private int maxItemPerRow = 7;
    public void UpdateInventory()
    {
        // Xoá dữ liệu cũ
        foreach (Transform obj in transform)
        {
            Destroy(obj.gameObject);
        }
        // Cập nhật dữ liệu mới
        int i = 0;
        foreach (ItemData item in playerInventory.InventoryItem) {
            GameObject obj = Instantiate(fileObj);
            obj.transform.SetParent(transform);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1, 1, 1);
            int x = i%maxItemPerRow;
            int y = i/maxItemPerRow;
            i++;
            rect.anchoredPosition = new Vector2(x*100, y*100);

            Transform quantityTrans = obj.transform.Find("Quantity");
            TMP_Text quantity = quantityTrans.GetComponent<TMP_Text>();
            //Color ItemColor = item.ItemRarity.RarityColor;
            //Image OverlayImage = obj.transform.Find("Overlay").GetComponent<Image>();
            //OverlayImage.color = new Color(ItemColor.r, ItemColor.g, ItemColor.b, 0.5f);

            Image itemImage = obj.transform.Find("ItemImage").GetComponent<Image>();
            itemImage.sprite = item.itemSprite;

            if (item is StackableItemData)
            {
                StackableItemData stackItem = (StackableItemData)item;
                quantityTrans.gameObject.SetActive(true);
                quantity.text = stackItem.StackCount.ToString();
            }
            else
                quantityTrans.gameObject.SetActive(false);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
