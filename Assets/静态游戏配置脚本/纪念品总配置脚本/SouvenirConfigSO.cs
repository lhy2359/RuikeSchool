using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "纪念品总配置", menuName = "游戏配置/纪念品总配置")]
public class SouvenirConfigSO : ScriptableObject
{
    public List<SouvenirData> AllSouvenirs;

    private static SouvenirConfigSO _instance;
    public static SouvenirConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<SouvenirConfigSO>("纪念品总配置/纪念品总配置");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 根据ID获取成就静态配置
    /// </summary>
    public SouvenirData GetSouvenirDataById(string id)
    {
        return AllSouvenirs.Find(a => a.SouvenirId == id);
    }
}