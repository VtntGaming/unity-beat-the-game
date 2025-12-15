using UnityEngine;

[System.Serializable]
public class Potion:Item
{
    public Potion() : this("Potion") { }
    public Potion(string Name) : base(Name, "Potion" , 10) { }
}
