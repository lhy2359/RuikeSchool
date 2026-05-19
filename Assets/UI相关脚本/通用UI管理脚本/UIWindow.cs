using UnityEngine;

/// <summary>
/// 所有UI窗口的统一接口
/// </summary>
public interface IUIWindow
{
    void Open();
    void Close();
}

/// <summary>
/// 实现IUIWindow接口
/// </summary>
public class UIWindow : MonoBehaviour, IUIWindow
{
    /// <summary>
    /// 打开当前UI
    /// </summary>
    public void Open()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭当前UI（调用UI栈系统）
    /// </summary>
    public void Close()
    {
        UIManager.Instance.CloseTopUI();
    }
}