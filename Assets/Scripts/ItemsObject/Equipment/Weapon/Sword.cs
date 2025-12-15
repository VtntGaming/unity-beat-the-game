using UnityEngine;

[System.Serializable]
public class Sword:WeaponEquipment
{
    public float HitDamage = 1f;
    public Sword() : this("Sword") { }
    public Sword(string Name) : this(Name, 1f) { }
    public Sword(string Name, float hitDamage) : base(Name, "Sword")
    {
        HitDamage = hitDamage;
    }
}
