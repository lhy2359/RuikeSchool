using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 背包/快捷栏 通用格子UI脚本
/// 挂载在 Grid 预制体上
/// </summary>
public class UniversalGridUI : MonoBehaviour, IPointerClickHandler
{
    // 格子编号（对应数据系统里的格子索引）
    public int gridIndex;

    [Header("UI 元素引用（拖拽赋值）")]
    [Tooltip("物品图标")]
    public RawImage icon; 
    [Tooltip("右下角数量/次数字体")]
    public TMP_Text countText;
    [Tooltip("空格子灰色遮罩")]
    public GameObject emptyMask;
    [Tooltip("快捷物品高亮边框")]
    public Image quickHighlight;
    [Tooltip("左上角快捷键文字(1/2/Q/E)")]
    public TMP_Text keyText;

    // 外部注册点击事件（由背包管理器监听处理）
    public System.Action<int> OnLeftClickEvent;
    public System.Action<int> OnRightClickEvent;

    // 当前格子存放的物品
    private IInventoryProp _currentProp;

    /// 初始化格子（设置快捷键文字）
    public void InitGrid(string key = "")
    {
        // 设置快捷键
        keyText.text = key;
        // 没有快捷键就隐藏左上角文字
        keyText.gameObject.SetActive(!string.IsNullOrEmpty(key));
        // 默认隐藏高亮
        SetHighlight(false);
    }

    /// 刷新格子UI显示
    public void RefreshGrid(IInventoryProp prop)
    {
        _currentProp = prop;

        // 空物品 → 彻底隐藏Icon
        if (prop == null)
        {
            emptyMask.SetActive(true);
            icon.gameObject.SetActive(false);
            icon.color = Color.clear;
            icon.texture = null; 
            countText.gameObject.SetActive(false);
            return;
        }

        // 有物品 → 正常显示Icon
        emptyMask.SetActive(false);
        icon.gameObject.SetActive(true);
        icon.color = Color.white; // 恢复为不透明，正常渲染物品
        icon.texture = prop.GetIcon()?.texture;

        // 显示数量/次数
        countText.gameObject.SetActive(true);
        if (prop is RuntimeItemData itemData)
        {
            countText.text = $"{itemData.RemainingUses}/{itemData.MaxUses}";
        }
        else
        {
            countText.text = prop.GetPropCount().ToString();
        }
    }

    /// 设置快捷栏高亮
    public void SetHighlight(bool isShow)
    {
        quickHighlight.gameObject.SetActive(isShow);
    }

    /// 鼠标左右键点击检测
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClickEvent?.Invoke(gridIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClickEvent?.Invoke(gridIndex);
        }
    }

    /// 获取当前格子的物品
    public IInventoryProp GetCurrentProp() => _currentProp;
}