using System;
using UnityEngine;

/// <summary
/// 运行时卡牌数据
/// </summary>
[Serializable]
public class RuntimeCardData : IInventoryProp
{
    [SerializeField] private CardData _config;
    [SerializeField] private string _cardId;
    [SerializeField] private int _cardNum;  // 卡牌堆叠数量
    [SerializeField] private int _gridIndex;   // 背包格子索引
    [SerializeField] private bool _isOnlyUsableInLevel; // 是否只能在关卡内使用（用于特殊物品比如复制后产生的）

    // 实现接口
    public Sprite GetIcon() => _config?.Icon;
    public string GetPropID() => _cardId;
    public string GetDescription() => _config?.Description;
    public int GetPropCount() => _cardNum;

    public bool IsOnlyUsableInLevel => _isOnlyUsableInLevel;


    public CardData Config => _config;
    public string CardId => _cardId;
    public int CardNum => _cardNum;
    public int GridIndex => _gridIndex;

    // 设置卡牌堆叠数量
    public void SetCardNum(int value)
    {
        _cardNum = Mathf.Max(0, value);
    }
    
    // 设置卡牌格子索引
    public void SetGridIndex(int index)
    {
        _gridIndex = index;
    }

    public RuntimeCardData(string cardId, int cardNum, int gridIndex, bool isOnlyUsableInLevel)
    {
        _cardId = cardId;
        _cardNum = cardNum;
        _gridIndex = gridIndex;
        _isOnlyUsableInLevel = isOnlyUsableInLevel;
    }

    // 通过ID从SO配置表中绑定静态数据
    public void BindConfig()
    {
        // 空ID直接返回
        if (string.IsNullOrEmpty(_cardId))
        {
            Debug.LogError("卡牌ID为空，无法绑定配置！");
            return;
        }

        // 调用SO单例，自动获取配置
        _config = CardConfigSO.Instance.GetCardDataById(_cardId);

        if (_config == null)
        {
            Debug.LogError($"未找到ID为 {_cardId}的卡牌配置！");
        }
    }

    // 存档前置空引用
    public void PrepareForSave()
    {
        // 清空静态配置引用
        _config = null;
    }
}