using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "玩家基础数据配置", menuName = "游戏配置/玩家基础数据配置")]
public class PlayerBasicDataConfigSO : ScriptableObject
{
    [Header("玩家基础移动速度")]
    public float BaseMoveSpeed = 3f;

    [Header("玩家基础视野范围")]
    public float BaseViewRange = 7f;

    [Header("玩家基础金币数")]
    public int BaseGoldCoin = 0;

    [Header("低稳定阈值以下时，玩家移速/视野的骤降值")]
    public float LowStabilityMoveSpeedDrop = 0.5f;

    public float LowStabilityViewRangeDrop = 0.5f;

    private static PlayerBasicDataConfigSO _instance;
    public static PlayerBasicDataConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<PlayerBasicDataConfigSO>("玩家配置/玩家基础数据配置");
            }
            return _instance;
        }
    }
}