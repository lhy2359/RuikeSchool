using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InLevelUIManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public static InLevelUIManager Instance { get; private set; }

    [Header("===== 快捷栏配置 =====")]
    public UniversalGridUI gridPrefab;
    public int maxQuickBarSize = 6;
    public Transform quickBarParent;

    [Header("===== 顶部水平布局容器 =====")]
    public Transform topQuickBar;

    [Header("===== 计时器配置 =====")]
    [SerializeField] private TextMeshProUGUI _timerText;
    private float _elapsedTime;
    private bool _isTimerRunning;
    private const int MaxMinutes = 59;
    private const int MaxSeconds = 59;

    private List<UniversalGridUI> _quickBarPool = new List<UniversalGridUI>();
    private UniversalGridUI _cardSlot;
    private UniversalGridUI _medicineSlot;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (quickBarParent == null)
            quickBarParent = transform.Find("LeftQuickBarArea");

        if (topQuickBar == null)
            topQuickBar = transform.Find("TopQuickBar");

        if (_timerText == null)
            _timerText = transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        Instance = this;

        OnAfterDeserialize();
        ClearAllSlots();
        InitAllGridPools();
        ResetTimer();
    }

    private void Start()
    {
        ItemManageSystem.Instance.OnItemDataUpdated += RefreshAllUI;
        CardManageSystem.Instance.OnCardDataUpdated += RefreshAllUI;
        MedicineManageSystem.Instance.OnMedicineDataUpdated += RefreshAllUI;

        RefreshAllUI();
    }

    private void Update()
    {
        // 计时器逻辑
        if (GameStateManager.Instance != null)
        {
            bool isInLevel = GameStateManager.Instance.IsInLevel;
            if (isInLevel && !_isTimerRunning && _elapsedTime == 0)
            {
                StartTimer();
            }
            else if (!isInLevel && _isTimerRunning)
            {
                StopTimer();
            }
        }

        if (_isTimerRunning)
        {
            _elapsedTime += Time.deltaTime;
            int maxTotalSeconds = MaxMinutes * 60 + MaxSeconds;
            if (_elapsedTime >= maxTotalSeconds)
            {
                _elapsedTime = maxTotalSeconds;
                _isTimerRunning = false;
            }
            UpdateTimerDisplay();
        }

        // 快捷键逻辑
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseQuickBarItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseQuickBarItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseQuickBarItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseQuickBarItem(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseQuickBarItem(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UseQuickBarItem(5);

        if (Input.GetKeyDown(KeyCode.Q)) UseCardSlotItem();
        if (Input.GetKeyDown(KeyCode.E)) UseMedicineSlotItem();
    }

    /// 更新计时器文本显示
    private void UpdateTimerDisplay()
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_elapsedTime / 60);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60);
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// 开始运行计时器
    public void StartTimer() => _isTimerRunning = true;

    /// 停止计时器
    public void StopTimer() => _isTimerRunning = false;

    /// 重置计时器为00:00
    public void ResetTimer()
    {
        _elapsedTime = 0f;
        _isTimerRunning = false;
        UpdateTimerDisplay();
    }

    /// 清空所有快捷栏格子
    private void ClearAllSlots()
    {
        foreach (Transform t in quickBarParent) Destroy(t.gameObject);
        quickBarParent.DetachChildren();

        if (topQuickBar != null)
        {
            foreach (Transform t in topQuickBar) Destroy(t.gameObject);
            topQuickBar.DetachChildren();
        }
    }

    /// 初始化所有快捷栏格子对象池
    private void InitAllGridPools()
    {
        // 初始化快捷栏
        _quickBarPool.Clear();
        for (int i = 0; i < maxQuickBarSize; i++)
        {
            var g = Instantiate(gridPrefab, quickBarParent);
            g.gameObject.SetActive(false);
            g.name = $"Quick_{i}";
            _quickBarPool.Add(g);
        }

        // 初始化卡牌/药品槽
        if (topQuickBar != null)
        {
            _cardSlot = Instantiate(gridPrefab, topQuickBar);
            _cardSlot.gameObject.SetActive(false);
            _cardSlot.InitGrid("Q");
            _cardSlot.name = "CardGrid";

            _medicineSlot = Instantiate(gridPrefab, topQuickBar);
            _medicineSlot.gameObject.SetActive(false);
            _medicineSlot.InitGrid("E");
            _medicineSlot.name = "MedicineGrid";
        }
    }

    /// 统一刷新所有UI
    public void RefreshAllUI()
    {
        RefreshQuickBar();
        RefreshCardSlot();
        RefreshMedicineSlot();
    }

    /// 刷新物品快捷栏UI与点击事件
    private void RefreshQuickBar()
    {
        int count = ItemManageSystem.Instance.CurrentQuickBarCount;
        var quickData = ItemManageSystem.Instance.GetAllQuickBarItems();

        foreach (var g in _quickBarPool)
        {
            g.gameObject.SetActive(false);
            g.RefreshGrid(null);
            g.OnLeftClickEvent = null;
        }

        for (int i = 0; i < count; i++)
        {
            quickData.TryGetValue(i, out var item);
            var grid = _quickBarPool[i];
            grid.gameObject.SetActive(true);
            grid.InitGrid((i + 1).ToString());
            grid.SetHighlight(true);
            grid.RefreshGrid(item);

            int idx = i;
            grid.OnLeftClickEvent = (_) => UseQuickBarItem(idx);
        }
    }

    /// 刷新卡牌快捷槽UI与点击事件
    private void RefreshCardSlot()
    {
        if (_cardSlot == null) return;
        _cardSlot.gameObject.SetActive(true);
        _cardSlot.SetHighlight(true);

        var cardData = CardManageSystem.Instance != null ? CardManageSystem.Instance.GetQuickBarCard() : null;
        _cardSlot.RefreshGrid(cardData);

        _cardSlot.OnLeftClickEvent = (_) => UseCardSlotItem();
    }

    /// 刷新药品快捷槽UI与点击事件
    private void RefreshMedicineSlot()
    {
        if (_medicineSlot == null) return;
        _medicineSlot.gameObject.SetActive(true);
        _medicineSlot.SetHighlight(true);

        var medicineData = MedicineManageSystem.Instance != null ? MedicineManageSystem.Instance.GetQuickBarMedicine() : null;
        _medicineSlot.RefreshGrid(medicineData);

        _medicineSlot.OnLeftClickEvent = (_) => UseMedicineSlotItem();
    }

    /// 使用快捷栏物品
    public void UseQuickBarItem(int idx) => ItemManageSystem.Instance?.UseQuickBarItem(idx);

    /// 使用快捷栏卡牌
    public void UseCardSlotItem() => CardManageSystem.Instance?.UseQuickCard();

    /// 使用快捷栏药品
    public void UseMedicineSlotItem() => MedicineManageSystem.Instance?.UseQuickMedicine();

    private void OnDestroy()
    {
        if (ItemManageSystem.Instance != null) ItemManageSystem.Instance.OnItemDataUpdated -= RefreshAllUI;
        if (CardManageSystem.Instance != null) CardManageSystem.Instance.OnCardDataUpdated -= RefreshAllUI;
        if (MedicineManageSystem.Instance != null) MedicineManageSystem.Instance.OnMedicineDataUpdated -= RefreshAllUI;
        Instance = null;
    }
}