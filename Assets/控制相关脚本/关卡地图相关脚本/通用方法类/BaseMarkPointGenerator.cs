using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 所有MarkPoint生成器的抽象基类
/// 通用逻辑（找Tilemap、收集格子、随机选择、全局防重叠）已封装
/// 子类仅需重写格子校验规则 + 位置记录逻辑
/// </summary>
public abstract class BaseMarkPointGenerator
{
    protected readonly string _tilemapName;
    protected readonly Scene _targetScene;
    protected readonly GameObject _prefab; // 对应物体预制体（空=仅记录位置）
    protected Transform _markPointParent; // 统一父物体

    protected BaseMarkPointGenerator(Scene targetScene, string tilemapName, GameObject prefab = null)
    {
        _targetScene = targetScene;
        _tilemapName = tilemapName;
        _prefab = prefab;
        _markPointParent = FindOrCreateMarkPointParent();
    }

    #region 通用工具方法
    /// <summary>
    /// 查找或创建统一父物体
    /// </summary>
    protected Transform FindOrCreateMarkPointParent()
    {
        GameObject parent = GameObject.Find("GeneratedMarkPoints");
        if (parent == null)
        {
            parent = new GameObject("GeneratedMarkPoints");
            SceneManager.MoveGameObjectToScene(parent, _targetScene);
        }
        return parent.transform;
    }
    #endregion

    #region 通用逻辑（所有子类共用，无需修改）
    /// <summary>
    /// 递归查找场景中的Tilemap
    /// </summary>
    protected Tilemap FindTilemap()
    {
        GameObject[] allRoots = _targetScene.GetRootGameObjects();
        foreach (var root in allRoots)
        {
            Tilemap map = FindTilemapRecursive(root.transform, _tilemapName);
            if (map != null) return map;
        }
        Debug.LogError($"❌ 未找到场景里名为 {_tilemapName} 的Tilemap");
        return null;
    }

    private Tilemap FindTilemapRecursive(Transform parent, string tilemapName)
    {
        Tilemap t = parent.GetComponent<Tilemap>();
        if (t != null && t.name == tilemapName)
            return t;

        foreach (Transform child in parent)
        {
            Tilemap res = FindTilemapRecursive(child, tilemapName);
            if (res != null) return res;
        }
        return null;
    }

    /// <summary>
    /// 收集所有符合规则的有效格子
    /// </summary>
    public List<Vector3Int> GetAllValidCells(Tilemap map)
    {
        List<Vector3Int> validCells = new List<Vector3Int>();
        BoundsInt bounds = map.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                // 核心：调用子类重写的校验规则
                if (IsCellValid(map, cell))
                {
                    validCells.Add(cell);
                }
            }
        }
        return validCells;
    }

    /// <summary>
    /// 随机选择n个格子（全局防重叠，所有生成器共用占用规则）
    /// </summary>
    public List<Vector3Int> SelectRandomCells(List<Vector3Int> validCells, int count)
    {
        List<Vector3Int> selectedCells = new List<Vector3Int>();
        // 检查所有占用格子是否都未被占用
        List<Vector3Int> availableCells = validCells.FindAll(c => !IsAnyCellOccupied(c));

        if (availableCells.Count == 0)
        {
            Debug.LogWarning($"⚠️ {_tilemapName} 无可用格子，已被全局占用！");
            return selectedCells;
        }

        // 随机选择，最多选count个
        int selectCount = Mathf.Min(count, availableCells.Count);
        for (int i = 0; i < selectCount; i++)
        {
            int index = Random.Range(0, availableCells.Count);
            Vector3Int selected = availableCells[index];

            selectedCells.Add(selected);
            OccupyAllCells(selected);
            availableCells.RemoveAt(index);
        }

        return selectedCells;
    }

    /// <summary>
    /// 生成并记录位置
    /// </summary>
    public List<Vector3> GenerateAndRecord(int generateCount = 1)
    {
        List<Vector3> worldPositions = new List<Vector3>();
        Tilemap map = FindTilemap();
        if (map == null) return worldPositions;

        List<Vector3Int> allValidCells = GetAllValidCells(map);
        if (allValidCells.Count == 0)
        {
            Debug.LogWarning($"{_tilemapName} 没有符合规则的有效格子");
            return worldPositions;
        }

        List<Vector3Int> selectedCells = SelectRandomCells(allValidCells, generateCount);
        foreach (var cell in selectedCells)
        {
            // 计算世界坐标（瓦片中心）
            Vector3 worldPos = map.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
            worldPositions.Add(worldPos);
            Debug.Log($"🎯 选中格子：({cell.x}, {cell.y}) → 世界坐标：{worldPos:F2}");
        }

        // 通知LevelSystem记录生成的位置
        RecordToLevelSystem(worldPositions);
        if (_prefab != null)
        {
            GenerateObjects(worldPositions);
        }
        return worldPositions;
    }
    #endregion

    #region 可重写的差异化方法（子类按需实现）
    /// <summary>
    /// 物体占用的格子尺寸（子类重写）
    /// </summary>
    protected abstract Vector2Int GetOccupiedSize();

    /// <summary>
    /// 实例化物体（子类重写）
    /// </summary>
    protected abstract void GenerateObjects(List<Vector3> positions);
    //{
    //    foreach (var pos in positions)
    //    {
    //        GameObject obj = Object.Instantiate(_prefab, pos, Quaternion.identity, _markPointParent);
    //        obj.name = $"{_prefab.name}_{pos:F0}";
    //    }
    //}

    /// <summary>
    /// 格子校验规则（子类重写：1x1/1x2/2x2等自定义规则）
    /// </summary>
    protected abstract bool IsCellValid(Tilemap map, Vector3Int cell);

    /// <summary>
    /// 将生成的位置记录到PlainLevelSystem
    /// </summary>
    protected abstract void RecordToLevelSystem(List<Vector3> positions);
    #endregion

    #region 内部辅助方法（自动处理多格子占用）
    /// <summary>
    /// 检查基准格子对应的所有占用格子是否都未被占用
    /// </summary>
    private bool IsAnyCellOccupied(Vector3Int baseCell)
    {
        Vector2Int size = GetOccupiedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cell = baseCell + new Vector3Int(x, y, 0);
                if (PlainLevelSystem.Instance.IsCellGloballyOccupied(cell))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 标记基准格子对应的所有占用格子为已占用
    /// </summary>
    private void OccupyAllCells(Vector3Int baseCell)
    {
        Vector2Int size = GetOccupiedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cell = baseCell + new Vector3Int(x, y, 0);
                PlainLevelSystem.Instance.OccupyGlobalCell(cell);
            }
        }
    }

    /// <summary>
    /// 计算物体中心位置（1x2自动居中在两个格子中间）
    /// </summary>
    private Vector3 CalculateObjectCenterPosition(Tilemap map, Vector3Int baseCell)
    {
        Vector2Int size = GetOccupiedSize();
        Vector3 basePos = map.CellToWorld(baseCell);
        // 偏移到物体中心
        return basePos + new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
    }
    #endregion
}