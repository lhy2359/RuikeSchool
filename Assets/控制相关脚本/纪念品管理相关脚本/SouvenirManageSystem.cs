using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SouvenirManageSystem : MonoBehaviour
{
    public static SouvenirManageSystem Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        InitNewGameSouvenirs();
    }

    // 配置数据
    private int _souvenirMaxStack;
    private int _souvenirGridCount;
    public int SouvenirMaxStack => _souvenirMaxStack;
    public int SouvenirGridCount => _souvenirGridCount;


    private Dictionary<int, RuntimeSouvenirData> _souvenirGridDict = new();

    // 初始化固定格子
    private void InitializeGridSlots()
    {
        _souvenirGridDict.Clear();
        for (int i = 0; i < _souvenirGridCount; i++)
        {
            _souvenirGridDict.Add(i, null);
        }
    }

    // 新游戏初始数据
    public void InitNewGameSouvenirs()
    {
        // 初始格子数从配置读取
        _souvenirMaxStack = InventoryConfigSO.Instance.SouvenirMaxStack;
        _souvenirGridCount = InventoryConfigSO.Instance.SouvenirGridCount;

        InitializeGridSlots();

        Debug.Log("✅ 新游戏纪念品初始化完成");
    }

    // 加载存档
    public void LoadSouvenirsFromSaveData(List<RuntimeSouvenirData> saveData, int saveMaxStack, int saveGridCount)
    {
        _souvenirMaxStack = saveMaxStack;
        _souvenirGridCount = saveGridCount;

        InitializeGridSlots();

        // 还原物品
        foreach (var data in saveData)
        {
            data.BindConfig();
            _souvenirGridDict[data.GridIndex] = data;
        }
        Debug.Log("✅ 存档纪念品加载完成");
    }

    // 保存数据
    public (List<RuntimeSouvenirData>, int saveMaxStack, int saveGridCount) GetCurrentSouvenirsForSave()
    {
        foreach (var data in _souvenirGridDict.Values)
        {
            data.PrepareForSave();
        }
        return (_souvenirGridDict.Values.ToList(), _souvenirMaxStack, _souvenirGridCount);
    }

    // 添加纪念品
    public int AddSouvenir(string souvenirId, int addCount)
    {
        if (string.IsNullOrEmpty(souvenirId) || addCount <= 0) return addCount;
        int remaining = addCount;

        // 优先堆叠同物品
        var validSlots = _souvenirGridDict.Where(p => p.Value != null && p.Value.SouvenirId == souvenirId).ToList();
        foreach (var slot in validSlots)
        {
            if (remaining <= 0) break;
            var item = slot.Value;
            int canAdd = _souvenirMaxStack - item.SouvenirNum;
            if (canAdd <= 0) continue;

            int realAdd = Mathf.Min(canAdd, remaining);
            item.SetSouvenirNum(item.SouvenirNum + realAdd);
            remaining -= realAdd;
        }

        // 剩余数量找空格子放置
        if (remaining > 0)
        {
            var emptySlots = _souvenirGridDict.Where(p => p.Value == null).ToList();
            foreach (var emptySlot in emptySlots)
            {
                if (remaining <= 0) break;

                int realAdd = Mathf.Min(_souvenirMaxStack, remaining);
                _souvenirGridDict[emptySlot.Key] = new RuntimeSouvenirData(souvenirId, realAdd, emptySlot.Key);
                _souvenirGridDict[emptySlot.Key].BindConfig();
                remaining -= realAdd;
            }
        }

        // 位置不够返回未添加的数量
        return remaining; 
    }

    // 移除纪念品
    public int RemoveSouvenir(string souvenirId, int removeCount)
    {
        if (string.IsNullOrEmpty(souvenirId) || removeCount <= 0) return removeCount;
        int remaining = removeCount;

        // 获取所有该物品的格子
        var targetSlots = _souvenirGridDict
            .Where(p => p.Value != null && p.Value.SouvenirId == souvenirId)
            .Select(p => p.Value)
            .ToList();

        foreach (var item in targetSlots)
        {
            if (remaining <= 0) break;

            int realRemove = Mathf.Min(item.SouvenirNum, remaining);
            item.SetSouvenirNum(item.SouvenirNum - realRemove);
            remaining -= realRemove;

            // 数量为0 → 格子置空
            if (item.SouvenirNum <= 0)
            {
                var slot = _souvenirGridDict.First(p => p.Value == item);
                _souvenirGridDict[slot.Key] = null;
            }
        }

        return remaining;
    }

    // 查找第一个空格子
    public int FindFirstEmptyGrid()
    {
        for (int i = 0; i < _souvenirGridCount; i++)
        {
            if (!_souvenirGridDict.ContainsKey(i)) return i;
        }
        return -1;
    }

    // 获取总数量
    public int GetSouvenirTotalCount(string souvenirId)
    {
        return _souvenirGridDict.Values
            .Where(item => item != null && item.SouvenirId == souvenirId)
            .Sum(item => item.SouvenirNum);
    }

    // 是否足够数量
    public bool HasEnoughSouvenir(string souvenirId, int needCount)
    {
        return GetSouvenirTotalCount(souvenirId) >= needCount;
    }

    // 得到格子上的药品数据
    public RuntimeSouvenirData GetSouvenirByGrid(int gridIndex)
    {
        _souvenirGridDict.TryGetValue(gridIndex, out var data);
        return data;
    }

    // 移除格子上的药品
    public void RemoveSouvenirByGrid(int gridIndex)
    {
        if (_souvenirGridDict.ContainsKey(gridIndex))
            _souvenirGridDict.Remove(gridIndex);
    }
}