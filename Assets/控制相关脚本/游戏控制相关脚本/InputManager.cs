using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                if (_instance == null)
                {
                    Debug.LogError("场景中未找到 InputManager！请确保 PersistentScene 已加载且 GameSystem 物体已激活");
                }
            }
            return _instance;
        }
    }

    [Header("调试信息")]
    [SerializeField] private bool _showDebugLogs = true;

    public bool CanGameInput { get; private set; } = true;

    // 按键配置（统一管理）
    private KeyCode _inventoryKey;  // 背包
    private KeyCode _cardKey;      // 卡牌
    private KeyCode _medicineKey;  // 药品

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 从配置文件读取所有按键
        if (InputKeyConfigSO.Instance != null)
        {
            _inventoryKey = InputKeyConfigSO.Instance.InventoryKey;
            _cardKey = InputKeyConfigSO.Instance.CardKey;     
            _medicineKey = InputKeyConfigSO.Instance.MedicineKey;
            if (_showDebugLogs) Debug.Log($"InputManager 初始化 | 背包:{_inventoryKey} 卡牌:{_cardKey} 药品:{_medicineKey}");
        }
        else
        {
            Debug.LogError("未找到 InputKeyConfigSO 配置文件！");
        }
    }

    // 修改输入状态
    public void SetCanGameInput(bool value)
    {
        CanGameInput = value;
        if (_showDebugLogs) Debug.Log($"InputManager 输入状态已设置为：{value}");
    }

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }
        else
        {
            Debug.LogWarning("GameStateManager 未找到，输入状态切换可能失效");
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameStateManager.GameState state)
    {
        // 只有 GamePlay 状态允许游戏输入
        SetCanGameInput(state == GameStateManager.GameState.GamePlay);
    }

    private void Update()
    {
        // 背包按键
        if (Input.GetKeyDown(_inventoryKey))
        {
            if (_showDebugLogs) Debug.Log("按下背包键");
            TryToggleInventory();
        }

        // 只有能操作游戏时，才响应
        if (CanGameInput)
        {
            // 只有在关卡里才能使用
            // 使用卡牌
            if (Input.GetKeyDown(_cardKey))
            {
                if (GameStateManager.Instance.IsInLevel)
                    UseCard();
                else Debug.Log("不在关卡里");
            }
            // 使用药品
            if (Input.GetKeyDown(_medicineKey))
            {
                if (GameStateManager.Instance.IsInLevel)
                    UseMedicine();
                else Debug.Log("不在关卡里");
            }

        }
        // 调试按键：P 获取火把，L 获取治疗药水
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("按下了P");
            GenerateNewPropUtil.PlayerGetNewItemById("Item_Torch");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("按下了L");
            GenerateNewPropUtil.PlayerGetNewMedicineById("Medicine_HealingPotionI");
        }
    }

    // 使用卡牌（关卡UI调用）
    private void UseCard()
    {
        if (InLevelUIManager.Instance == null)
        {
            Debug.Log("InLevelUIManager为空");
            return;
        }
        InLevelUIManager.Instance.UseCardSlotItem();
        if (_showDebugLogs) Debug.Log("使用卡牌");
    }

    // 使用药品（关卡UI调用）
    private void UseMedicine()
    {
        if (InLevelUIManager.Instance == null)
        {
            Debug.Log("InLevelUIManager为空");
            return;
        }
        InLevelUIManager.Instance.UseMedicineSlotItem();
        if (_showDebugLogs) Debug.Log("使用药品");
    }

    // 尝试切换背包UI（根据当前状态决定是打开还是关闭）
    private void TryToggleInventory()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("UIManager 未初始化");
            return;
        }

        var currentState = GameStateManager.Instance.CurrentState;
        GameObject topUI = UIManager.Instance.GetCurrentTopUI();

        if (!GameStateManager.Instance.IsInLevel)
        {
            Debug.Log("不在关卡里，无法打开背包");
            return;
        }

        bool canToggle = (currentState == GameStateManager.GameState.GamePlay) ||
                         (currentState == GameStateManager.GameState.UI && topUI != null && topUI.name.Contains("InventoryPanel"));

        if (canToggle)
        {
            if (currentState == GameStateManager.GameState.GamePlay)
                UIManager.Instance.OpenInventory();
            else
                UIManager.Instance.CloseTopUI();
        }
    }

    // 刷新按键配置
    public void RefreshKeyConfig()
    {
        if (InputKeyConfigSO.Instance != null)
        {
            _inventoryKey = InputKeyConfigSO.Instance.InventoryKey;
            _cardKey = InputKeyConfigSO.Instance.CardKey;      
            _medicineKey = InputKeyConfigSO.Instance.MedicineKey;
            Debug.Log($"按键配置刷新 | 背包:{_inventoryKey} 卡牌:{_cardKey} 药品:{_medicineKey}");
        }
    }
}