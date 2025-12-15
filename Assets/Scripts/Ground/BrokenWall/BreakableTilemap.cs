using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;                   // Tilemap cần phá
    public TileBase breakableTile;            // Tile có thể phá
    public GameObject breakableRockPrefab;    // Prefab rock rơi ra

    [Header("Rock Settings")]
    public int rockCount = 6;
    public float rockForce = 2f;
    public float rockTorque = 5f;
    public float rockLifetime = 1.5f;

    [Header("Debug")]
    public bool enableDebug = true;

    // Phá tile tại vị trí world
    public void BreakTileAt(Vector2 worldPos)
    {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        Tile tile = tilemap.GetTile<Tile>(cell);

        // Kiểm tra tile lân cận nếu tile không phải breakable
        if (tile == null || tile != breakableTile)
        {
            Vector3Int[] neighbors = {
                cell + Vector3Int.right,
                cell + Vector3Int.left,
                cell + Vector3Int.up,
                cell + Vector3Int.down
            };

            foreach (var c in neighbors)
            {
                tile = tilemap.GetTile<Tile>(c);
                if (tile == breakableTile)
                {
                    cell = c;
                    break;
                }
            }
        }

        if (tile == null || tile != breakableTile)
        {
            if (enableDebug) Debug.Log($"[BreakableTilemap] Không có tile để phá tại {cell}");
            return;
        }

        if (enableDebug) Debug.Log($"[BreakableTilemap] Phá tile tại {cell} (WorldPos={worldPos})");

        Vector3 spawnPos = tilemap.GetCellCenterWorld(cell);
        tilemap.SetTile(cell, null);
        tilemap.RefreshAllTiles();

        // Spawn rock
        for (int i = 0; i < rockCount; i++)
        {
            GameObject rock = Instantiate(breakableRockPrefab, spawnPos, Quaternion.identity);
            SpriteRenderer sr = rock.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = tile.sprite;

            Rigidbody2D rb = rock.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 2f));
                rb.AddForce(randomDir * rockForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-rockTorque, rockTorque), ForceMode2D.Impulse);
            }

            Destroy(rock, rockLifetime);
        }

        if (enableDebug) Debug.Log($"[BreakableTilemap] Spawn {rockCount} rock từ tile {cell}");
    }

    // Phá tilemap từ box (hỗ trợ melee attack)
    public void BreakTileFromBox(Vector2 center, Vector2 size)
    {
        Vector3Int min = tilemap.WorldToCell(center - size * 0.5f);
        Vector3Int max = tilemap.WorldToCell(center + size * 0.5f);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Tile tile = tilemap.GetTile<Tile>(cell);
                if (tile == breakableTile)
                {
                    BreakTileAt(tilemap.GetCellCenterWorld(cell));
                }
            }
        }
    }

    // Kiểm tra có tile breakable trong box không
    public bool HasTileInBox(Vector2 boxCenter, Vector2 boxSize)
    {
        Vector3Int minCell = tilemap.WorldToCell(boxCenter - boxSize * 0.5f);
        Vector3Int maxCell = tilemap.WorldToCell(boxCenter + boxSize * 0.5f);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile == breakableTile) return true;
            }
        }
        return false;
    }
}
