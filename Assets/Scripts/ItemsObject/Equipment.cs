using UnityEngine;

[System.Serializable]
public class Equipment : Item
{
    public string EquipmentType = "None";
    public Equipment() : this("Equipment") { }
    public Equipment(string Name):this(Name, "None") {}
    public Equipment(string Name, string Type):base(Name, "Equipment" , 1) // Là trang bị chỉ có 1 nên stackcount = 1
    {
        EquipmentType = Type;
    }
}
