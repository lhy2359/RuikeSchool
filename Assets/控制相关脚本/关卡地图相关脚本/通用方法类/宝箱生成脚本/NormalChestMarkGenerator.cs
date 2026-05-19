using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 普通宝箱生成器
/// </summary>
public class NormalChestMarkGenerator : BaseMarkPointGenerator
{
    public NormalChestMarkGenerator(Scene targetScene, string tilemapName, GameObject chestPrefab)
        : base(targetScene, tilemapName, chestPrefab) { }

    protected override Vector2Int GetOccupiedSize()
    {
        return Vector2Int.one;
    }

    protected override void GenerateObjects(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject obj = Object.Instantiate(_prefab, pos, Quaternion.identity, _markPointParent);
            obj.name = $"GoldenChest_{pos.x:F0}_{pos.y:F0}";
        }
    }

    protected override bool IsCellValid(Tilemap map, Vector3Int cell)
    {
        // 两个条件必须同时满足：瓦片存在，格子没有被全局占用池包含
        bool tileExists = map.GetTile(cell) != null;
        bool notOccupied = !PlainLevelSystem.Instance.IsCellGloballyOccupied(cell);

        return tileExists && notOccupied;
    }

    protected override void RecordToLevelSystem(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            PlainLevelSystem.Instance.AddNormalChestPosition(pos);
        }
    }
}