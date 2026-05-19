using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "药品总配置", menuName = "游戏配置 /药品总配置")]

public class MedicineConfigSO : ScriptableObject
{
    public List<MedicineData> MedicineDataList;

    private static MedicineConfigSO _instance;
    public static MedicineConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<MedicineConfigSO>("药品总配置/药品总配置");
            }
            return _instance;
        }
    }

    // 根据ID获取物品配置
    public MedicineData GetMedicineDataById(string id)
    {
        return MedicineDataList.Find(medicine => medicine.PropId == id);
    }
}

