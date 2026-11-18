using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Makes the walkable path readable: walkable cells get a dirt tile instead of grass,
// and every edge between dirt and a blocked cell gets a leafy grass fringe so the
// path has soft, natural borders. Called by GridManagerScript after the level is built.
public class PathDecorator : MonoBehaviour
{
    // Either 16 tiles of a seamless 4x4 texture (Tools/generate_path_tiles.py), or any number of variants picked per cell.
    [SerializeField] private TileBase[] pathTiles;
    [SerializeField] private Sprite edgeSprite;      // grass fringe for a top edge; pivot on the border line
    [SerializeField] private Sprite cornerSprite;    // grass blob for an inner corner; pivot on the corner point (bottom-right)
    // Draw order: GameBackground -2, tilemap -1, this 0, stones/flies/pond 1, trees 2, frog 3.
    [SerializeField] private int sortingOrder = 0;
    [SerializeField] private float edgeInset = 0.06f; // how far the border sits inside the dirt cell
    [SerializeField] private Sprite[] leafSprites;     // loose leaves/tufts scattered on the path (Tools/generate_leaves.py)
    [SerializeField] private float leavesPerCell = 0.9f;
    [SerializeField] private Vector2 leafScale = new Vector2(0.8f, 1.2f);

    private static readonly Vector2Int[] sides = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
    private static readonly float[] sideAngles = { 0f, 90f, 180f, 270f };
    private static readonly Vector2Int[] diagonals = { new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };
    private static readonly float[] diagonalAngles = { 0f, 90f, 180f, 270f };

    private bool[,] walkable;

    // rows[y][x] are the level codes with y = 0 at the bottom (as GridManagerScript builds them).
    public void Decorate(Tilemap tilemap, List<string[]> rows)
    {
        int height = rows.Count;
        int width = 0;
        foreach (var row in rows) width = Mathf.Max(width, row.Length);

        walkable = new bool[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < rows[y].Length; x++)
            {
                walkable[x, y] = IsWalkable(rows[y][x].Trim());
            }
        }

        var decor = new GameObject("PathDecor").transform;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!walkable[x, y]) continue;

                var cell = new Vector3Int(x, y, 0);
                if (pathTiles != null && pathTiles.Length == 16)
                {
                    // 16 tiles = one seamless 4x4 texture (row 0 at the top), repeated over the board.
                    int row = 3 - (y & 3), col = x & 3;
                    tilemap.SetTile(cell, pathTiles[row * 4 + col]);
                }
                else if (pathTiles != null && pathTiles.Length > 0)
                {
                    tilemap.SetTile(cell, pathTiles[Hash(x, y) % pathTiles.Length]);
                }

                Vector3 center = tilemap.GetCellCenterWorld(cell);
                ScatterLeaves(decor, center, x, y);
                for (int i = 0; i < sides.Length; i++)
                {
                    var side = sides[i];
                    if (Walkable(x + side.x, y + side.y)) continue;
                    Vector3 position = center + new Vector3(side.x, side.y, 0) * (0.5f - edgeInset);
                    Place(decor, edgeSprite, position, sideAngles[i], Hash(x * 7 + i, y) % 2 == 0);
                }

                for (int i = 0; i < diagonals.Length; i++)
                {
                    var d = diagonals[i];
                    if (Walkable(x + d.x, y) && Walkable(x, y + d.y) && !Walkable(x + d.x, y + d.y))
                    {
                        Vector3 position = center + new Vector3(d.x, d.y, 0) * 0.5f;
                        Place(decor, cornerSprite, position, diagonalAngles[i], false);
                    }
                }
            }
        }
    }

    // Trees block the path; "FF" is a decorative fly placed off the path.
    private static bool IsWalkable(string code)
    {
        return code.Length > 0 && code[0] != 'T' && code != "FF";
    }

    private bool Walkable(int x, int y)
    {
        return x >= 0 && y >= 0 && x < walkable.GetLength(0) && y < walkable.GetLength(1) && walkable[x, y];
    }

    // Same level always gets the same leaves (seeded by the cell position).
    private void ScatterLeaves(Transform parent, Vector3 center, int x, int y)
    {
        if (leafSprites == null || leafSprites.Length == 0) return;
        var rng = new System.Random(Hash(x * 31 + 5, y * 17 + 3));
        int count = (int)leavesPerCell + (rng.NextDouble() < leavesPerCell % 1f ? 1 : 0);
        for (int i = 0; i < count; i++)
        {
            var sprite = leafSprites[rng.Next(leafSprites.Length)];
            var offset = new Vector3((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f, 0) * 0.8f;
            Place(parent, sprite, center + offset, (float)rng.NextDouble() * 360f, rng.Next(2) == 0);
            float s = Mathf.Lerp(leafScale.x, leafScale.y, (float)rng.NextDouble());
            parent.GetChild(parent.childCount - 1).localScale = new Vector3(s, s, 1);
        }
    }

    private void Place(Transform parent, Sprite sprite, Vector3 position, float angle, bool flip)
    {
        if (sprite == null) return;
        var go = new GameObject(sprite.name);
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.flipX = flip;
        renderer.sortingOrder = sortingOrder;
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 73856093 ^ y * 19349663;
            return (h ^ (h >> 13)) & 0x7fffffff;
        }
    }
}
