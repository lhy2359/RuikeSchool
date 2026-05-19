using System;
using UnityEngine;

/// <summary>
/// 运行时成就数据
/// </summary>
[Serializable]
public class RuntimeAchievementData
{
    [SerializeField] private AchievementData _config;
    [SerializeField] private string _achievementId;
    [SerializeField] private bool _isUnlocked;
    [SerializeField] private float _currentProgress;
    [SerializeField] private bool _isCareerMatched;

    public AchievementData Config => _config;
    public string AchievementId => _achievementId;
    public bool IsUnlocked => _isUnlocked;
    public float CurrentProgress => _currentProgress;
    public bool IsCareerMatched => _isCareerMatched;

    public RuntimeAchievementData(string achievementId, bool isUnlocked, float currentProgress, bool isCareerMatched)
    {
        _achievementId = achievementId;
        _isUnlocked = isUnlocked;
        _currentProgress = currentProgress;
        _isCareerMatched = isCareerMatched;
        _config = null;
    }

    public void SetIsUnlocked(bool value = true) => _isUnlocked = value;
    public void SetCurrentProgress(float value) => _currentProgress = value;
    public void SetIsCareerMatched(bool value) => _isCareerMatched = value;

    // 通过ID从SO配置表中绑定静态数据
    public void BindConfig()
    {
        // 空ID直接返回
        if (string.IsNullOrEmpty(_achievementId))
        {
            Debug.LogError("成就ID为空，无法绑定配置！");
            return;
        }

        // 调用SO单例，自动获取配置
        _config = AchievementConfigSO.Instance.GetAchievementById(_achievementId);

        if (_config == null)
        {
            Debug.LogError($"未找到ID为 {_achievementId} 的成就配置！");
        }
    }

    // 存档前置空引用
    public void PrepareForSave()
    {
        // 清空静态配置引用
        _config = null;
    }
}