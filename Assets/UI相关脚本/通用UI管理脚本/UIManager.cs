using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("淡入淡出遮罩")]
    public Image fadeMask;

    private GameObject _repairUIInstance;
    private GameObject _inventoryUIInstance;
    private GameObject _inLevelUIInstance;

    private readonly Stack<GameObject> _uiStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("✅ UIManager 初始化成功！");

        // 初始化遮罩（从Canvas获取）
        if (fadeMask == null)
        {
            fadeMask = GameObject.Find("Canvas/Fademask").GetComponent<Image>();
        }
        if (fadeMask != null)
        {
            Color c = fadeMask.color;
            c.a = 0;
            fadeMask.color = c;
            fadeMask.gameObject.SetActive(false);
        }
    }

    // 打开背包
    public void OpenInventory()
    {

        GameObject prefab = Resources.Load<GameObject>("UIPrefabs/InventoryPanel");

        if (!prefab)
        {
            Debug.LogError("预制体加载失败！");
            return;
        }

        Debug.Log("预制体加载成功");

        if (_inventoryUIInstance == null)
        {
            Debug.Log("创建背包UI实例");
            Transform canvas = GameObject.Find("Canvas").transform;
            _inventoryUIInstance = Instantiate(prefab, canvas);
            _inventoryUIInstance.SetActive(false);
        }

        OpenUI(_inventoryUIInstance);
    }

    // 打开关卡内UI
    public void OpenInLevelUI()
    {
        GameObject prefab = Resources.Load<GameObject>("UIPrefabs/InLevelUI");

        if (!prefab) 
        { 
            Debug.LogError("关卡UI预制体加载失败！检查路径是否正确"); 
            return; 
        }

        Debug.Log("预制体加载成功");

        if (_inLevelUIInstance == null)
        {
            Debug.Log("创建关卡内UI实例");
            Transform canvas = GameObject.Find("Canvas").transform;
            _inLevelUIInstance = Instantiate(prefab, canvas);
            Debug.Log("关卡UI预制体加载成功");
        }
        _inLevelUIInstance.SetActive(true);
    }

    // 加载维修店
    public void OpenRepairShop()
    {

        GameObject repairPrefab = Resources.Load<GameObject>("UIPrefabs/RepairshopUI");
        if (repairPrefab == null)
        {
            Debug.LogError("维修店Prefab加载失败！请检查路径是否存在！");
            return;
        }
        Debug.Log($"成功加载维修店Prefab：{repairPrefab.name}");

        if (_repairUIInstance == null)
        {
            Transform canvas = GameObject.Find("Canvas").transform;
            _repairUIInstance = Instantiate(repairPrefab, canvas);

            if (_repairUIInstance == null)
            {
                Debug.LogError("维修店UI实例化失败！");
                return;
            }

            _repairUIInstance.SetActive(false);
            Debug.Log("维修店UI实例化成功！");
        }

        OpenUI(_repairUIInstance);
    }

    // 淡入逻辑
    public IEnumerator FadeIn(float fadeTime = 0.5f)
    {
        Debug.Log($"【UIManager】开始淡入，持续时间：{fadeTime}秒");
        if (fadeMask == null)
        {
            Debug.LogError("FadeMask未设置！无法执行淡入效果！");
            yield break;
        }
        fadeMask.gameObject.SetActive(true);
        float t = 0;
        Color color = fadeMask.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, t / fadeTime);
            fadeMask.color = color;
            yield return null;
        }
        color.a = 1;
        fadeMask.color = color;
    }

    // 淡出逻辑
    public IEnumerator FadeOut(float fadeTime = 0.5f)
    {
        Debug.Log($"开始淡出，持续时间：{fadeTime}秒");
        if (fadeMask == null)
        {
            Debug.LogError("FadeMask未设置！无法执行淡出效果！");
            yield break;
        }
        float t = 0;
        Color color = fadeMask.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, t / fadeTime);
            fadeMask.color = color;
            yield return null;
        }
        color.a = 0;
        fadeMask.color = color;
        fadeMask.gameObject.SetActive(false);
    }

    // UI栈逻辑
    public void OpenUI(GameObject ui)
    {
        if (ui == null)
        {
            Debug.LogError("传入的UI对象为空！无法打开！");
            return;
        }
        if (ui.activeSelf) return;

        if (_uiStack.Count > 0)
            _uiStack.Peek().SetActive(false);

        ui.SetActive(true);
        _uiStack.Push(ui);

        // 调用状态管理器
        GameStateManager.Instance.SwitchToUIState();
        Debug.Log($"成功打开UI：{ui.name}");
    }

    public void CloseTopUI()
    {
        if (_uiStack.Count == 0) return;
        var topUI = _uiStack.Pop();
        topUI.SetActive(false);

        if (_uiStack.Count > 0)
        {
            _uiStack.Peek().SetActive(true);
        }
        else
        {   // 调用状态管理器
            GameStateManager.Instance.SwitchToGamePlayState();
        }
    }

    public GameObject GetCurrentTopUI()
    {
        return _uiStack.Count > 0 ? _uiStack.Peek() : null;
    }
}