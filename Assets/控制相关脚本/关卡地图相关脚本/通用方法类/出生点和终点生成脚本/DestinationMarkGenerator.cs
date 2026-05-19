using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 出生点生成器
/// </summary>
public class DestinationMarkGenerator : BaseMarkPointGenerator
{
    public DestinationMarkGenerator(Scene targetScene, string tilemapName, GameObject destinationPrefab)
        : base(targetScene, tilemapName, destinationPrefab) { }

    // 占用1x1格子
    protected override Vector2Int GetOccupiedSize()
    {
        return Vector2Int.one;
    }

    // 自实例化终点预制体
    protected override void GenerateObjects(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject obj = Object.Instantiate(_prefab, pos, Quaternion.identity, _markPointParent);
            obj.name = $"Destination_{pos.x:F0}_{pos.y:F0}";
        }
    }

    // 单个Tile存在即可
    protected override bool IsCellValid(Tilemap map, Vector3Int cell)
    {
        // 两个条件必须同时满足：瓦片存在，格子没有被全局占用池包含
        bool tileExists = map.GetTile(cell) != null;
        bool notOccupied = !PlainLevelSystem.Instance.IsCellGloballyOccupied(cell);

        return tileExists && notOccupied;
    }

    // 记录到关卡系统的终点列表
    protected override void RecordToLevelSystem(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            PlainLevelSystem.Instance.AddDestinationPosition(pos);
        }
    }
}