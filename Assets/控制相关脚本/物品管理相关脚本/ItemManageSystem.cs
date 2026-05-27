using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemManageSystem : MonoBehaviour
{
    public static ItemManageSystem Instance { get; private set; }
    public event Action OnItemDataUpdated;

    private int _currentItemTotalGridCount;
    private int _currentQuickBarCount;
    public int CurrentQuickBarCount
    {
        get => _currentQuickBarCount;
        set
        {
            _currentQuickBarCount = value;
            InitializeQuickBarGrids(); // 重新初始化快捷栏容器
            OnItemDataUpdated?.Invoke(); // 自动刷新UI
        }
    }

    private Dictionary<int, RuntimeItemData> _itemGridDict = new();
    private Dictionary<int, RuntimeItemData> _quickBarItems = new();
    private RuntimeItemData _lastUsedItem;

    public int CurrentTotalGrid => _currentItemTotalGridCount;
    public RuntimeItemData LastUsedItem => _lastUsedItem;

    //  UI 获取快捷栏所有物品
    public Dictionary<int, RuntimeItemData> GetAllQuickBarItems()
    {
        return _quickBarItems;
    }

    private void Awake()
    {
        Debug.Log("aaaaaaaaaaaaaaaa");
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        InitNewGameItems();
    }

    public void NotifyDataChanged() => OnItemDataUpdated?.Invoke();

    public void SwapItemGrid(int fromIndex, int toIndex)
    {
        if (!_itemGridDict.ContainsKey(fromIndex) || !_itemGridDict.ContainsKey(toIndex)) return;
        var temp = _itemGridDict[fromIndex];
        _itemGridDict[fromIndex] = _itemGridDict[toIndex];
        _itemGridDict[toIndex] = temp;
        NotifyDataChanged();
    }

    private void InitializeInventoryGrids()
    {
        _itemGridDict.Clear();
        for (int i = 0; i < _currentItemTotalGridCount; i++) _itemGridDict.Add(i, null);
    }

    private void InitializeQuickBarGrids()
    {
        _quickBarItems.Clear();
        for (int i = 0; i < _currentQuickBarCount; i++) _quickBarItems.Add(i, null);
    }

    public bool IsValidQuickBarIndex(int quickIndex) => quickIndex >= 0 && quickIndex < _currentQuickBarCount;

    public bool SetItemToQuickBar(int quickBarIndex, int inventoryGridIndex)
    {
        if (!IsValidQuickBarIndex(quickBarIndex) || !_itemGridDict.ContainsKey(inventoryGridIndex)) return false;
        _quickBarItems[quickBarIndex] = _itemGridDict[inventoryGridIndex];
        OnItemDataUpdated?.Invoke();
        return true;
    }

    public RuntimeItemData GetQuickBarItem(int quickBarIndex)
    {
        if (!IsValidQuickBarIndex(quickBarIndex)) return null;
        _quickBarItems.TryGetValue(quickBarIndex, out var item);
        if (item == null) Debug.Log($"快捷栏格子 {quickBarIndex} 为空");
        return item;
    }

    public void ClearQuickBarItem(int quickBarIndex)
    {
        if (IsValidQuickBarIndex(quickBarIndex)) { _quickBarItems[quickBarIndex] = null; OnItemDataUpdated?.Invoke(); }
    }

    public void ClearAllQuickBar()
    {
        for (int i = 0; i < _currentQuickBarCount; i++) _quickBarItems[i] = null;
        OnItemDataUpdated?.Invoke();
    }

    public bool UnlockExtraQuickSlotAndCopyItem()
    {
        if (_lastUsedItem == null) { Debug.LogWarning("暂无上一个使用的物品"); return false; }
        CurrentQuickBarCount++;
        int newSlotIndex = _currentQuickBarCount - 1;
        var copiedItem = CopyItemForLevel(_lastUsedItem);
        copiedItem.SetGridIndex(-1);
        _quickBarItems[newSlotIndex] = copiedItem;
        Debug.Log($"解锁快捷栏：{newSlotIndex}");
        return true;
    }

    public void SetLastUsedItem(RuntimeItemData item) => _lastUsedItem = item;
    public void ClearLastUsedItem() => _lastUsedItem = null;

    public RuntimeItemData CopyItemForLevel(RuntimeItemData original)
    {
        if (original == null) return null;
        return new RuntimeItemData(original.ItemId, original.MaxUses, original.RemainingUses, -1, true);
    }

    public void UseItem(int gridIndex)
    {
        Debug.Log($"尝试使用物品格子 {gridIndex}");
        var item = GetItemByGrid(gridIndex);
        if (item == null || item.RemainingUses <= 0)
        {
            Debug.Log("物品为空或者物品使用次数为0！");
            return;
        }
        ExecuteItemEffect(item);
        item.SetRemainingUses(item.RemainingUses - 1);
        SetLastUsedItem(item);
        OnItemDataUpdated?.Invoke();
    }

    // 统一分发物品效果 → 调用对应功能系统
    private void ExecuteItemEffect(RuntimeItemData item)
    {

    }

    public void UseQuickBarItem(int quickIndex)
    {
        var item = GetQuickBarItem(quickIndex);

        if (item == null || item.RemainingUses <= 0 || item.GridIndex == -1) return;
        item.SetRemainingUses(item.RemainingUses - 1);
        SetLastUsedItem(item);
        if (item.RemainingUses <= 0)
        {
            ClearQuickBarItem(quickIndex);
        }
        else
        {
            OnItemDataUpdated?.Invoke();
        }
    }

    public void InitNewGameItems()
    {
        _currentItemTotalGridCount = InventoryConfigSO.Instance.ItemTotalGridCount;
        _currentQuickBarCount = InventoryConfigSO.Instance.ItemQuickBarCount;
        InitializeInventoryGrids();
        InitializeQuickBarGrids();
        OnItemDataUpdated?.Invoke();
        Debug.Log($"物品系统初始化 | 背包：{_currentItemTotalGridCount} | 快捷栏：{_currentQuickBarCount}");
    }

    public void LoadItemsFromSaveData(List<RuntimeItemData> saveData, int saveTotalGrid, int saveQuickBar)
    {
        _currentItemTotalGridCount = saveTotalGrid;
        _currentQuickBarCount = saveQuickBar;
        InitializeInventoryGrids();
        foreach (var data in saveData) { data.BindConfig(); _itemGridDict[data.GridIndex] = data; }
        InitializeQuickBarGrids();
        OnItemDataUpdated?.Invoke();
    }

    public (List<RuntimeItemData> itemData, int totalGrid, int quickBar) GetCurrentItemsForSave()
    {
        foreach (var data in _itemGridDict.Values) data?.PrepareForSave();
        return (_itemGridDict.Values.ToList(), _currentItemTotalGridCount, _currentQuickBarCount);
    }

    public void UnlockMoreGrids(int newInventoryGrid, int newQuickBarGrid)
    {
        if (newInventoryGrid < _currentItemTotalGridCount || newQuickBarGrid < 0) { Debug.LogWarning("格子数无效"); return; }
        _currentItemTotalGridCount = newInventoryGrid;
        InitializeInventoryGrids();
        _currentQuickBarCount = newQuickBarGrid;
        InitializeQuickBarGrids();
        OnItemDataUpdated?.Invoke();
    }

    public bool AddItem(string itemId, int maxUses, int remainingUses)
    {
        int emptyIndex = FindFirstEmptyGrid();
        if (emptyIndex == -1) { Debug.Log("物品栏已满"); return false; }
        var newData = new RuntimeItemData(itemId, maxUses, remainingUses, emptyIndex, false);
        newData.BindConfig();
        _itemGridDict[emptyIndex] = newData;
        // 如果快捷栏有空位，自动把物品放到快捷栏
        for (int i = 0; i < _currentQuickBarCount; i++)
        {
            if (_quickBarItems[i] == null)
            {
                _quickBarItems[i] = newData;
                break;
            }
        }
        OnItemDataUpdated?.Invoke();
        return true;
    }

    private int FindFirstEmptyGrid()
    {
        for (int i = 0; i < _currentItemTotalGridCount; i++) if (_itemGridDict[i] == null) return i;
        return -1;
    }

    public RuntimeItemData GetItemByGrid(int gridIndex)
    {
        _itemGridDict.TryGetValue(gridIndex, out var data);
        return data;
    }

    public Dictionary<int, RuntimeItemData> GetAllInventoryGrids() => _itemGridDict;

    public void RemoveItemByGrid(int gridIndex)
    {
        if (_itemGridDict.ContainsKey(gridIndex))
        {
            for (int i = 0; i < _currentQuickBarCount; i++) if (_quickBarItems[i] == _itemGridDict[gridIndex]) _quickBarItems[i] = null;
            _itemGridDict[gridIndex] = null;
            OnItemDataUpdated?.Invoke();
        }
    }

    public void ResetCurrentItems()
    {
        _itemGridDict.Clear();
        _quickBarItems.Clear();
        InitNewGameItems();
    }
}