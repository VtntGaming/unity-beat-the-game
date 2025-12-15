using UnityEngine;

[CreateAssetMenu(fileName = "New stackable item", menuName = "Item/Stackable item")]
public class StackableItemData : ItemData
{
    [Header("Setup cho nhiều item cùng loại (stackable)")]
    public int StackCount;
    [HideInInspector] public int CurrentCount = 0;
}
