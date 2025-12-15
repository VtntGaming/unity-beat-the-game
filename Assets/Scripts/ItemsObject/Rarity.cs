using NUnit.Framework.Constraints;
using UnityEngine;

[System.Serializable]
public class Rarity
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public string RarityType = "Common"; // Starting rarity
    [SerializeField] public int RarityLevel = 1; // Rarity level
    [SerializeField] public Color RarityColor = new Color(175, 175, 175); // Rarity level


    public Rarity() : this("Common") { }
    public Rarity(string Name) : this(Name, 1) { }
    public Rarity(string Name, int Level) : this(Name, Level, new Color(175, 175, 175)) { }
    public Rarity(string Name, int Level, Color color)
    {
        RarityType = Name;
        RarityLevel = Level;
        RarityColor = color;
    }

}
