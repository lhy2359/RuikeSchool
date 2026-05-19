using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("===== 背包格子配置 =====")]
    public UniversalGridUI gridPrefab;
    public Transform inventoryGridParent;
    [Tooltip("设置为游戏最大可能格子数")]
    public int maxPoolSize = 40;

    [Header("===== 分类Tab =====")]
    public Button tab_Item;
    public Button tab_Card;
    public Button tab_Medicine;
    public Button tab_Souvenir;
    public Color tabActiveColor = Color.white;
    public Color tabInactiveColor = Color.gray;

    [Header("===== 右键菜单 =====")]
    public GameObject rightClickMenu;
    public GameObject btn_Info;
    public GameObject btn_SetQuick;

    [Header("===== 物品详情面板 =====")]
    public GameObject infoPanel;
    public TMP_Text infoTitle;
    public TMP_Text infoContent;

    private enum InventoryTab { Item, Card, Medicine, Souvenir }
    private InventoryTab _currentTab = InventoryTab.Item;

    private List<UniversalGridUI> _gridPool = new List<UniversalGridUI>();
    private IInventoryProp _selectedProp;
    private int _selectedGridIndex = -1;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize(){ }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("【背包UI】检测到重复单例，销毁自身");
            DestroyImmediate(gameObject);
            return;
        }
        Instance = this;

        if (inventoryGridParent == null || inventoryGridParent.gameObject.scene != gameObject.scene)
        {
            inventoryGridParent = transform.Find("RightMainArea/MainScrollView/Viewport/Content");
        }

        // 清空Content
        if (inventoryGridParent != null)
        {
            foreach (Transform child in inventoryGridParent) Destroy(child.gameObject);
            inventoryGridParent.DetachChildren();
        }

        // 初始化UI
        rightClickMenu.SetActive(false);
        infoPanel.SetActive(false);

        // 绑定Tab事件
        tab_Item.onClick.AddListener(() => SwitchTab(InventoryTab.Item));
        tab_Card.onClick.AddListener(() => SwitchTab(InventoryTab.Card));
        tab_Medicine.onClick.AddListener(() => SwitchTab(InventoryTab.Medicine));
        tab_Souvenir.onClick.AddListener(() => SwitchTab(InventoryTab.Souvenir));

        InitGridPool();
        gameObject.SetActive(false);
    }

    private void Start()
    {
        // 绑定所有数据刷新事件
        ItemManageSystem.Instance.OnItemDataUpdated += RefreshAllUI;
        CardManageSystem.Instance.OnCardDataUpdated += RefreshAllUI;
        MedicineManageSystem.Instance.OnMedicineDataUpdated += RefreshAllUI;

        UpdateTabVisual();
        RefreshAllUI();
    }

    // 初始化格子对象池
    private void InitGridPool()
    {
        foreach (var grid in _gridPool) if (grid != null) Destroy(grid.gameObject);
        _gridPool.Clear();

        for (int i = 0; i < maxPoolSize; i++)
        {
            if (gridPrefab == null || inventoryGridParent == null) continue;
            var grid = Instantiate(gridPrefab, inventoryGridParent);
            grid.gameObject.SetActive(false);
            grid.name = $"Pool_Grid_{i}";
            _gridPool.Add(grid);
        }
    }

    // 切换Tab
    private void SwitchTab(InventoryTab tab)
    {
        _currentTab = tab;
        UpdateTabVisual();
        RefreshInventoryGrids();
        CloseAllPopups();
    }

    // 更新Tab视觉状态
    private void UpdateTabVisual()
    {
        tab_Item.image.color = _currentTab == InventoryTab.Item ? tabActiveColor : tabInactiveColor;
        tab_Card.image.color = _currentTab == InventoryTab.Card ? tabActiveColor : tabInactiveColor;
        tab_Medicine.image.color = _currentTab == InventoryTab.Medicine ? tabActiveColor : tabInactiveColor;
        tab_Souvenir.image.color = _currentTab == InventoryTab.Souvenir ? tabActiveColor : tabInactiveColor;
    }

    // 刷新当前Tab的格子显示
    public void RefreshAllUI()
    {
        RefreshInventoryGrids();
    }

    // 根据当前Tab刷新格子内容
    private void RefreshInventoryGrids()
    {
        ResetAllGrids();

        switch (_currentTab)
        {
            case InventoryTab.Item:
                RefreshItemTabContent();
                break;
            case InventoryTab.Card:
                RefreshCardTabContent();
                break;
            case InventoryTab.Medicine:
                RefreshMedicineTabContent();
                break;
            case InventoryTab.Souvenir:
                break;
        }
    }

    // 重置所有格子状态
    private void ResetAllGrids()
    {
        foreach (var grid in _gridPool)
        {
            if (grid == null) continue;
            grid.gameObject.SetActive(false);
            grid.SetHighlight(false);
            grid.RefreshGrid(null);
            grid.OnLeftClickEvent = null;
            grid.OnRightClickEvent = null;
        }
    }

    #region 物品刷新逻辑
    private void RefreshItemTabContent()
    {
        var gridData = ItemManageSystem.Instance.GetAllInventoryGrids();
        int quickBarCount = ItemManageSystem.Instance.CurrentQuickBarCount;

        for (int i = 0; i < gridData.Count; i++)
        {
            if (i >= _gridPool.Count) break;
            var grid = _gridPool[i];
            if (grid == null) continue;

            grid.gameObject.SetActive(true);
            grid.gridIndex = i;
            bool isQuickBar = i < quickBarCount;
            grid.InitGrid(isQuickBar ? (i + 1).ToString() : "");
            grid.SetHighlight(isQuickBar);
            grid.RefreshGrid(gridData[i]);

            grid.OnLeftClickEvent += OnGridLeftClick_Item;
            grid.OnRightClickEvent += (index) => OnGridRightClick(index, gridData[i]);
        }
    }
    #endregion

    #region 卡牌刷新逻辑
    private void RefreshCardTabContent()
    {
        var gridData = CardManageSystem.Instance.GetAllCardGrids();

        for (int i = 0; i < gridData.Count; i++)
        {
            if (i >= _gridPool.Count) break;
            var grid = _gridPool[i];
            if (grid == null) continue;

            grid.gameObject.SetActive(true);
            grid.gridIndex = i;
            grid.InitGrid(""); // 卡牌无数字快捷键
            grid.SetHighlight(false);
            grid.RefreshGrid(gridData[i]);

            // 绑定卡牌专属点击事件
            grid.OnLeftClickEvent += OnGridLeftClick_Card;
            grid.OnRightClickEvent += (index) => OnGridRightClick(index, gridData[i]);
        }
    }
    #endregion

    #region 药品刷新逻辑
    private void RefreshMedicineTabContent()
    {
        var gridData = MedicineManageSystem.Instance.GetAllMedicineGrids();

        for (int i = 0; i < gridData.Count; i++)
        {
            if (i >= _gridPool.Count) break;
            var grid = _gridPool[i];
            if (grid == null) continue;

            grid.gameObject.SetActive(true);
            grid.gridIndex = i;
            grid.InitGrid(""); // 药品无数字快捷键
            grid.SetHighlight(false);
            grid.RefreshGrid(gridData[i]);

            // 绑定药品专属点击事件
            grid.OnLeftClickEvent += OnGridLeftClick_Medicine;
            grid.OnRightClickEvent += (index) => OnGridRightClick(index, gridData[i]);
        }
    }
    #endregion

    #region 左键使用逻辑
    private void OnGridLeftClick_Item(int index) => ItemManageSystem.Instance.UseItem(index);
    private void OnGridLeftClick_Card(int index) => CardManageSystem.Instance.UseQuickCard(); // 直接使用卡牌
    private void OnGridLeftClick_Medicine(int index) => MedicineManageSystem.Instance.UseQuickMedicine(); // 直接使用药品
    #endregion

    #region 右键菜单
    private void OnGridRightClick(int index, IInventoryProp item)
    {
        _selectedGridIndex = index;
        _selectedProp = item;
        if (item == null) return;

        rightClickMenu.SetActive(true);
        rightClickMenu.transform.position = Input.mousePosition;
    }

    public void OnClick_ShowInfo()
    {
        if (_selectedProp == null) return;
        infoPanel.SetActive(true);
        infoTitle.text = _selectedProp.GetPropID();
        infoContent.text = _selectedProp.GetDescription();
        rightClickMenu.SetActive(false);
    }

    // 通用设置快捷栏（物品/卡牌/药品）
    public void OnClick_SetToQuickBar()
    {
        if (_selectedGridIndex == -1 || _selectedProp == null) return;

        switch (_currentTab)
        {
            case InventoryTab.Item:
                SetItemToQuickBar();
                break;
            case InventoryTab.Card:
                CardManageSystem.Instance.SetQuickBarCard(_selectedGridIndex);
                Debug.Log("卡牌已设置到快捷栏");
                break;
            case InventoryTab.Medicine:
                MedicineManageSystem.Instance.SetQuickBarMedicine(_selectedGridIndex);
                Debug.Log("药品已设置到快捷栏");
                break;
        }
        CloseAllPopups();
        RefreshAllUI();
    }

    // 物品专属快捷栏逻辑
    private void SetItemToQuickBar()
    {
        var system = ItemManageSystem.Instance;
        for (int i = 0; i < system.CurrentQuickBarCount; i++)
        {
            if (system.GetItemByGrid(i) == null)
            {
                system.SwapItemGrid(_selectedGridIndex, i);
                break;
            }
        }
    }
    #endregion

    public void CloseAllPopups()
    {
        rightClickMenu.SetActive(false);
        infoPanel.SetActive(false);
        _selectedGridIndex = -1;
        _selectedProp = null;
    }

    public void CloseInfoPanel() => infoPanel.SetActive(false);
    public void CloseInventoryPanel() => UIManager.Instance.CloseTopUI();

    private void OnDestroy()
    {
        if (ItemManageSystem.Instance != null) ItemManageSystem.Instance.OnItemDataUpdated -= RefreshAllUI;
        if (CardManageSystem.Instance != null) CardManageSystem.Instance.OnCardDataUpdated -= RefreshAllUI;
        if (MedicineManageSystem.Instance != null) MedicineManageSystem.Instance.OnMedicineDataUpdated -= RefreshAllUI;
        Instance = null;
    }
}