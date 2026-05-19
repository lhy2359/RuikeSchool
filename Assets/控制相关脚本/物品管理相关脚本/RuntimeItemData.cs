using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时物品数据
/// </summary>
[Serializable]
public class RuntimeItemData : IUsableItem
{

    [SerializeField] private ItemData _config;
    [SerializeField] private string _itemId;
    [SerializeField] private int _maxUses;
    [SerializeField] private int _remainingUses;
    [SerializeField] private int _gridIndex; // 格子索引（用于背包）
    [SerializeField] private bool _isOnlyUsableInLevel; // 是否只能在关卡内使用（用于特殊物品比如复制后产生的）

    public Sprite GetIcon() => _config?.Icon;
    public string GetPropID() => _itemId;
    public string GetDescription() => _config?.Description;
    public int GetPropCount() => 1;
    public int GetRemainUse() => _remainingUses;
    public int GetMaxUse() => _maxUses;


    public ItemData Config => _config;
    public string ItemId => _itemId;
    public int MaxUses => _maxUses;
    public int RemainingUses => _remainingUses;
    public int GridIndex => _gridIndex;
    public bool IsOnlyUsableInLevel => _isOnlyUsableInLevel;
    

    // 设置最大使用次数
    public void SetMaxUses(int value) => _maxUses = value;
    public void SetRemainingUses(int value) => _remainingUses = value;
    public void SetGridIndex(int value) => _gridIndex = value;
    public void SetIsOnlyUsableInLevel(bool value) => _isOnlyUsableInLevel = value;

    public RuntimeItemData(string itemId, int maxUses, int remainingUses, int gridIndex, bool isOnlyUsableInLevel)
    {
        _itemId = itemId;
        _maxUses = maxUses;
        _remainingUses = remainingUses;
        _gridIndex = gridIndex;
        _isOnlyUsableInLevel = isOnlyUsableInLevel;
    }

    // 通过ID从SO配置表中绑定静态数据
    public void BindConfig()
    {
        // 空ID直接返回
        if (string.IsNullOrEmpty(_itemId))
        {
            Debug.LogError("成就ID为空，无法绑定配置！");
            return;
        }

        // 调用SO单例，自动获取配置
        _config = ItemConfigSO.Instance.GetItemDataById(_itemId);

        if (_config == null)
        {
            Debug.LogError($"未找到ID为 {_itemId}的物品配置！");
        }
    }

    // 存档前置空引用
    public void PrepareForSave()
    {
        // 清空静态配置引用
        _config = null;
    }

    
}
