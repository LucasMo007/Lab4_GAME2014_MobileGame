using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 编辑器小工具：根据下面的文字地图自动画 Tilemap，并加好碰撞。
/// 用法：挂在 Ground（Tilemap）上 → 把两个 tile 拖进字段 → 右键组件标题 → Build Level。
/// 画完可以把这个组件删掉，画好的 tile 会保留。
///
/// 地图符号：
///   G = 顶部有草的地面
///   D = 泥土（地面下面 / 墙）
///   . = 空
/// 最下面一行在 Y = bottomY，最左边一列在 X = leftX。
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class LevelBuilder : MonoBehaviour
{
    public TileBase grassTile;   // 顶部有草的 tile
    public TileBase dirtTile;    // 纯泥土的 tile

    public int leftX = -12;
    public int bottomY = -6;

    [TextArea(12, 30)]
    public string map =
"D.....................................................................................D\n" +
"D.....................................................................................D\n" +
"D.....................................................................................D\n" +
"D.......................................................................GGGG..........D\n" +
"D.....................................................................................D\n" +
"D...................GGGGG...........................GGGG...........GGG................D\n" +
"D.....................................................................................D\n" +
"D..............GGGG..........GGG................GGG...........GGG.....................D\n" +
"D.....................................................................................D\n" +
"GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG...GGGGGGGGGGGGGGGGGGGGGGGG...GGGGGGGGGGGGGGGGGGGGG\n" +
"DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD...DDDDDDDDDDDDDDDDDDDDDDDD...DDDDDDDDDDDDDDDDDDDDD\n" +
"DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD...DDDDDDDDDDDDDDDDDDDDDDDD...DDDDDDDDDDDDDDDDDDDDD";

    [ContextMenu("Build Level")]
    void BuildLevel()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (grassTile == null || dirtTile == null)
        {
            Debug.LogError("LevelBuilder: 先把 Grass Tile 和 Dirt Tile 拖进来。");
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCompleteObjectUndo(tilemap, "Build Level");
#endif
        tilemap.ClearAllTiles();

        string[] rows = map.Replace("\r", "").Split('\n');
        int height = rows.Length;

        for (int r = 0; r < height; r++)
        {
            int y = bottomY + (height - 1 - r);   // 第一行在最上面
            string row = rows[r];

            for (int c = 0; c < row.Length; c++)
            {
                TileBase tile = null;
                switch (row[c])
                {
                    case 'G': tile = grassTile; break;
                    case 'D': tile = dirtTile; break;
                }

                if (tile != null)
                {
                    tilemap.SetTile(new Vector3Int(leftX + c, y, 0), tile);
                }
            }
        }

        AddColliders();
        Debug.Log("LevelBuilder: 关卡画好了。");
    }

    [ContextMenu("Clear Level")]
    void ClearLevel()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCompleteObjectUndo(tilemap, "Clear Level");
#endif
        tilemap.ClearAllTiles();
    }

    void AddColliders()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            gameObject.layer = groundLayer;
        }
        else
        {
            Debug.LogWarning("LevelBuilder: 没找到 Ground 层，记得在 Tags and Layers 里加，并把 Ground 物体的 Layer 设成 Ground。");
        }

        TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null) tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        if (GetComponent<CompositeCollider2D>() == null) gameObject.AddComponent<CompositeCollider2D>();

        tilemapCollider.usedByComposite = true;
    }
}
