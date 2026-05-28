using UnityEngine;

/// <summary>
/// 背包所有物品通用基础接口
/// </summary>
public interface IInventoryProp
{
    Sprite GetIcon();
    string GetPropID();
    string GetDescription();
    int GetPropCount(); // Item默认为1
}

/// <summary>
/// 物品专属接口
/// </summary>
public interface IUsableItem : IInventoryProp
{
    int GetRemainUse();
    int GetMaxUse();
}