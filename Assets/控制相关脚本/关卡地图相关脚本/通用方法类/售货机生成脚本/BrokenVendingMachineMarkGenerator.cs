using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 损坏自动售货机生成器（2行1列纵向，当前格子和上方格子都存在才有效）
/// </summary>
public class BrokenVendingMachineMarkGenerator : BaseMarkPointGenerator
{
    public BrokenVendingMachineMarkGenerator(Scene targetScene, string tilemapName, GameObject brokenVendingPrefab)
        : base(targetScene, tilemapName, brokenVendingPrefab) { }

    // 占用尺寸为2行1列（纵向高2格）
    protected override Vector2Int GetOccupiedSize()
    {
        return new Vector2Int(1, 2);
    }

    // 生成损坏自动售货机
    protected override void GenerateObjects(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject obj = Object.Instantiate(_prefab, pos, Quaternion.identity, _markPointParent);
            obj.name = $"{_prefab.name}_{pos:F0}";
        }
    }

    // 当前格子和上方格子都存在（2行1列）
    protected override bool IsCellValid(Tilemap map, Vector3Int cell)
    {
        // 两个条件必须同时满足：瓦片存在，格子没有被全局占用池包含
        Vector3Int topCell = new Vector3Int(cell.x, cell.y + 1, 0);
        bool tileExists = map.GetTile(cell) != null && map.GetTile(topCell) != null;
        bool notOccupied = !PlainLevelSystem.Instance.IsCellGloballyOccupied(cell) && !PlainLevelSystem.Instance.IsCellGloballyOccupied(topCell);
        return tileExists && notOccupied;
    }

    // 记录到关卡系统的自动售货机列表
    protected override void RecordToLevelSystem(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            PlainLevelSystem.Instance.AddBrokenVendingMachinePosition(pos);
        }
    }
}