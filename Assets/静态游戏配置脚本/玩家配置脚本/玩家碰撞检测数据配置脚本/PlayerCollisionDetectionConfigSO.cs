using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "玩家碰撞检测数据配置", menuName = "游戏配置/玩家碰撞检测数据配置")]
public class PlayerCollisionDetectionConfigSO : ScriptableObject
{
    [Header("碰撞体长度")]
    public float playerColliderHalfWidth = 0.25f;

    [Header("碰撞体高度")]
    public float playerColliderHalfHeight = 0.45f;

    private static PlayerCollisionDetectionConfigSO _instance;
    public static PlayerCollisionDetectionConfigSO Instance
    {
        
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PlayerCollisionDetectionConfigSO>("玩家配置/玩家碰撞检测数据配置");
            }
            return _instance;
        }
    }
}
