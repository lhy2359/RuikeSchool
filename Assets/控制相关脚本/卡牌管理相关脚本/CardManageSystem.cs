using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManageSystem : MonoBehaviour
{
    public static CardManageSystem Instance { get; private set; }
    public event Action OnCardDataUpdated; 

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
        InitNewGameCards();
    }

    private int _cardMaxStack;
    private int _cardGridCount;
    public int CardMaxStack => _cardMaxStack;
    public int CardGridCount => _cardGridCount;

    private Dictionary<int, RuntimeCardData> _cardGridDict = new();
    private RuntimeCardData _quickBarCard;

    private void InitializeGridSlots()
    {
        _cardGridDict.Clear();
        for (int i = 0; i < _cardGridCount; i++)
            _cardGridDict.Add(i, null);
        _quickBarCard = null;
    }

    public void SetQuickBarCard(int gridIndex) => _quickBarCard = GetCardByGrid(gridIndex);
    public RuntimeCardData GetQuickBarCard() => _quickBarCard;
    public void ClearQuickBarCard() => _quickBarCard = null;

    private void NotifyDataChanged() => OnCardDataUpdated?.Invoke();

    // 初始化新游戏卡牌数据
    public void InitNewGameCards()
    {
        _cardMaxStack = InventoryConfigSO.Instance.CardMaxStack;
        _cardGridCount = InventoryConfigSO.Instance.CardGridCount;
        InitializeGridSlots();
        NotifyDataChanged();
    }

    public void LoadCardsFromSaveData(List<RuntimeCardData> saveData, int saveMaxStack, int saveGridCount)
    {
        _cardMaxStack = saveMaxStack;
        _cardGridCount = saveGridCount;
        InitializeGridSlots();
        foreach (var data in saveData) { data.BindConfig(); _cardGridDict[data.GridIndex] = data; }
        NotifyDataChanged();
    }

    public (List<RuntimeCardData>, int, int) GetCurrentCardsForSave()
    {
        var list = _cardGridDict.Values.Where(x => x != null).ToList();
        foreach (var data in list) data.PrepareForSave();
        return (list, _cardMaxStack, _cardGridCount);
    }

    public Dictionary<int, RuntimeCardData> GetAllCardGrids() => _cardGridDict;

    #region 核心添加卡牌
    public int AddCard(string cardId, int addCount) => AddCard(cardId, addCount, false);
    public int AddCard(string cardId, int addCount, bool isOnlyUsableInLevel)
    {
        if (string.IsNullOrEmpty(cardId) || addCount <= 0) return addCount;
        int remaining = addCount;

        var validSlots = _cardGridDict
            .Where(p => p.Value != null && p.Value.CardId == cardId && p.Value.IsOnlyUsableInLevel == isOnlyUsableInLevel)
            .ToList();

        foreach (var slot in validSlots)
        {
            if (remaining <= 0) break;
            var card = slot.Value;
            int canAdd = _cardMaxStack - card.CardNum;
            if (canAdd <= 0) continue;
            int realAdd = Math.Min(canAdd, remaining);
            card.SetCardNum(card.CardNum + realAdd);
            remaining -= realAdd;
        }

        if (remaining > 0)
        {
            var emptySlots = _cardGridDict.Where(p => p.Value == null).ToList();
            foreach (var emptySlot in emptySlots)
            {
                if (remaining <= 0) break;
                int realAdd = Math.Min(_cardMaxStack, remaining);
                var newCard = new RuntimeCardData(cardId, realAdd, emptySlot.Key, isOnlyUsableInLevel);
                newCard.BindConfig();
                _cardGridDict[emptySlot.Key] = newCard;
                remaining -= realAdd;
            }
        }

        if (_quickBarCard == null)
        {
            var firstValidCard = _cardGridDict.Values.FirstOrDefault(m => m != null);
            if (firstValidCard != null)
            {
                _quickBarCard = firstValidCard;
                Debug.Log("自动将第一个卡牌设置到快捷栏");
            }
        }

        NotifyDataChanged();
        return remaining;
    }
    #endregion

    public int RemoveCard(string cardId, int removeCount, bool isOnlyUsableInLevel = false)
    {
        if (string.IsNullOrEmpty(cardId) || removeCount <= 0) return removeCount;
        int remaining = removeCount;

        var targetSlots = _cardGridDict
            .Where(p => p.Value != null && p.Value.CardId == cardId && p.Value.IsOnlyUsableInLevel == isOnlyUsableInLevel)
            .Select(p => p.Value)
            .ToList();

        foreach (var card in targetSlots)
        {
            if (remaining <= 0) break;
            int realRemove = Math.Min(card.CardNum, remaining);
            card.SetCardNum(card.CardNum - realRemove);
            remaining -= realRemove;

            if (card.CardNum <= 0)
            {
                var slot = _cardGridDict.First(p => p.Value == card);
                _cardGridDict[slot.Key] = null;
                if (_quickBarCard == card) ClearQuickBarCard();
            }
        }

        NotifyDataChanged(); 
        return remaining;
    }

    public int FindFirstEmptyGrid()
    {
        for (int i = 0; i < _cardGridCount; i++)
            if (_cardGridDict.TryGetValue(i, out var data) && data == null) return i;
        return -1;
    }

    public int GetCardTotalCount(string cardId, bool isOnlyUsableInLevel = false)
    {
        return _cardGridDict.Values
            .Where(card => card != null && card.CardId == cardId && card.IsOnlyUsableInLevel == isOnlyUsableInLevel)
            .Sum(card => card.CardNum);
    }

    public bool HasEnoughCard(string cardId, int needCount, bool isOnlyUsableInLevel = false)
    {
        return GetCardTotalCount(cardId, isOnlyUsableInLevel) >= needCount;
    }

    public RuntimeCardData GetCardByGrid(int gridIndex)
    {
        _cardGridDict.TryGetValue(gridIndex, out var data);
        return data;
    }

    public void RemoveCardByGrid(int gridIndex)
    {
        if (_cardGridDict.ContainsKey(gridIndex))
        {
            var removedItem = _cardGridDict[gridIndex];
            _cardGridDict[gridIndex] = null;
            if (_quickBarCard == removedItem) ClearQuickBarCard();
            NotifyDataChanged();
        }
    }

    public void UseQuickCard()
    {
        var card = GetQuickBarCard();
        if (card == null || card.CardNum <= 0) return;

        card.SetCardNum(card.CardNum - 1);
        if (card.CardNum <= 0)
        {
            ClearQuickBarCard();
        }
        NotifyDataChanged();
        Debug.Log("使用卡牌：" + card.CardId);
    }
}