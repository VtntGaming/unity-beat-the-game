using UnityEngine;

[System.Serializable]
public class Item
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public string Name = "Item";
    [SerializeField] public string Type = "";
    [SerializeField] public Rarity ItemRarity = new Rarity("Common", 1, new Color(0, 255, 0));
    [SerializeField] public readonly int StackCount = 1;
    [SerializeField] public int CurrentCount = 1;
    [SerializeField] public Sprite ItemIcon;
    public Item(string Name, string Type, int StackCount) : this(Name, Type, StackCount, new Rarity("Common", 1, new Color(175, 175, 175)), null) { }
    public Item(string Name, string Type, int StackCount, Rarity Rarity, Sprite ItemIcon)
    {
        // Các tên và type sẽ được T lập trình để đặt tên mặc định tuỳ thuộc vào các class con
        this.ItemRarity = Rarity;
        this.Type = Type;
        this.Name = Name;
        this.StackCount = StackCount;
        this.ItemIcon = ItemIcon;
    }
     
}