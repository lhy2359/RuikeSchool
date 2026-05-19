using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 效果先算特殊优先级，再算基于当前值的优先级，最后算基于基础值的优先级
public enum EffectPriority
{
    BasedOnBaseValue = 1,
    BasedOnCurrentValue = 2,
    Special = 99
}

[Serializable]
public class RuntimeEffect
{
    public string PropId; // 对应的道具的PropId
    public EffectPriority Priority; // 效果优先级
    public EffectUnit EffectConfig;
    public float RemainingDuration;
    public bool IsPermanent;

    public RuntimeEffect(EffectUnit config)
    {
        EffectConfig = config;
    }
}
