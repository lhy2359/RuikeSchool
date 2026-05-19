using System.Collections.Generic;
using UnityEngine;
using System;


public class AchievementManageSystem : MonoBehaviour
{
    public static AchievementManageSystem Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private Dictionary<string, RuntimeAchievementData> _runtimeDict = new();

    // 新游戏初始化
    public void InitNewGameAchievements(ProfessionType playerProfession)
    {
        _runtimeDict.Clear();
        foreach (var staticData in AchievementConfigSO.Instance.AllAchievements)
        {
            RuntimeAchievementData data = new RuntimeAchievementData(
                staticData.AchievementId,
                false,
                0,
                (playerProfession == staticData.PlayerProfessionType)
            );
            data.BindConfig();
            _runtimeDict.Add(staticData.AchievementId, data);
        }
        Debug.Log($"新游戏成就初始化 | 职业：{playerProfession}");
    }

    // 加载存档
    public void LoadAchievementsFromSaveData(List<RuntimeAchievementData> saveData)
    {
        _runtimeDict.Clear();
        foreach (var data in saveData)
        {
            data.BindConfig();
            _runtimeDict[data.AchievementId] = data;
        }
        Debug.Log("存档成就加载完成");
    }

    // 保存数据
    public List<RuntimeAchievementData> GetCurrentAchievementsForSave()
    {
        foreach (var data in _runtimeDict.Values)
        {
            data.PrepareForSave();
        }
        return new List<RuntimeAchievementData>(_runtimeDict.Values);
    }

    // 获取成就静态配置数据
    public AchievementData GetAchievementData(string achievementId)
    {
        return AchievementConfigSO.Instance.GetAchievementById(achievementId);
    }

    // 获取成就运行时数据
    public RuntimeAchievementData GetRuntimeAchievementData(string achievementId)
    {
        _runtimeDict.TryGetValue(achievementId, out var data);
        return data;
    }

    // 解锁成就
    public void UnlockAchievement(string achievementId)
    {
        var data = GetRuntimeAchievementData(achievementId);
        if (data != null)
        {
            data.SetIsUnlocked(true);
        }
    }

    // 更新成就进度
    public void UpdateAchievementProgress(string achievementId, float value)
    {
        var data = GetRuntimeAchievementData(achievementId);
        if (data != null)
        {
            data.SetCurrentProgress(value);
        }
    }
}