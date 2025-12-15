using UnityEngine;

[System.Serializable]
public class WeaponEquipment : Equipment
{
    public string WeaponType = "None";
    public WeaponEquipment() : this("Weapon") { }
    public WeaponEquipment(string Name) : this(Name, "None") { }
    public WeaponEquipment(string Name, string Type) : base(Name, "Weapon") {
        WeaponType = Type;
    }
}
