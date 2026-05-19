using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时数据
/// </summary>
[Serializable]
public class RuntimeSouvenirData : IInventoryProp
{
    [SerializeField] private SouvenirData _config;
    [SerializeField] private string _souvenirId;
    [SerializeField] private int _souvenirNum;
    [SerializeField] private int _gridIndex;

    public Sprite GetIcon() => _config?.Icon;
    public string GetPropID() => _souvenirId;
    public string GetDescription() => _config?.Description;
    public int GetPropCount() => _souvenirNum;

    public SouvenirData Config => _config;
    public string SouvenirId => _souvenirId;
    public int SouvenirNum => _souvenirNum;
    public int GridIndex => _gridIndex;


    
    public void SetSouvenirNum(int value) => _souvenirNum = value;
    public void SetGridIndex(int value) => _gridIndex = value;


    public RuntimeSouvenirData(string souvenirId, int souvenirNum, int gridIndex)
    {
        _souvenirId = souvenirId;
        _souvenirNum = souvenirNum;
        _gridIndex = gridIndex;
    }

    /// 通过ID从SO配置表中绑定静态数据
    public void BindConfig()
    {
        if (string.IsNullOrEmpty(_souvenirId))
        {
            Debug.LogError("成就ID为空，无法绑定配置！");
            return;
        }

        _config = SouvenirConfigSO.Instance.GetSouvenirDataById(_souvenirId);

        if (_config == null)
        {
            Debug.LogError($"未找到ID为 {_souvenirId}的纪念品配置！");
        }
    }

    /// 存档前置空引用
    public void PrepareForSave()
    {
        // 清空静态配置引用
        _config = null;
    }
}