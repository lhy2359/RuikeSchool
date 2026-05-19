using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌配置数据
/// </summary>
[Serializable]
public class CardData : PropData
{
    [Header("卡牌基础信息")]
    public EchelonType EchelonType;      // 位阶
    public CardType CardType;            // 卡牌类型
    public String FlavorfulText;         // 风味文字

    [Header("是否职业专属")]
    public bool IsProfessionExclusive;    // 开启：本职业专属，其他职业不能使用此卡牌

    [Header("职业协同设置")]
    public bool IsProfessionSynergy; // 开启：职业匹配时触发协同效果

    [Header("协同效果（仅【职业匹配】时执行）")]
    public List<EffectUnitListHappenEvent> SynergyEffectEvents;

}

/// <summary>
/// 卡牌位阶（正论/逆论/悖论）
/// </summary>
public enum EchelonType
{
    none,
    ThesisMode,     // 正论
    AntithesisMode, // 逆论
    ParadoxMode     // 悖论
}

/// <summary>
/// 卡牌类型（现世/记忆/终焉）
/// </summary>
public enum CardType
{
    none,
    Mundus,   // 现世
    Mnemos,   // 记忆
    Finis     // 终焉
}