using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "输入键位总配置", menuName = "游戏配置 /输入键位总配置")]

public class InputKeyConfigSO : ScriptableObject
{
    public KeyCode InventoryKey = KeyCode.R;
    public KeyCode CardKey = KeyCode.Q; 
    public KeyCode MedicineKey = KeyCode.E;

    private static InputKeyConfigSO _instance;
    public static InputKeyConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<InputKeyConfigSO>("输入键位总配置/输入键位总配置");
            }
            return _instance;
        }
    }
}
