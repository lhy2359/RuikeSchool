using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 单个物品的静态配置数据
/// </summary>
[Serializable]
public class ItemData : PropData
{
    public int DefaultMaxUses;    // 默认最大使用次数
    public bool IsOncePerLevel;   // 是否每关只能使用一次
}

