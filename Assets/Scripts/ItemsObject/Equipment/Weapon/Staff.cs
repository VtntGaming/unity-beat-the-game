using UnityEngine;

[System.Serializable]
public class Staff:WeaponEquipment
{
    public float MagicDamage = 1f;
    public Staff() : this("Staff") { }
    public Staff(string Name) : this(Name, 1f) { }
    public Staff(string Name, float MagicDamage) : base(Name) {
        this.MagicDamage = MagicDamage;
    }
}
