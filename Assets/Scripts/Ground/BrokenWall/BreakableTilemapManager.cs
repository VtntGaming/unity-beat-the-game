using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BreakableTilemapManager : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemap;

    [Header("Tile Settings")]
    public float defaultTileHealth = 20f; // HP mặc định nếu tile không phải BreakableTile
    public GameObject shardPrefab;        // Prefab mảnh vỡ
    public int shardCount = 4;

    // Lưu HP từng ô tile
    private Dictionary<Vector3Int, float> tileHealth = new Dictionary<Vector3Int, float>();

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        Debug.Log("<color=cyan>[BreakableTilemapManager]</color> Tilemap Manager Initialized!");
    }

    // Gọi khi player tấn công
    public void TakeDamageAtPosition(Vector3 worldPos, float dmg)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);

        Debug.Log($"<color=yellow>[BreakableTilemapManager]</color> Hit at worldPos {worldPos} | cellPos {cellPos}");

        TileBase tile = tilemap.GetTile(cellPos);

        if (tile == null)
        {
            Debug.Log("<color=grey>[BreakableTilemapManager]</color> No tile at this position.");
            return;
        }

        // Khởi tạo HP nếu tile chưa có
        if (!tileHealth.ContainsKey(cellPos))
        {
            tileHealth[cellPos] = defaultTileHealth;
            Debug.Log($"<color=green>[BreakableTilemapManager]</color> Init HP for tile {cellPos}: {defaultTileHealth}");
        }

        // Trừ HP
        tileHealth[cellPos] -= dmg;
        Debug.Log($"<color=orange>[BreakableTilemapManager]</color> Tile {cellPos} took {dmg} dmg → HP = {tileHealth[cellPos]}");

        // Nếu HP <= 0 → phá
        if (tileHealth[cellPos] <= 0)
        {
            BreakTile(cellPos);
        }
    }

    private void BreakTile(Vector3Int cellPos)
    {
        // Xóa tile
        tilemap.SetTile(cellPos, null);
        Debug.Log($"<color=red>[BreakableTilemapManager]</color> Tile {cellPos} DESTROYED!");

        // Spawn shard
        if (shardPrefab)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector3 spawnPos = tilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                GameObject shard = Instantiate(shardPrefab, spawnPos, Quaternion.identity);

                Debug.Log($"<color=magenta>[BreakableTilemapManager]</color> Spawn shard #{i + 1} at {spawnPos}");

                if (shard.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 force = Random.insideUnitCircle.normalized * Random.Range(1f, 3f);
                    rb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }

        // Xóa HP khỏi dictionary
        tileHealth.Remove(cellPos);
    }
}
