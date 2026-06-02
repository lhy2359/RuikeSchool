using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelModeType
{
    none,
    ThesisMode,     // 正论
    AntithesisMode, // 逆论
    ParadoxMode     // 悖论
}

public class PlainLevelSystem : MonoBehaviour
{
    public static PlainLevelSystem Instance { get; private set; }

    private LevelModeType _modeType;

    private int _normalChestCount;
    private int _purpleChestCount;
    private int _goldenChestCount;

    private int _normalVendingMachineCount;
    private int _brokenVendingMachineCount;

    private int _finisAsymmetricLevelEventCount;
    private int _mnemosAsymmetricLevelEventCount;
    private int _mundusAsymmetricLevelEventCount;

    private int _storeCount;
    private int _repairShopCount;
    private int _pharmacyShopCount;

    public LevelModeType ModeType => _modeType;
    public int NormalChestCount => _normalChestCount;
    public int PurpleChestCount => _purpleChestCount;
    public int GoldenChestCount => _goldenChestCount;

    public int NormalVendingMachineCount => _normalVendingMachineCount;
    public int BrokenVendingMachineCount => _brokenVendingMachineCount;

    public int FinisAsymmetricLevelEventCount => _finisAsymmetricLevelEventCount;
    public int MnemosAsymmetricLevelEventCount => _mnemosAsymmetricLevelEventCount;
    public int MundusAsymmetricLevelEventCount => _mundusAsymmetricLevelEventCount;

    public int StoreCount => _storeCount;
    public int RepairShopCount => _repairShopCount;
    public int PharmacyShopCount => _pharmacyShopCount;

    private List<Vector3> _spawnPositions = new List<Vector3>();

    private List<Vector3> _normalChestPositions = new List<Vector3>();
    private List<Vector3> _purpleChestPositions = new List<Vector3>();
    private List<Vector3> _goldenChestPositions = new List<Vector3>();

    private List<Vector3> _storePositions = new List<Vector3>();
    private List<Vector3> _repairShopPositions = new List<Vector3>();
    private List<Vector3> _PharmacyPositions = new List<Vector3>();

    private List<Vector3> _normalVendingMachinePositions = new List<Vector3>();
    private List<Vector3> _brokenVendingMachinePositions = new List<Vector3>();

    private List<Vector3> _destinationPositions = new List<Vector3>();

    // 防重叠
    private HashSet<Vector3Int> _globalOccupiedCells = new HashSet<Vector3Int>();

    // 判断格子是否被全局占用
    public bool IsCellGloballyOccupied(Vector3Int cell) => _globalOccupiedCells.Contains(cell);

    // 标记格子为全局已占用
    public void OccupyGlobalCell(Vector3Int cell) => _globalOccupiedCells.Add(cell);

    // 清空全局占用记录（切换关卡/重置时调用）
    public void ClearGlobalOccupiedCells() => _globalOccupiedCells.Clear();

    public void AddSpawnPosition(Vector3 pos) => _spawnPositions.Add(pos);
    public void AddNormalChestPosition(Vector3 pos) => _normalChestPositions.Add(pos);
    public void AddPurpleChestPosition(Vector3 pos) => _purpleChestPositions.Add(pos);
    public void AddGoldenChestPosition(Vector3 pos) => _goldenChestPositions.Add(pos);
    public void AddStorePosition(Vector3 pos) => _storePositions.Add(pos);
    public void AddRepairShopPosition(Vector3 pos) => _repairShopPositions.Add(pos);
    public void AddPharmacyShopPosition(Vector3 pos) => _PharmacyPositions.Add(pos);

    public void AddNormalVendingMachinePosition(Vector3 pos) => _normalVendingMachinePositions.Add(pos);
    public void AddBrokenVendingMachinePosition(Vector3 pos) => _brokenVendingMachinePositions.Add(pos);

    public void AddDestinationPosition(Vector3 pos) => _destinationPositions.Add(pos);

    public List<Vector3> GetSpawnPositions() => _spawnPositions;
    public List<Vector3> GetNormalChestPositions() => _normalChestPositions;
    public List<Vector3> GetPurpleChestPositions() => _purpleChestPositions;
    public List<Vector3> GetGoldenChestPositions() => _goldenChestPositions;
    public List<Vector3> GetStorePositions() => _storePositions;
    public List<Vector3> GetRepairShopPositions() => _repairShopPositions;
    public List<Vector3> GetPharmacyShopPositions() => _PharmacyPositions;
    public List<Vector3> GetNormalVendingMachinePositions() => _normalVendingMachinePositions;
    public List<Vector3> GetBrokenVendingMachinePositions() => _brokenVendingMachinePositions;
    public List<Vector3> GetDestinationPositions() => _destinationPositions;

    // 静态初始化
    public static void Initialize()
    {
        // 如果已存在实例，无需重复创建
        if (Instance != null)
        {
            Debug.LogWarning("PlainLevelSystem 已初始化，无需重复调用！");
            return;
        }

        // 创建空物体挂载系统
        GameObject systemObj = new GameObject("PlainLevelSystem");
        Instance = systemObj.AddComponent<PlainLevelSystem>();

        // 设为全局不销毁
        DontDestroyOnLoad(systemObj);

        GameStateManager.Instance.SetInLevel(true);
        UIManager.Instance.OpenInLevelUI();

        Debug.Log("PlainLevelSystem 静态初始化完成");
    }
    // 静态销毁
    public static void Destroy()
    {
        if (Instance == null) return;

        // 销毁游戏物体
        Destroy(Instance.gameObject);
        // 清空静态实例
        Instance = null;

        Debug.Log("PlainLevelSystem 已销毁");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChooseLeveMode(LevelModeType type = LevelModeType.ThesisMode)
    {
        // 切换模式时，清空所有位置记录，并清空全局占用记录
        ResetAllMarkPositions();
        ClearGlobalOccupiedCells();
        DestroyGeneratedObjects();

       _modeType = type;
        if (type == LevelModeType.none) return;

        if (type == LevelModeType.ThesisMode)
        {
            // 正论模式
            _normalChestCount = PlainLevelConfigSO.Instance.NormalChestCount;
            _purpleChestCount = PlainLevelConfigSO.Instance.PurpleChestCount;
            _goldenChestCount = PlainLevelConfigSO.Instance.GoldenChestCount;
            _normalVendingMachineCount = PlainLevelConfigSO.Instance.NormalVendingMachineCount;
            _brokenVendingMachineCount = PlainLevelConfigSO.Instance.BrokenVendingMachineCount;
            _finisAsymmetricLevelEventCount = PlainLevelConfigSO.Instance.FinisAsymmetricLevelEventCount;
            _mnemosAsymmetricLevelEventCount = PlainLevelConfigSO.Instance.MnemosAsymmetricLevelEventCount;
            _mundusAsymmetricLevelEventCount = PlainLevelConfigSO.Instance.MundusAsymmetricLevelEventCount;
            _storeCount = PlainLevelConfigSO.Instance.StoreCount;
            _repairShopCount = PlainLevelConfigSO.Instance.RepairShopCount;
            _pharmacyShopCount = PlainLevelConfigSO.Instance.PharmacyShopCount;
        }
        else if (type == LevelModeType.AntithesisMode)
        {
            // 逆论模式
        }
        else if (type == LevelModeType.ParadoxMode)
        {
            // 悖论模式
        }
    }

    public void ResetAllMarkPositions()
    {
        _spawnPositions.Clear();

        _normalChestPositions.Clear();
        _purpleChestPositions.Clear();
        _goldenChestPositions.Clear();

        _storePositions.Clear();
        _repairShopPositions.Clear();
        _PharmacyPositions.Clear();

        _normalVendingMachinePositions.Clear();
        _brokenVendingMachinePositions.Clear();

        _destinationPositions.Clear();

    }
    public void DestroyGeneratedObjects()
    {
        GameObject parent = GameObject.Find("GeneratedMarkPoints");
        if (parent != null)
        {
            Destroy(parent);
        }
    }

    public void InitAllMarkPositions()
    {
        Scene targetScene = SceneManager.GetSceneByName("PlainLevelScene");

        if (!targetScene.isLoaded)
        {
            Debug.LogError("PlainLevelScene 未加载，无法生成标记点！");
            return;
        }

        // 生成出生点
        SpawnMarkGenerator spawnGenerator = new SpawnMarkGenerator(targetScene, "SpawnMarkPoint");
        spawnGenerator.GenerateAndRecord(1);

        // 生成终点
        DestinationMarkGenerator destinationGenerator = new DestinationMarkGenerator(
            targetScene,
            "DestinationMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/Destination")
        );
        destinationGenerator.GenerateAndRecord(1);

        // 生成宝箱类
        NormalChestMarkGenerator normalChestGenerator = new NormalChestMarkGenerator(
            targetScene,
            "NormalAndPurpleChestMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/NormalChest")
        );
        normalChestGenerator.GenerateAndRecord(NormalChestCount);

        PurpleChestMarkGenerator purpleChestGenerator = new PurpleChestMarkGenerator(
            targetScene,
            "NormalAndPurpleChestMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/PurpleChest")
        );
        purpleChestGenerator.GenerateAndRecord(PurpleChestCount);

        // 生成金色宝箱和Finis事件
        GoldenChestMarkGenerator goldenChestGenerator = new GoldenChestMarkGenerator(
            targetScene,
            "FinisAsymmetricAndGoldChestMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/GoldenChest")
        );
        goldenChestGenerator.GenerateAndRecord(GoldenChestCount);

        // 生成正常和损坏自动售货机
        NormalVendingMachineMarkGenerator normalVendingGenerator = new NormalVendingMachineMarkGenerator(
            targetScene,
            "VendingMachineMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/NormalVendingMachine")
        );
        normalVendingGenerator.GenerateAndRecord(NormalVendingMachineCount);

        BrokenVendingMachineMarkGenerator brokenVendingGenerator = new BrokenVendingMachineMarkGenerator(
            targetScene,
            "VendingMachineMarkPoint", 
            Resources.Load<GameObject>("MarkPositionPrefabs/BrokenVendingMachine")
        );
        brokenVendingGenerator.GenerateAndRecord(BrokenVendingMachineCount);

        // 生成商店类
        StoreMarkGenerator storeGenerator = new StoreMarkGenerator(
            targetScene,
            "ShopMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/Store")
        );
        storeGenerator.GenerateAndRecord(StoreCount);

        RepairShopMarkGenerator repairShopGenerator = new RepairShopMarkGenerator(
            targetScene,
            "ShopMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/RepairShop")
        );
        repairShopGenerator.GenerateAndRecord(RepairShopCount);

        PharmacyMarkGenerator pharmacyGenerator = new PharmacyMarkGenerator(
            targetScene,
            "ShopMarkPoint",
            Resources.Load<GameObject>("MarkPositionPrefabs/Pharmacy")
        );
        pharmacyGenerator.GenerateAndRecord(PharmacyShopCount);

        Debug.Log("所有标记点生成完成！");
    }

    private void OnApplicationQuit()
    {
        Instance = null;
    }
}