using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;


public class Inventory : MonoBehaviour
{
    // Chứa danh sách item
    [SerializeField] public List<ItemData> InventoryItem = new List<ItemData>();
    [SerializeField] public int MaximumInventory = 32;

    // Các equipment cho người chơi
    [SerializeField] public WeaponEquipment Weapon;
    [SerializeField] public HeadEquipment Head;
    [SerializeField] public BodyEquipment Body;
    public event Action InventoryChangedEvent;
    private bool itemChagned = false;
    private ItemData findForItemAvaiable(string name) {
        // Tìm xem còn item nào còn slot để add vào không
        foreach (ItemData item in InventoryItem) {
            // Phân biệt name với name
            if (item is StackableItemData)
            {
                StackableItemData stackableItem = (StackableItemData)item;
                if (item.itemName == name && stackableItem.CurrentCount < stackableItem.StackCount) return stackableItem;
            }
        }

        return null;
    }
    public string AddItem(ItemData item)
    {
        //Debug.Log("Adding item - Name: " + item.Name);
        // Thêm số lượng vào item (nếu stack được)
        if (item is StackableItemData)
        {
            StackableItemData avaiableItem = (StackableItemData) findForItemAvaiable(item.itemName);
            if (avaiableItem != null) {
                avaiableItem.CurrentCount++;
                itemChagned = true;
                return "Success";
            }
        }
        if (InventoryItem.Count >= MaximumInventory) return "InventoryFull";
        else 
        {
            InventoryItem.Add(item);
            itemChagned = true;
            return "Success";
        }
    }

    void Start()
    {
        //Weapon = new Sword("test 123", 22f);
        //// Demo test
        //string Result = this.AddItem(new Sword("Test kiếm", 123f));
        //this.AddItem(new Staff("Test trượng", 234f));

    }

    // Update is called once per frame
    private int prevInventoryCount = 0;
    void Update()
    {
        if (InventoryItem.Count != prevInventoryCount)
            itemChagned = true;
        prevInventoryCount = InventoryItem.Count;
        if (itemChagned) {
            itemChagned = false;
            InventoryChangedEvent?.Invoke();
        }
    }
}
