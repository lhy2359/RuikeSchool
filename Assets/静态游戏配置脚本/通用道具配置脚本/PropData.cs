using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 职业类型
/// </summary>
[Serializable]
public enum ProfessionType
{
    None,
    Coder,
    Painter,
    Architect,
    Doctor
}

/// <summary>
/// 抽象道具数据类：物品数据，药品数据，卡牌数据派生自这个
/// </summary>
[Serializable]
public abstract class PropData
{
    public string PropId;         // 唯一ID
    public string PropName;       // 名称
    public ProfessionType ProfessionType; // 对应职业
    public Sprite Icon;           // 图标
    public string Description;    // 物品描述
    public int SalePrice;        // 购买价格

    [Header("道具效果事件列表")]
    public List<EffectUnitListHappenEvent> EffectEvents; // 效果事件列表，拆分成多个子事件

}

# region 效果事件详细构成
/// <summary>
/// 独立道具效果单元
/// </summary>
[Serializable]
public class EffectUnit
{
    [Header("发生次数")]
    public int OccurrenceCount = 1;

    [Header("效果作用目标")]
    public EffectTarget Target;

    [Header("只能选一个!!!")]
    [Header("对应目标的具体效果")]
    public PlayerEffectType PlayerEffect;
    public CameraEffectType CameraEffect;
    public MapEffectType MapEffect;
    public RewardFacilityEffectType FacilityEffect;
    public PropEffectType PropEffect;
    public LevelEffectType LevelEffect;

    [Header("效果极性（Buff/Debuff/中立）")]
    public EffectPolarity Polarity;

    [Header("只能选一个!!!")]
    [Header("效果参数")]
    public PlayerEffectParameter PlayerParameters;
    public CameraEffectParameter CameraParameters;
    public MapEffectParameter MapParameters;
    public RewardFacilityEffectParameter FacilityParameters;
    public PropEffectParameter PropParameters;
    public LevelEffectParameter LevelParameters;
}

/// <summary>
/// 效果单元列表包装类
/// 每个实例代表一组效果单元列表
/// </summary>
[Serializable]
public class EffectUnitListWrapper
{
    [Header("该组的效果单元列表")]
    public List<EffectUnit> EffectUnits;
}


/// <summary>
///  每个效果事件出现概率
/// </summary>
[Serializable]
public struct EffectUnitListHappenEvent
{
    public float HappenProbability;   // 事件发生概率（0-100）
    public int M;             // 效果单元列表数量M（每个事件包含M个效果单元，M>=1）
    public int N;             // 从效果单元列表中随机选择N个效果单元生效（N<=M）
    public List<EffectUnitListWrapper> EffectUnitsList; // 效果单元列表（包含M个效果单元列表，每个效果单元列表至少1个效果）
}
#endregion

#region 效果目标和效果类型详细分类
public enum EffectTarget
{
    Player,         // 玩家自身（属性，BUFF，快捷栏增加
    Camera,         // 摄像机（视角、特效等）
    Map,            // 地图场景（火把、改变地形等）
    RewardFacility, // 奖励设施（宝箱、售货机）
    Prop,           // 道具（复制
    Level,          // 关卡结算，关卡全局事件等
    Disparity       // 参差事件
}

public enum PlayerEffectType
{
    none,

    // 改变当前稳定值：急救包, 随机数种子干扰器, 急救针, 血袋，五号化合物
    ChangeCurrentStability,

    // 改变最大稳定值：五号化合物
    ChangeMaxStability,

    // 改变稳定值衰减速度：肾上腺素
    ChangeStabilityDecaySpeed,

    // 改变幸运值：幸运币
    ChangeLuckValue,

    // 改变视野范围：望远镜，肾上腺素，双重视界
    ChangeVisionRadius,

    // 改变移动速度：肾上腺素
    ChangeMoveSpeed,

    // 得到逾界机动能力
    GetBoundaryOverrideAbility,

    // 清除所有负面效果
    RemoveAllDebuffs,

    // 增加额外快捷栏并且复制上一个使用的物品：虚拟机
    AddExtraInventorySlotAndCopyLastUsedItem,

    // 消耗金币并且随机生成药品
    ConsumeGoldAndRandomlyProduceMedicine,

    // 每秒稳定值额外变化
    ChangeStabilityPerSecond,
}

public enum CameraEffectType
{
    none,

    // 改变摄像机视野大小
    ChangeCameraViewSize,

    // 生成特效（播放动画/光影效果）
    SpawnVisualEffect,

    // 开启透视视野
    EnableXRay
}

public enum MapEffectType
{
    none,

    // 在地图上放置结构
    PlaceStructure,

    // 影响地图（改变地形等
    ImpactMap,

    // 高亮显示最近的指定类型事件点
    HighlightClosestSpecificEventPoint
}

public enum RewardFacilityEffectType
{
    none,

    // 影响奖励设施（改变奖励设施状态、内容、显示等）：修复扳手、望远镜
    ImpactRewardFacility
}

public enum PropEffectType
{
    none,

    // 产生血袋
    SpawnBloodBag,

    // 随机产生一个新的药品
    RandomlySpawnMedicine,

    // 改变上一个使用物品剩余使用次数
    ChangeLastUsedItemRemainingUses,

    // 选择一个物品并改变剩余使用次数
    SelectItemAndChangeRemainingUses,

    // 改变物品最大使用次数
    ChangeItemMaxUses,

    // 销毁选择的物品
    DestorySelectedItemAfterLevel,

    // 上一次使用的卡牌数量加1
    IncreaseLastUsedCard
}

public enum LevelEffectType
{
    none,

    // 关卡结算时增加额外奖励
    AddExtraRewardAtLevelEnd,

    // 直接通关
    InstantLevelClear
}
#endregion

/// <summary>
/// 效果极性分类
/// </summary>
public enum EffectPolarity
{
    None,       // 中立效果：火把、设施等
    Buff,       // 正向增益
    Debuff      // 负面减益
}

[Serializable]
public class ChangeAttributeParameter
{
    [Header("效果持续时间类型，持续时间，属性配置")]
    public EffectTimeTypeConfig EffectTimeType;     // 效果持续时间类型
    public float EffectDuration;             // 效果持续时间，不是限时直接写0
    public PropertyModifyConfig ModifyConfig; // 属性修改配置
}

// 越界机动能力类型
public enum BoundaryOverrideType
{
    none,
    WallWalking,
    Flying,
    InertiaWalking
}

[Serializable]
public class PlayerBoundaryOverrideParameter
{
    public BoundaryOverrideType Type;
    public EffectTimeTypeConfig EffectTimeType;
    public float EffectDuration;
}


[Serializable]
public class PlayerEffectParameter
{
    [Header("只能选一个!!!")]

    [Header("玩家属性改变效果时间参数")]
    public ChangeAttributeParameter AttributeParameter;

    [Header("得到越界机动能力效果参数")]
    public PlayerBoundaryOverrideParameter BoundaryOverrideParameter;

    [Header("消耗金币数量")]
    public int ConsumeGoldAmount;

    [Header("稳定值每秒改变配置")]
    public ChangeStabilityPerSecondParameter ChangeStabilityParameter;
}

[Serializable]
public class CameraEffectParameter
{
    [Header("只能选一个!!!")]
    public ChangeAttributeParameter CameraViewSizeParameter;
    public PlayerXRayParameter XRayParameter;
}

[Serializable]
public class MapEffectParameter
{
    [Header("只能选一个!!!")]
    public String PlaceStructureId;
}

[Serializable]
public class RewardFacilityEffectParameter
{
    [Header("只能选一个!!!")]
    public List<TargetRewardFacilityConfig> RewardConfig;
    public List<UpgradeRewardFacilityConfig> UpgradeConfig;
    public RewardFacilityType ReceivedRewardFacility;
}

[Serializable]
public class PropEffectParameter
{
    [Header("只能选一个!!!")]
    public ChangeItemRemainingUsesParameter ChangeRemainingUsesParameter;
    public ChangeItemMaxUsesParameter ChangeMaxUsesParameter;
    public List<EchelonType> AllowedIncreaseCardType;
}

[Serializable]
public class LevelEffectParameter
{
    [Header("只能选一个!!!")]
    public int TEST;
}

[Serializable]
public class PlayerXRayParameter
{
    public EffectTimeTypeConfig EffectTimeType;
    public float EffectDuration;
}

/// <summary>
/// 效果持续时间类型配置：瞬时、限时、关卡永久
/// </summary>
public enum EffectTimeTypeConfig
{
    Instant,    // 瞬时
    Timed,      // 限时
    Permanent   // 关卡永久
}

/// <summary>
/// 属性修改类型
/// </summary>
public enum PropertyModifyType
{
    [Header("直接设置为固定值")]
    SetAbsolute,
    [Header("增加固定值")]
    AddFlat,
    [Header("减少固定值")]
    SubtractFlat,
    [Header("增加百分比")]
    AddPercent,
    [Header("减少百分比")]
    SubtractPercent
}

/// <summary>
/// 属性修改
/// </summary>
[Serializable]
public class PropertyModifyConfig
{
    [Header("修改模式")]
    public PropertyModifyType ModifyType;

    [Header("如果是set，是否要比当前值小才set")]
    public bool IsSetOnlyIfSmaller;

    [Header("如果是百分比，是基于当前值还是基础值")]
    public bool IsBasedOnCurrentValue;

    [Header("修改数值（固定值 / 百分比 0.1=10%）")]
    public float Value;

    [Header("效果持续时间类型，持续时间，属性配置")]
    public EffectTimeTypeConfig EffectTimeType;     // 效果持续时间类型
    public float EffectDuration;             // 效果持续时间，不是限时直接写0
}

[Serializable]
public class ChangeStabilityPerSecondParameter
{
    [Header("修改模式")]
    public PropertyModifyType ModifyType;

    [Header("增加或减少的绝对阈值大小")]
    public float StopThreshold;

    [Header("如果是百分比，是基于当前值还是基础值")]
    public bool IsBasedOnCurrentValue;

    [Header("修改数值（固定值 / 百分比 0.1=10%）")]
    public float Value;

}


/// <summary>
/// 道具可作用的奖励设施目标类型
/// </summary>
public enum RewardFacilityType
{
    None,
    Chest_Normal,
    Chest_Purple,
    Chest_Gold,
    VendingMachine_Normal,
    VendingMachine_Broken,
}

/// <summary>
/// 目标奖励设施配置
/// </summary>
[Serializable]
public class TargetRewardFacilityConfig
{
    [Header("目标类型")]
    public RewardFacilityType TargetType;
    [Header("正常效果是否必出最佳奖励")]
    public bool IsGuaranteeBestReward;
    [Header("bug触发概率（0-100）")]
    [Range(0, 100)] public float BugProbability;
    [Header("正常效果奖励倍数")]
    public float NormalRewardMultiplier;
    [Header("Bug效果奖励倍数")]
    public float BugRewardMultiplier;
    [Header("Bug时是否彻底损坏设施")]
    public bool IsBugDestroyFacility;
}

/// <summary>
/// 奖励设施对应可升级的奖励设施
/// </summary>
[Serializable]
public class UpgradeRewardFacilityConfig
{
    [Header("作用的奖励设施类型")]
    public RewardFacilityType SourceType;
    [Header("目标的奖励设施类型")]
    public RewardFacilityType TargetType;
}

[Serializable]
public class ChangeItemRemainingUsesParameter
{
    [Header("是否直接恢复到最大值")]
    public bool IsRestoredToMaxUses;
    [Header("是否清零当前剩余使用次数")]
    public bool IsClearCurrentRemainingUses;
    [Header("是增加还是减少")]
    public bool IsAddOrSubtract;
    [Header("剩余使用次数的变化值")]
    public int ChangeUses;
}

[Serializable]
public class ChangeItemMaxUsesParameter
{
    [Header("增大当前多少倍")]
    public int AddPercent;
}