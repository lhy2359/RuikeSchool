using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 药品等级枚举
/// </summary>
public enum MedicineLevel
{
    none, // 特殊药品默认
    Level1 = 1, // I级
    Level2 = 2, // II级
    Level3 = 3  // Max级
}

/// <summary>
/// 单个药品的静态配置数据
/// </summary>
[Serializable]
public class MedicineData : PropData
{
    public MedicineLevel Level;       // 药品等级
    public bool IsOnlySaleableAndOnlyDoctor;     // 是否只能售卖和医者专属（如果是只能售卖和医者专属时，购买价格无效）,对应SalePrice字段无效
    public int OnlySellPrice;            // 只能卖出价格（如果不能购只能卖出时有效）,对应SalePrice字段无效
}
