using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "平原关卡配置", menuName = "游戏配置/平原关卡配置")]
public class PlainLevelConfigSO : ScriptableObject
{
    [Header("普通宝箱数量")]
    public int NormalChestCount = 40;
    [Header("紫色宝箱数量")]
    public int PurpleChestCount = 10;
    [Header("金色宝箱数量")]
    public int GoldenChestCount = 1;

    [Header("普通自动贩卖机数量")]
    public int NormalVendingMachineCount = 3;
    [Header("损坏自动贩卖机数量")]
    public int BrokenVendingMachineCount = 2;

    [Header("终焉参差数量")]
    public int FinisAsymmetricLevelEventCount = 2;
    [Header("记忆参差数量")]
    public int MnemosAsymmetricLevelEventCount = 2;
    [Header("现世参差数量")]
    public int MundusAsymmetricLevelEventCount = 5;

    [Header("商店数量")]
    public int StoreCount = 2;
    [Header("修理店数量")]
    public int RepairShopCount = 2;
    [Header("药店数量")]
    public int PharmacyShopCount = 2;


    private static PlainLevelConfigSO _instance;
    public static PlainLevelConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<PlainLevelConfigSO>("关卡总配置/平原关卡配置");
            }
            return _instance;
        }
    }
}

