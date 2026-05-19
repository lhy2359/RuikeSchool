using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "成就总配置", menuName = "游戏配置/成就总配置")]
public class AchievementConfigSO : ScriptableObject
{
    public List<AchievementData> AllAchievements = new List<AchievementData>();

    private static AchievementConfigSO _instance;
    public static AchievementConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<AchievementConfigSO>("成就总配置/成就总配置");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 根据ID获取成就静态配置
    /// </summary>
    public AchievementData GetAchievementById(string id)
    {
        return AllAchievements.Find(a => a.AchievementId == id);
    }
}