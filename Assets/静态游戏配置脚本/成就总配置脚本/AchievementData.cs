using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using System;

/// <summary>
/// 成就解锁条件类型
/// </summary>
public enum AchievementConditionType
{
    CountAccumulation,  // 累计次数型
    StateReach,         // 状态达成型
    OneTimeTrigger,     // 一次性触发型
    CollectionComplete,  // 收集完成型
    HonorTitle          // 荣誉称号型

}

/// <summary>
/// 成就解锁条件
/// </summary>
[Serializable]
public struct AchievementCondition
{
    [Header("条件类型")]
    public AchievementConditionType ConditionType;

    [Header("监听的事件名（和事件中心对应）")]
    public string ListenEventName;

    [Header("解锁目标值（累计次数/状态阈值）")]
    public float TargetValue;

}

/// <summary>
/// 单个成就的静态配置数据
/// </summary>
[Serializable]
public class AchievementData
{
    [Header("成就唯一ID")]
    public string AchievementId;

    [Header("成就名称")]
    public string AchievementName;

    [Header("成就对应的职业类型")]
    public ProfessionType PlayerProfessionType;

    [Header("成就图标")]
    public Sprite AchievementIcon;

    [Header("解锁条件")]
    public AchievementCondition UnlockCondition;

    [Header("成就描述")]
    public string Description;
}