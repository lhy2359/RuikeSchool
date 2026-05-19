using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "背包总配置", menuName = "游戏配置/背包总配置")]

public class InventoryConfigSO : ScriptableObject
{
    [Header("物品：总格子数，快捷栏格子数")]
    public int ItemTotalGridCount;
    public int ItemQuickBarCount;

    [Header("卡牌：堆叠上限，格子数")]
    public int CardMaxStack;
    public int CardGridCount;

    [Header("纪念品：堆叠上限，格子数")]
    public int SouvenirMaxStack;
    public int SouvenirGridCount;

    [Header("药品：堆叠上限，格子数")]
    public int MedicineMaxStack;
    public int MedicineGridCount;

    private static InventoryConfigSO _instance;
    public static InventoryConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<InventoryConfigSO>("背包总配置/背包总配置");
            }
            return _instance;
        }
    }
}
