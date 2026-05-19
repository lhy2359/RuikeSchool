using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TilemapGenerator : MonoBehaviour
{
    [Header("功能开关")]
    [SerializeField] private bool _isEnabled = true;

    [Header("=== 目标Tilemap ===")]
    [SerializeField] private Tilemap _targetTilemap;

    [Header("=== 生成用瓦片 ===")]
    [SerializeField] private TileBase _targetTile;

    [Header("=== 生成参数 ===")]
    [Tooltip("起点格子坐标 (X,Y,Z)")]
    [SerializeField] private Vector3Int _startGridPos = new Vector3Int(0, 0, 0);

    [Tooltip("线条生成长度（多少格）")]
    [SerializeField] private int _generateLength = 100;

    [Tooltip("矩形生成尺寸（宽X高，单位：格）")]
    [SerializeField] private Vector2Int _generateSize = new Vector2Int(20, 20);

    private List<Vector3Int> _lastGeneratedTiles = new List<Vector3Int>();
    private void OnEnable() { }
    private void OnDisable() { }

    [ContextMenu("✅ 生成【水平】瓦片（X轴）")]
    public void GenerateHorizontalWall()
    {
        if (!_isEnabled || !CheckValid()) return;

        _lastGeneratedTiles.Clear();

        for (int i = 0; i < _generateLength; i++)
        {
            Vector3Int gridPos = _startGridPos + new Vector3Int(i, 0, 0);
            _targetTilemap.SetTile(gridPos, _targetTile);
            _lastGeneratedTiles.Add(gridPos);
        }

        Debug.Log($"✅ 水平线条生成成功！长度：{_generateLength} 格 | 已记录可撤销");
    }

    [ContextMenu("✅ 生成【竖直】瓦片（Y轴）")]
    public void GenerateVerticalWall()
    {
        if (!_isEnabled || !CheckValid()) return;

        _lastGeneratedTiles.Clear();

        for (int i = 0; i < _generateLength; i++)
        {
            Vector3Int gridPos = _startGridPos + new Vector3Int(0, i, 0);
            _targetTilemap.SetTile(gridPos, _targetTile);
            _lastGeneratedTiles.Add(gridPos);
        }

        Debug.Log($"✅ 竖直线条生成成功！长度：{_generateLength} 格 | 已记录可撤销");
    }

    // 生成矩形区域
    [ContextMenu("✅ 生成【矩形】瓦片（宽X高）")]
    public void GenerateRectangle()
    {
        if (!_isEnabled || !CheckValidRectangle()) return;

        _lastGeneratedTiles.Clear();

        // 双重循环生成矩形区域
        for (int x = 0; x < _generateSize.x; x++)
        {
            for (int y = 0; y < _generateSize.y; y++)
            {
                Vector3Int gridPos = _startGridPos + new Vector3Int(x, y, 0);
                _targetTilemap.SetTile(gridPos, _targetTile);
                _lastGeneratedTiles.Add(gridPos);
            }
        }

        Debug.Log($"✅ 矩形区域生成成功！尺寸：{_generateSize.x}x{_generateSize.y} 格 | 总格子数：{_lastGeneratedTiles.Count} | 已记录可撤销");
    }

    [ContextMenu("❌ 清除【上一步】生成的瓦片")]
    public void ClearLastGenerated()
    {
        if (!_isEnabled || _targetTilemap == null || _lastGeneratedTiles.Count == 0)
        {
            Debug.LogWarning("⚠️ 没有可撤销的上一步操作！");
            return;
        }

        foreach (var pos in _lastGeneratedTiles)
        {
            _targetTilemap.SetTile(pos, null);
        }

        Debug.Log($"🗑️ 已撤销上一步，删除了 {_lastGeneratedTiles.Count} 格瓦片");
        _lastGeneratedTiles.Clear();
    }

    [ContextMenu("🗑️ 清除【全部】瓦片")]
    public void ClearAllTiles()
    {
        if (!_isEnabled || _targetTilemap == null) return;

        _targetTilemap.ClearAllTiles();
        _lastGeneratedTiles.Clear();
        Debug.Log("✅ 已清空Tilemap所有瓦片");
    }

    private bool CheckValid()
    {
        if (_targetTilemap == null || _targetTile == null)
        {
            Debug.LogError("❌ 请先拖入 Tilemap 和 生成瓦片！");
            return false;
        }
        if (_generateLength <= 0)
        {
            Debug.LogError("❌ 生成长度必须大于0！");
            return false;
        }
        return true;
    }

    private bool CheckValidRectangle()
    {
        if (_targetTilemap == null || _targetTile == null)
        {
            Debug.LogError("❌ 请先拖入 Tilemap 和 生成瓦片！");
            return false;
        }
        if (_generateSize.x <= 0 || _generateSize.y <= 0)
        {
            Debug.LogError("❌ 矩形宽和高必须都大于0！");
            return false;
        }
        return true;
    }
}