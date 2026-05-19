using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 维修店生成器（1x2连续，当前格子和右边格子都存在才有效）
/// </summary>
public class RepairShopMarkGenerator : BaseMarkPointGenerator
{
    public RepairShopMarkGenerator(Scene targetScene, string tilemapName, GameObject repairShopPrefab)
        : base(targetScene, tilemapName, repairShopPrefab) { }

    // 重写占用尺寸为1行2列
    protected override Vector2Int GetOccupiedSize()
    {
        return new Vector2Int(2, 1);
    }

    // 生成维修店
    protected override void GenerateObjects(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject obj = Object.Instantiate(_prefab, pos, Quaternion.identity, _markPointParent);
            obj.name = $"{_prefab.name}_{pos:F0}";
        }
    }

    // 当前格子和右边格子都存在（1行2列）
    protected override bool IsCellValid(Tilemap map, Vector3Int cell)
    {
        // 两个条件必须同时满足：瓦片存在，格子没有被全局占用池包含
        Vector3Int rightCell = new Vector3Int(cell.x + 1, cell.y, 0);
        bool tileExists = map.GetTile(cell) != null && map.GetTile(rightCell) != null;
        bool notOccupied = !PlainLevelSystem.Instance.IsCellGloballyOccupied(cell) && !PlainLevelSystem.Instance.IsCellGloballyOccupied(rightCell);
        return tileExists && notOccupied;
    }

    // 记录到关卡系统的补给点列表
    protected override void RecordToLevelSystem(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            PlainLevelSystem.Instance.AddStorePosition(pos);
        }
    }
}