using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 玩家基础数据系统
/// </summary>
public class PlayerBasicDataSystem : MonoBehaviour
{
    public static PlayerBasicDataSystem Instance;

    // 基础数据
    [SerializeField] private ProfessionType _playerProfessionType;
    [SerializeField] private int _currentGoldCoin;

    [Header("基础属性")]
    [SerializeField] private float _baseMoveSpeed;
    [SerializeField] private float _baseViewRange;

    [Header("低稳定状态骤降值")]
    [SerializeField] private float _lowStabilityMoveSpeedDrop;
    [SerializeField] private float _lowStabilityViewRangeDrop;

    [Header("最终属性")]
    [SerializeField] private float _currentMoveSpeed;
    [SerializeField] private float _currentViewRange;

    // 公开属性
    public float CurrentMoveSpeed => _currentMoveSpeed;
    public float CurrentViewRange => _currentViewRange;
    public int CurrentGoldCoin => _currentGoldCoin;
    public ProfessionType PlayerProfessionType => _playerProfessionType;
    public IReadOnlyList<RuntimeEffect> ActiveEffects => _activeEffects.AsReadOnly();

    private List<RuntimeEffect> _activeEffects = new List<RuntimeEffect>();

    // 事件
    public event Action OnGoldChanged;
    public event Action OnStatsChanged;
    public event Action OnPlayerProfessionTypeChanged;
    public event Action OnEffectsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitNewGameBasicData();
    }

    private void Update()
    {
        // 完全对齐状态系统执行顺序
        UpdateEffectTimer();
        CalculateFinalAttributes();
    }

    // 初始化
    private void InitNewGameBasicData()
    {
        _currentGoldCoin = 0;

        // 配置表读取基础值和骤降值
        var config = PlayerBasicDataConfigSO.Instance;
        _baseMoveSpeed = config.BaseMoveSpeed;
        _baseViewRange = config.BaseViewRange;
        _lowStabilityMoveSpeedDrop = config.LowStabilityMoveSpeedDrop;
        _lowStabilityViewRangeDrop = config.LowStabilityViewRangeDrop;

        _playerProfessionType = ProfessionType.None;
        _activeEffects.Clear();
        Debug.Log(_baseMoveSpeed);

    }

    // 最终属性计算
    private void CalculateFinalAttributes()
    {
        // 初始基础值
        float finalMoveSpeed = _baseMoveSpeed;
        float finalViewRange = _baseViewRange;

        // 低稳定：直接赋值骤降值（无最小值限制）
        if (PlayerStatusSystem.Instance != null)
        {
            bool isLowStability = PlayerStatusSystem.Instance.CurrentStability <= PlayerStatusSystem.Instance.LowStabilityThreshold
                                 && PlayerStatusSystem.Instance.CurrentStability > 0;

            if (isLowStability)
            {
                finalMoveSpeed = _lowStabilityMoveSpeedDrop;
                finalViewRange = _lowStabilityViewRangeDrop;
            }
        }

        // 优先级排序所有Buff
        var sortedBuffs = _activeEffects.OrderBy(e => e.Priority).ToList();

        // 叠加所有持续效果
        foreach (RuntimeEffect effect in sortedBuffs)
        {
            if (effect.EffectConfig.Target != EffectTarget.Player) continue;
            var playerParams = effect.EffectConfig.PlayerParameters;

            if (playerParams == null || playerParams.AttributeParameter == null) continue;
            var modifier = playerParams.AttributeParameter.ModifyConfig;

            // 移速效果
            if (effect.EffectConfig.PlayerEffect == PlayerEffectType.ChangeMoveSpeed)
            {
                ApplyModifier(ref finalMoveSpeed, modifier, _baseMoveSpeed);
            }

            // 视野效果
            if (effect.EffectConfig.PlayerEffect == PlayerEffectType.ChangeVisionRadius)
            {
                ApplyModifier(ref finalViewRange, modifier, _baseViewRange);
            }
        }

        // 最终赋值
        _currentMoveSpeed = finalMoveSpeed;
        _currentViewRange = finalViewRange;
        OnStatsChanged?.Invoke();
    }

    // 效果计时器（和状态系统完全一致）
    private void UpdateEffectTimer()
    {
        bool effectChanged = false;
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            RuntimeEffect effect = _activeEffects[i];
            if (effect.IsPermanent) continue;

            effect.RemainingDuration -= Time.deltaTime;
            if (effect.RemainingDuration <= 0)
            {
                _activeEffects.RemoveAt(i);
                effectChanged = true;
            }
        }

        if (effectChanged)
        {
            OnEffectsChanged?.Invoke();
            CalculateFinalAttributes();
        }
    }

    // 添加效果
    public void AddEffect(EffectUnit effectUnit)
    {
        // 瞬时效果：直接应用，不进列表
        if (effectUnit.PlayerParameters != null && effectUnit.PlayerParameters.AttributeParameter != null &&
            effectUnit.PlayerParameters.AttributeParameter.EffectTimeType == EffectTimeTypeConfig.Instant)
        {
            ApplyInstantEffect(effectUnit);
            return;
        }

        // 持续效果：创建并设置优先级
        var runtimeBuff = new RuntimeEffect(effectUnit);
        if (effectUnit.PlayerParameters.AttributeParameter != null)
        {
            bool isCurrentBased = effectUnit.PlayerParameters.AttributeParameter.ModifyConfig.IsBasedOnCurrentValue;
            runtimeBuff.Priority = isCurrentBased ? EffectPriority.BasedOnCurrentValue : EffectPriority.BasedOnBaseValue;
        }
        else
        {
            Debug.Log("zzzzzz");
        }

        _activeEffects.Add(runtimeBuff);
        OnEffectsChanged?.Invoke();
        CalculateFinalAttributes();
    }

    // 瞬时效果
    private void ApplyInstantEffect(EffectUnit effectUnit)
    {
        if (effectUnit.Target != EffectTarget.Player) return;
        var playerParams = effectUnit.PlayerParameters;

        if (playerParams == null || playerParams.AttributeParameter == null) return;
        var modifier = playerParams.AttributeParameter.ModifyConfig;

        switch (effectUnit.PlayerEffect)
        {
            case PlayerEffectType.RemoveAllDebuffs:
                RemoveAllDebuffs();
                break;

            case PlayerEffectType.ChangeMoveSpeed:
                ApplyModifier(ref _currentMoveSpeed, modifier, _baseMoveSpeed);
                break;

            case PlayerEffectType.ChangeVisionRadius:
                ApplyModifier(ref _currentViewRange, modifier, _baseViewRange);
                break;
        }

        OnStatsChanged?.Invoke();
        CalculateFinalAttributes();
    }

    #region 通用方法
    public void RemoveEffects(EffectPolarity? targetPolarity = null, bool includePermanent = true)
    {
        int removedCount = _activeEffects.RemoveAll(effect =>
        {
            if (!includePermanent && effect.IsPermanent) return false;
            if (targetPolarity.HasValue && effect.EffectConfig.Polarity != targetPolarity.Value) return false;
            return true;
        });

        if (removedCount > 0)
        {
            OnEffectsChanged?.Invoke();
            CalculateFinalAttributes();
        }
    }

    public void RemoveAllDebuffs() => RemoveEffects(EffectPolarity.Debuff);
    public void RemoveAllBuffs() => RemoveEffects(EffectPolarity.Buff);
    public void RemoveAllEffects() => RemoveEffects();

    // 统一计算
    private void ApplyModifier(ref float currentValue, PropertyModifyConfig modifier, float baseValue)
    {
        float calculateBase = modifier.IsBasedOnCurrentValue ? currentValue : baseValue;

        switch (modifier.ModifyType)
        {
            case PropertyModifyType.SetAbsolute:
                if (modifier.IsSetOnlyIfSmaller && currentValue <= modifier.Value) return;
                currentValue = modifier.Value;
                break;
            case PropertyModifyType.AddFlat:
                currentValue += modifier.Value;
                break;
            case PropertyModifyType.SubtractFlat:
                currentValue -= modifier.Value;
                break;
            case PropertyModifyType.AddPercent:
                currentValue += calculateBase * modifier.Value;
                break;
            case PropertyModifyType.SubtractPercent:
                currentValue -= calculateBase * modifier.Value;
                break;
        }
    }
    #endregion

    #region 外部设置
    public void SetPlayerProfessionType(ProfessionType type)
    {
        _playerProfessionType = type;
        OnPlayerProfessionTypeChanged?.Invoke();
    }

    public void SetGoldCoin(int value)
    {
        _currentGoldCoin = value;
        OnGoldChanged?.Invoke();
    }
    #endregion
}