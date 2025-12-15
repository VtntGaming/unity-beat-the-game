using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Breakable Tile")]
public class BreakableTile : Tile
{
    public int maxHealth = 3;            // Số hit để phá
    public Sprite crackedSprite;         // Sprite khi nứt
    public bool spawnShards = true;      // Có spawn mảnh không?
    public GameObject[] shardPrefabs;    // Mảnh vỡ
    public int shardCount = 5;           // Bao nhiêu mảnh
}
