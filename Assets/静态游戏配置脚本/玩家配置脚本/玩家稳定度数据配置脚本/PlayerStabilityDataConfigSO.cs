using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "玩家稳定度数据配置", menuName = "游戏配置/玩家稳定度数据配置")]
public class PlayerStabilityDataConfigSO : ScriptableObject
{
    [Header("最大投影稳定度")]
    public float MaxStability = 100f;

    [Header("稳定度下降速度")]
    public float StabilityReducePerMinute = 0.2f;

    [Header("低稳定度阈值")]
    public float LowStabilityThreshold = 40f;


    private static PlayerStabilityDataConfigSO _instance;
    public static PlayerStabilityDataConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<PlayerStabilityDataConfigSO>("玩家配置/玩家稳定度数据配置");
            }
            return _instance;
        }
    }
}