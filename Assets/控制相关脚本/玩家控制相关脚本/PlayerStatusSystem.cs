using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 玩家状态系统：稳定度管理
/// </summary>
public class PlayerStatusSystem : MonoBehaviour
{
    public static PlayerStatusSystem Instance;

    [Header("稳定度配置")]
    private float _baseMaxStability;

    [Header("当前状态")]
    private float _currentStability;
    private float _currentMaxStability;

    [Header("阈值配置")]
    private float _lowStabilityThreshold;

    [Header("系统开关")]
    private bool _isSystemStabilityDecayActive;

    // 固定衰减：每秒稳定度下降值（由每分钟衰减计算而来）
    private float _stabilityReducePerSecond;

    // 效果列表
    private List<RuntimeEffect> _activeEffects = new List<RuntimeEffect>();

    public float MaxStability => _currentMaxStability;
    public float CurrentStability => _currentStability;
    public float LowStabilityThreshold => _lowStabilityThreshold;
    public bool IsSystemStabilityDecayActive => _isSystemStabilityDecayActive;
    public IReadOnlyList<RuntimeEffect> ActiveEffects => _activeEffects.AsReadOnly();

    // 事件
    public event Action OnStabilityChanged;
    public event Action OnStabilityEmpty;
    public event Action OnEffectsChanged;

    // 低稳定状态 进入/退出
    public event Action OnEnterLowStability;
    public event Action OnExitLowStability;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitNewGameStatus();
    }

    private void Update()
    {
        // 更新效果计时器
        UpdateEffectTimer();

        // 最高优先级执行系统基础稳定度衰减
        if (_isSystemStabilityDecayActive) ProcessStabilityDecay();

        // 执行每秒稳定值变化效果
        ProcessStabilityPerSecond();

        // 按优先级执行效果（基础值→当前值→特殊）
        CalculateFinalStability();
    }

    private void InitNewGameStatus()
    {
        var config = PlayerStabilityDataConfigSO.Instance;
        _baseMaxStability = config.MaxStability;
        _lowStabilityThreshold = config.LowStabilityThreshold;
        _isSystemStabilityDecayActive = false;

        // 计算：每秒固定衰减值
        _stabilityReducePerSecond = config.StabilityReducePerMinute / 60f;

        _currentStability = _baseMaxStability;
        _currentMaxStability = _baseMaxStability;
        _activeEffects.Clear();
    }

    private void CalculateFinalStability()
    {
        float finalMaxStability = _baseMaxStability;
        float finalCurrentStability = _currentStability;

        // 按效果优先级排序（基础值 → 当前值 → 特殊）
        var sortedEffects = _activeEffects.OrderBy(e => e.Priority).ToList();

        foreach (var effect in sortedEffects)
        {
            // 仅处理玩家稳定度相关效果
            if (effect.EffectConfig.Target == EffectTarget.Player)
            {
                var param = effect.EffectConfig.PlayerParameters;
                if (param == null || param.AttributeParameter == null) continue;

                if (effect.EffectConfig.PlayerEffect == PlayerEffectType.ChangeMaxStability)
                    ApplyModifier(ref finalMaxStability, param.AttributeParameter.ModifyConfig, _baseMaxStability);

                // 当前稳定值修改仅为瞬时，不在此处处理
            }
        }

        _currentMaxStability = finalMaxStability;
        _currentStability = Mathf.Clamp(finalCurrentStability, 0, _currentMaxStability);
        OnStabilityChanged?.Invoke();
    }

    private void UpdateEffectTimer()
    {
        bool changed = false;
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var e = _activeEffects[i];
            if (e.IsPermanent) continue;

            e.RemainingDuration -= Time.deltaTime;
            if (e.RemainingDuration <= 0)
            {
                _activeEffects.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) OnEffectsChanged?.Invoke();
    }

    // 每秒稳定值变化效果
    private void ProcessStabilityPerSecond()
    {
        if (_currentStability <= 0) return;

        foreach (var effect in _activeEffects)
        {
            if (effect.EffectConfig.PlayerEffect != PlayerEffectType.ChangeStabilityPerSecond)
                continue;

            var param = effect.EffectConfig.PlayerParameters.ChangeStabilityParameter;
            if (param == null) continue;

            // 固定数值判断：小于等于阈值直接停止
            if (_currentStability <= param.StopThreshold)
                continue;

            float calculateBase = param.IsBasedOnCurrentValue ? _currentStability : _baseMaxStability;
            float changeValue = 0;

            switch (param.ModifyType)
            {
                case PropertyModifyType.AddFlat:
                    changeValue = param.Value;
                    break;
                case PropertyModifyType.SubtractFlat:
                    changeValue = -param.Value;
                    break;
                case PropertyModifyType.AddPercent:
                    changeValue = calculateBase * param.Value;
                    break;
                case PropertyModifyType.SubtractPercent:
                    changeValue = -calculateBase * param.Value;
                    break;
                default:
                    continue; 
            }

            // 应用每秒变化，最低不低于0
            _currentStability = Mathf.Max(0, _currentStability + changeValue * Time.deltaTime);
        }

        OnStabilityChanged?.Invoke();
    }

    private void ProcessStabilityDecay()
    {
        if (!_isSystemStabilityDecayActive) return;

        if (_currentStability > 0)
        {
            _currentStability = Mathf.Max(0, _currentStability - _stabilityReducePerSecond * Time.deltaTime);
            OnStabilityChanged?.Invoke();
        }

        // 阈值判断并发送事件
        bool isLow = _currentStability <= _lowStabilityThreshold && _currentStability > 0;
        if (isLow) OnEnterLowStability?.Invoke();
        else OnExitLowStability?.Invoke();

        if (_currentStability <= 0)
        {
            OnExitLowStability?.Invoke();
            OnStabilityEmpty?.Invoke();
        }
    }

    public void AddEffect(EffectUnit effectUnit)
    {
        // 检查是否为瞬时效果并拆分参数
        if (effectUnit.PlayerParameters != null && effectUnit.PlayerParameters.AttributeParameter != null &&
            effectUnit.PlayerParameters.AttributeParameter.EffectTimeType == EffectTimeTypeConfig.Instant)
        {
            ApplyInstantEffect(effectUnit);
            return;
        }

        // 创建效果实例
        var runtimeEffect = new RuntimeEffect(effectUnit);

        // 自动设置效果优先级
        if (effectUnit.PlayerParameters.AttributeParameter != null)
        {
            bool isCurrentBased = effectUnit.PlayerParameters.AttributeParameter.ModifyConfig.IsBasedOnCurrentValue;
            runtimeEffect.Priority = isCurrentBased ? EffectPriority.BasedOnCurrentValue : EffectPriority.BasedOnBaseValue;
        }
        else
        {
            Debug.Log("zzzzzz");
        }

        _activeEffects.Add(runtimeEffect);
        OnEffectsChanged?.Invoke();
    }

    public void RemoveEffects(EffectPolarity? targetPolarity = null, bool includePermanent = true)
    {
        int removedCount = _activeEffects.RemoveAll(effect =>
        {
            if (!includePermanent && effect.IsPermanent) return false;
            if (targetPolarity.HasValue && effect.EffectConfig.Polarity != targetPolarity.Value) return false;
            return true;
        });
        if (removedCount > 0) OnEffectsChanged?.Invoke();
    }

    public void RemoveAllDebuffs() => RemoveEffects(EffectPolarity.Debuff);
    public void RemoveAllBuffs() => RemoveEffects(EffectPolarity.Buff);
    public void RemoveAllEffects() => RemoveEffects();

    public void RestoreStability(float value)
    {
        _currentStability = Mathf.Min(_currentMaxStability, _currentStability + value);
        OnStabilityChanged?.Invoke();
    }

    public void SetStability(float value)
    {
        _currentStability = Mathf.Clamp(value, 0, _currentMaxStability);
        OnStabilityChanged?.Invoke();
    }

    public void SetSystemActive(bool active) => _isSystemStabilityDecayActive = active;
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

    private void ApplyInstantEffect(EffectUnit effectUnit)
    {
        if (effectUnit.Target == EffectTarget.Player)
        {
            if (effectUnit.PlayerEffect == PlayerEffectType.ChangeCurrentStability)
            {
                if (effectUnit.PlayerParameters != null && effectUnit.PlayerParameters.AttributeParameter != null)
                {
                    ApplyModifier(ref _currentStability, effectUnit.PlayerParameters.AttributeParameter.ModifyConfig, _baseMaxStability);
                    _currentStability = Mathf.Clamp(_currentStability, 0, _currentMaxStability);
                    OnStabilityChanged?.Invoke();
                }
            }
        }
    }

    public void LoadStatusFromSaveData(float lowStabilityThreshold, float stabilityReducePerMinute)
    {
        _lowStabilityThreshold = lowStabilityThreshold;
        _stabilityReducePerSecond = stabilityReducePerMinute / 60f;
    }

    public (float lowStabilityThreshold, float stabilityReducePerMinute) GetCurrentStatusForSave()
    {
        var config = PlayerStabilityDataConfigSO.Instance;
        return (_lowStabilityThreshold, config.StabilityReducePerMinute);
    }
}