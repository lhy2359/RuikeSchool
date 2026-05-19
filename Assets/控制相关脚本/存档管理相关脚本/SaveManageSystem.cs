using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class PlayerSaveData
{
    // 玩家基础数据
    [Header("存档槽编号")]
    private int _saveSlotId;

    [Header("玩家职业")]
    private ProfessionType _playerProfession;

    [Header("玩家金币数")]
    private int _goldCoin;

    // 玩家成就数据
    [Header("该存档的成就数据")]
    private List<RuntimeAchievementData> _saveAchievementData = new List<RuntimeAchievementData>();

    // 玩家背包物品数据
    [Header("该存档的背包物品数据")]
    private List<RuntimeItemData> _saveInventoryItemData = new List<RuntimeItemData>();

    [Header("该存档的背包物品总格子数")]
    private int _saveItemTotalGridCount;

    [Header("该存档的背包物品快捷栏格子数")]
    private int _saveQuickBarCount;

    // 玩家背包纪念品数据
    [Header("该存档的背包纪念品数据")]
    private List<RuntimeSouvenirData> _saveInventorySouvenirData = new List<RuntimeSouvenirData>();

    [Header("该存档的纪念品最大堆叠数")]
    private int _saveSouvenirMaxStack;

    [Header("该存档的纪念品格子数")]
    private int _saveSouvenirGridCount;

    // 玩家背包药品数据
    [Header("该存档的背包药品数据")]
    private List<RuntimeMedicineData> _saveInventoryMedicineData = new List<RuntimeMedicineData>();

    [Header("该存档的药品最大堆叠数")]
    private int _saveMedicineMaxStack;

    [Header("该存档的药品格子数")]
    private int _saveMedicineGridCount;


    // 玩家状态数据
    [Header("当前低稳定度阈值")]
    private float _lowStabilityThreshold;

    [Header("当前稳定度下降速度")]
    private float _stabilityReducePerMinute;

    [Header("当前低于一定阈值时玩家的移动速度和视野范围骤降值")]
    private float _minMoveSpeed;
    private float _minViewRange;


    public int SaveSlotId => _saveSlotId;

    public ProfessionType PlayerProfession => _playerProfession;
    public int GoldCoin => _goldCoin;

    public List<RuntimeAchievementData> AchievementData => _saveAchievementData;

    public int ItemTotalGridCount => _saveItemTotalGridCount;
    public int QuickBarCount => _saveQuickBarCount;
    public List<RuntimeItemData> InventoryItemData => _saveInventoryItemData;

    public int SouvenirMaxStack => _saveSouvenirMaxStack;
    public int SouvenirGridCount => _saveSouvenirGridCount;
    public List<RuntimeSouvenirData> InventorySouvenirData => _saveInventorySouvenirData;

    public int MedicineMaxStack => _saveMedicineMaxStack;
    public int MedicineGridCount => _saveMedicineGridCount;
    public List<RuntimeMedicineData> InventoryMedicineData => _saveInventoryMedicineData;
    
    public float LowStabilityThreshold => _lowStabilityThreshold;
    public float StabilityReducePerMinute => _stabilityReducePerMinute;
    public float MinMoveSpeed => _minMoveSpeed;

    public float MinViewRange => _minViewRange;


    public void SetSaveSlotId(int id) => _saveSlotId = id;
    public void SetProfession(ProfessionType type) => _playerProfession = type;
    public void SetGoldCoin(int value) => _goldCoin = value;

    public void SetInventoryItemData(
        List<RuntimeItemData> items,
        int totalGrid,
        int quickBar)
    {
        _saveInventoryItemData = items;
        _saveItemTotalGridCount = totalGrid;
        _saveQuickBarCount = quickBar;
    }

    public void SetInventorySouvenirData(
        List<RuntimeSouvenirData> souvenirs,
        int maxStack,
        int gridCount)
    {
        _saveInventorySouvenirData = souvenirs;
        _saveSouvenirMaxStack = maxStack;
        _saveSouvenirGridCount = gridCount;
    }

    public void SetAchievementData(List<RuntimeAchievementData> data)
    {
        _saveAchievementData = data;
    }
    public void SetPlayerStatusData(float lowStabilityThreshold, float stabilityReducePerMinute)
    {
        _lowStabilityThreshold = lowStabilityThreshold;
        _stabilityReducePerMinute = stabilityReducePerMinute;
    }

    public void SetInventoryMedicineData(
        List<RuntimeMedicineData> medicines,
        int maxStack,
        int gridCount)
    {
        _saveInventoryMedicineData = medicines;
        _saveMedicineMaxStack = maxStack;
        _saveMedicineGridCount = gridCount;
    }

}

public class SaveManageSystem : MonoBehaviour
{
    public static SaveManageSystem Instance;

    // 固定10个存档槽，不可修改
    private const int MAX_SAVE_SLOT = 10;
    // 统一存储10个存档，null=该槽无存档
    private List<PlayerSaveData> _allSaveSlots = new List<PlayerSaveData>();
    // 存档文件路径（一个文件存全部10个档）
    private string _saveFilePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化存档路径
        _saveFilePath = Path.Combine(Application.persistentDataPath, "GameSaveSlots.json");
        // 初始化10个槽位，默认全空
        InitSaveSlotList();
        // 启动时加载本地所有存档
        LoadAllSaveFromDisk();
    }

    // 初始化10个存档槽位
    private void InitSaveSlotList()
    {
        _allSaveSlots.Clear();
        for (int i = 0; i < MAX_SAVE_SLOT; i++)
        {
            _allSaveSlots.Add(null);
        }
    }

    // 将当前游戏数据保存到第N个存档槽（仅在游戏内指定存档点调用）
    public void SaveToSlot(int slotIndex)
    {
        // 校验槽位合法性
        if (!IsSlotValid(slotIndex)) return;

        // 创建/覆盖该槽存档数据
        PlayerSaveData saveData = new PlayerSaveData();
        saveData.SetSaveSlotId(slotIndex);
        // 收集全游戏数据
        CollectGameDataToSave(saveData);
        // 存入列表
        _allSaveSlots[slotIndex] = saveData;
        // 写入本地文件
        WriteAllSaveToDisk();

        Debug.Log($"已手动保存至第 {slotIndex} 号存档");
    }

    // 从第N个存档槽读取数据到游戏
    public void LoadFromSlot(int slotIndex)
    {
        if (!IsSlotValid(slotIndex) || !HasSaveInSlot(slotIndex))
        {
            Debug.LogWarning($"第 {slotIndex} 号存档不存在或无效");
            return;
        }

        // 将存档数据应用到游戏各系统
        ApplySaveDataToGame(_allSaveSlots[slotIndex]);
        Debug.Log($"已加载第 {slotIndex} 号存档");
    }

    // 删除第N个存档槽
    public void DeleteSlot(int slotIndex)
    {
        if (!IsSlotValid(slotIndex)) return;

        _allSaveSlots[slotIndex] = null;
        WriteAllSaveToDisk();
        Debug.Log($"已删除第 {slotIndex} 号存档");
    }

    // 判断第N个槽是否有存档
    public bool HasSaveInSlot(int slotIndex)
    {
        if (!IsSlotValid(slotIndex)) return false;
        return _allSaveSlots[slotIndex] != null;
    }

    // 收集当前游戏所有数据到存档对象
    private void CollectGameDataToSave(PlayerSaveData saveData)
    {
        saveData.SetGoldCoin(PlayerBasicDataSystem.Instance.CurrentGoldCoin);
        saveData.SetProfession(PlayerBasicDataSystem.Instance.PlayerProfessionType);

        var (itemList, totalGrid, quickBar) = ItemManageSystem.Instance.GetCurrentItemsForSave();
        saveData.SetInventoryItemData(itemList, totalGrid, quickBar);

        var (souvenirList, souvenirMaxStack, souvenirGrid) = SouvenirManageSystem.Instance.GetCurrentSouvenirsForSave();
        saveData.SetInventorySouvenirData(souvenirList, souvenirMaxStack, souvenirGrid);

        var achievementList = AchievementManageSystem.Instance.GetCurrentAchievementsForSave();
        saveData.SetAchievementData(achievementList);

        var (lowStabilityThreshold, stabilityReducePerMinute) = PlayerStatusSystem.Instance.GetCurrentStatusForSave();
        saveData.SetPlayerStatusData(lowStabilityThreshold, stabilityReducePerMinute);

        var (medicineList, medicineMaxStack, medicineGrid) = MedicineManageSystem.Instance.GetCurrentMedicinesForSave();
        saveData.SetInventoryMedicineData(medicineList, medicineMaxStack, medicineGrid);
    }

    private void ApplySaveDataToGame(PlayerSaveData saveData)
    {
        PlayerBasicDataSystem.Instance.SetGoldCoin(saveData.GoldCoin);
        PlayerBasicDataSystem.Instance.SetPlayerProfessionType(saveData.PlayerProfession);

        ItemManageSystem.Instance.LoadItemsFromSaveData(saveData.InventoryItemData, saveData.ItemTotalGridCount, saveData.QuickBarCount);
        SouvenirManageSystem.Instance.LoadSouvenirsFromSaveData(saveData.InventorySouvenirData, saveData.SouvenirMaxStack, saveData.SouvenirGridCount);
        AchievementManageSystem.Instance.LoadAchievementsFromSaveData(saveData.AchievementData);

        PlayerStatusSystem.Instance.LoadStatusFromSaveData(saveData.LowStabilityThreshold, saveData.StabilityReducePerMinute);

        MedicineManageSystem.Instance.LoadMedicinesFromSaveData(saveData.InventoryMedicineData, saveData.MedicineMaxStack, saveData.MedicineGridCount);
    }

    // 校验槽位是否在0~9范围内
    private bool IsSlotValid(int slotIndex)
    {
        bool valid = slotIndex >= 0 && slotIndex < MAX_SAVE_SLOT;
        if (!valid) Debug.LogWarning("存档槽位必须在 0~9 之间");
        return valid;
    }

    // 写入所有存档到本地文件
    private void WriteAllSaveToDisk()
    {
        string json = JsonUtility.ToJson(_allSaveSlots, true);
        File.WriteAllText(_saveFilePath, json);
    }

    // 从本地加载所有存档
    private void LoadAllSaveFromDisk()
    {
        if (!File.Exists(_saveFilePath)) return;

        string json = File.ReadAllText(_saveFilePath);
        var loadedSaves = JsonUtility.FromJson<List<PlayerSaveData>>(json);

        if (loadedSaves == null) return;

        // 同步加载的数据到槽位列表
        for (int i = 0; i < MAX_SAVE_SLOT && i < loadedSaves.Count; i++)
        {
            _allSaveSlots[i] = loadedSaves[i];
        }
    }
}