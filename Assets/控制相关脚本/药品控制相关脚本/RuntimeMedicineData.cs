using System;
using UnityEngine;

/// <summary>
/// 运行时药品数据
/// </summary>
[Serializable]
public class RuntimeMedicineData : IInventoryProp
{
    [SerializeField] private MedicineData _config;
    [SerializeField] private string _medicineId; // 药品唯一ID（对应静态配置表）
    [SerializeField] private int _medicineNum; // 药品堆叠数量
    [SerializeField] private int _gridIndex; // 格子索引（用于背包）
    [SerializeField] private MedicineLevel _level; // 药水等级

    // 实现接口
     public Sprite GetIcon() => _config?.Icon;
     public string GetPropID() => _medicineId;
     public string GetDescription() => _config?.Description;
     public int GetPropCount() => _medicineNum;

    public MedicineData Config => _config;
    public string MedicineId => _medicineId;
    public int MedicineNum => _medicineNum;
    public int GridIndex => _gridIndex;
    public MedicineLevel Level => _level;

    // 设置药品堆叠数量
    public void SetMedicineNum(int value)
    {
        _medicineNum = value;
    }

    // 设置格子索引
    public void SetGridIndex(int value)
    {
        _gridIndex = value;
    }
    public void SetLevel(MedicineLevel level)
    {
        _level = level;
    }

    public RuntimeMedicineData(string medicineId, int medicineNum, int gridIndex, MedicineLevel level)
    {
        _medicineId = medicineId;
        _medicineNum = medicineNum;
        _gridIndex = gridIndex;
        _level = level;
    }

    /// 通过ID从SO配置表中绑定静态数据
    public void BindConfig()
    {
        // 空ID直接返回
        if (string.IsNullOrEmpty(_medicineId))
        {
            Debug.LogError("药品ID为空，无法绑定配置！");
            return;
        }

        // 调用SO单例，自动获取配置
        _config = MedicineConfigSO.Instance.GetMedicineDataById(_medicineId);

        if (_config == null)
        {
            Debug.LogError($"未找到ID为 {_medicineId}的药品配置！");
        }
    }

    /// 存档前置空引用
    public void PrepareForSave()
    {
        // 清空静态配置引用
        _config = null;
    }
}