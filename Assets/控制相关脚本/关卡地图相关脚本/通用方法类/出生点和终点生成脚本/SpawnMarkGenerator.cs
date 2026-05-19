using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 出生点生成器
/// </summary>
public class SpawnMarkGenerator : BaseMarkPointGenerator
{
    public SpawnMarkGenerator(Scene targetScene, string tilemapName)
        : base(targetScene, tilemapName, null) { }

    // 占用1x1格子
    protected override Vector2Int GetOccupiedSize()
    {
        return Vector2Int.one;
    }

    // 出生点不生成任何物体
    protected override void GenerateObjects(List<Vector3> positions)
    {
        // 空实现，仅记录位置
    }

    // 单个Tile存在即可
    protected override bool IsCellValid(Tilemap map, Vector3Int cell)
    {
        // 两个条件必须同时满足：瓦片存在，格子没有被全局占用池包含
        bool tileExists = map.GetTile(cell) != null;
        bool notOccupied = !PlainLevelSystem.Instance.IsCellGloballyOccupied(cell);

        return tileExists && notOccupied;
    }

    // 记录到关卡系统的出生点列表
    protected override void RecordToLevelSystem(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            PlainLevelSystem.Instance.AddSpawnPosition(pos);
        }
    }
}