using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MedicineManageSystem : MonoBehaviour
{
    public static MedicineManageSystem Instance { get; private set; }
    public event Action OnMedicineDataUpdated; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitNewGameMedicines();
    }

    private int _medicineMaxStack;
    private int _medicineGridCount;
    public int MedicineMaxStack => _medicineMaxStack;
    public int MedicineGridCount => _medicineGridCount;

    private Dictionary<int, RuntimeMedicineData> _medicineGridDict = new();
    private RuntimeMedicineData _quickBarMedicine;

    // 初始化药品格子
    private void InitializeGridSlots()
    {
        _medicineGridDict.Clear();
        for (int i = 0; i < _medicineGridCount; i++)
            _medicineGridDict.Add(i, null);
        _quickBarMedicine = null;
    }

    public void SetQuickBarMedicine(int gridIndex) => _quickBarMedicine = GetMedicineByGrid(gridIndex);
    public RuntimeMedicineData GetQuickBarMedicine() => _quickBarMedicine;
    public void ClearQuickBarMedicine() => _quickBarMedicine = null;

    private void NotifyDataChanged() => OnMedicineDataUpdated?.Invoke();

    // 新游戏初始化药品数据
    public void InitNewGameMedicines()
    {
        _medicineMaxStack = InventoryConfigSO.Instance.MedicineMaxStack;
        _medicineGridCount = InventoryConfigSO.Instance.MedicineGridCount;
        InitializeGridSlots();
        NotifyDataChanged();
    }

    // 从存档数据加载药品
    public void LoadMedicinesFromSaveData(List<RuntimeMedicineData> saveData, int saveMaxStack, int saveGridCount)
    {
        _medicineMaxStack = saveMaxStack;
        _medicineGridCount = saveGridCount;
        InitializeGridSlots();
        foreach (var data in saveData) { data.BindConfig(); _medicineGridDict[data.GridIndex] = data; }
        NotifyDataChanged();
    }

    // 保存前准备数据
    public (List<RuntimeMedicineData>, int, int) GetCurrentMedicinesForSave()
    {
        var list = _medicineGridDict.Values.Where(x => x != null).ToList();
        foreach (var data in list) data.PrepareForSave();
        return (list, _medicineMaxStack, _medicineGridCount);
    }

    public Dictionary<int, RuntimeMedicineData> GetAllMedicineGrids() => _medicineGridDict;

    // 添加药品
    public int AddMedicine(string medicineId, int addCount)
    {
        if (string.IsNullOrEmpty(medicineId) || addCount <= 0) return addCount;
        int remaining = addCount;

        var validSlots = _medicineGridDict.Where(p => p.Value != null && p.Value.MedicineId == medicineId).ToList();
        foreach (var slot in validSlots)
        {
            if (remaining <= 0) break;
            var item = slot.Value;
            int canAdd = _medicineMaxStack - item.MedicineNum;
            if (canAdd <= 0) continue;
            int realAdd = Math.Min(canAdd, remaining);
            item.SetMedicineNum(item.MedicineNum + realAdd);
            remaining -= realAdd;
        }

        if (remaining > 0)
        {
            var emptySlots = _medicineGridDict.Where(p => p.Value == null).ToList();
            foreach (var emptySlot in emptySlots)
            {
                if (remaining <= 0) break;
                int realAdd = Math.Min(_medicineMaxStack, remaining);
                _medicineGridDict[emptySlot.Key] = new RuntimeMedicineData(medicineId, realAdd, emptySlot.Key, MedicineLevel.none);
                _medicineGridDict[emptySlot.Key].BindConfig();
                remaining -= realAdd;
            }
        }

        if (_quickBarMedicine == null)
        {
            var firstValidMedicine = _medicineGridDict.Values.FirstOrDefault(m => m != null);
            if (firstValidMedicine != null)
            {
                _quickBarMedicine = firstValidMedicine;
                Debug.Log("✅ 自动将第一个药水设置到快捷栏");
            }
        }

        NotifyDataChanged();
        return remaining;
    }

    // 移除药品
    public int RemoveMedicine(string medicineId, int removeCount)
    {
        if (string.IsNullOrEmpty(medicineId) || removeCount <= 0) return removeCount;
        int remaining = removeCount;

        var targetSlots = _medicineGridDict
            .Where(p => p.Value != null && p.Value.MedicineId == medicineId)
            .Select(p => p.Value)
            .ToList();

        foreach (var item in targetSlots)
        {
            if (remaining <= 0) break;
            int realRemove = Math.Min(item.MedicineNum, remaining);
            item.SetMedicineNum(item.MedicineNum - realRemove);
            remaining -= realRemove;

            if (item.MedicineNum <= 0)
            {
                var slot = _medicineGridDict.First(p => p.Value == item);
                _medicineGridDict[slot.Key] = null;
                if (_quickBarMedicine == item) ClearQuickBarMedicine();
            }
        }

        NotifyDataChanged();
        return remaining;
    }

    // 找到第一个空格子
    public int FindFirstEmptyGrid()
    {
        for (int i = 0; i < _medicineGridCount; i++)
            if (_medicineGridDict.TryGetValue(i, out var data) && data == null) return i;
        return -1;
    }

    // 获取某种药品的总数量
    public int GetMedicineTotalCount(string medicineId)
    {
        return _medicineGridDict.Values
            .Where(item => item != null && item.MedicineId == medicineId)
            .Sum(item => item.MedicineNum);
    }

    // 检查是否有足够的药品
    public bool HasEnoughMedicine(string medicineId, int needCount)
    {
        return GetMedicineTotalCount(medicineId) >= needCount;
    }

    // 根据格子索引获取药品数据
    public RuntimeMedicineData GetMedicineByGrid(int gridIndex)
    {
        _medicineGridDict.TryGetValue(gridIndex, out var data);
        return data;
    }

    // 根据格子索引移除药品
    public void RemoveMedicineByGrid(int gridIndex)
    {
        if (_medicineGridDict.ContainsKey(gridIndex))
        {
            var removedItem = _medicineGridDict[gridIndex];
            _medicineGridDict[gridIndex] = null;
            if (_quickBarMedicine == removedItem) ClearQuickBarMedicine();
            NotifyDataChanged(); // 🔥 刷新UI
        }
    }

    // 使用药品
    public void UseQuickMedicine()
    {
        var medicine = GetQuickBarMedicine();
        if (medicine == null || medicine.MedicineNum <= 0) return;

        medicine.SetMedicineNum(medicine.MedicineNum - 1);
        if (medicine.MedicineNum <= 0)
        {
            ClearQuickBarMedicine();
        }
        NotifyDataChanged();
        Debug.Log("使用药品：" + medicine.MedicineId);
    }
}