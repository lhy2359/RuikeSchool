using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TilemapCopyMarkerTool : MonoBehaviour
{
    [Header("=== 源Tilemaps ===")]
    [SerializeField] private List<Tilemap> _sourceTilemaps = new List<Tilemap>();

    [Header("=== 目标Tilemap ===")]
    [SerializeField] private Tilemap _targetTilemap;

    [Header("=== 替换用的MarkBlock瓦片 ===")]
    [SerializeField] private TileBase _markBlockTile;

    [Header("=== 选项 ===")]
    [Tooltip("复制前是否清空目标Tilemap？建议勾选，避免重复")]
    [SerializeField] private bool _clearTargetBeforeCopy = true;


    [ContextMenu("✅ 一键复制区域到目标Tilemap（替换为MarkBlock）")]
    public void CopyGrassToChestSpawn()
    {
        // 基础校验
        if (_sourceTilemaps.Count == 0 || _targetTilemap == null || _markBlockTile == null)
        {
            Debug.LogError("❌ 请先拖入源Tilemap、目标Tilemap和MarkBlock瓦片！");
            return;
        }

        // 复制前清空目标（可选）
        if (_clearTargetBeforeCopy)
        {
            _targetTilemap.ClearAllTiles();
            Debug.Log("已清空目标Tilemap");
        }

        // 收集所有源Tilemap的非空格子坐标（去重，避免重复设置）
        HashSet<Vector3Int> allSourcePositions = new HashSet<Vector3Int>();
        foreach (var sourceTilemap in _sourceTilemaps)
        {
            if (sourceTilemap == null) continue;

            BoundsInt bounds = sourceTilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, 0);
                    if (sourceTilemap.GetTile(gridPos) != null)
                    {
                        allSourcePositions.Add(gridPos);
                    }
                }
            }
        }

        // 把所有收集到的格子，在目标Tilemap里替换成标记瓦片
        foreach (var pos in allSourcePositions)
        {
            _targetTilemap.SetTile(pos, _markBlockTile);
        }

        Debug.Log($"✅ 复制完成！共复制 {allSourcePositions.Count} 个格子，已替换为MarkBlock瓦片");
    }


    [ContextMenu("🗑️ 清空目标Tilemap")]
    public void ClearTargetTilemap()
    {
        if (_targetTilemap == null) return;
        _targetTilemap.ClearAllTiles();
        Debug.Log("已清空目标Tilemap");
    }
}