using UnityEngine;

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;

    [Header("迷雾设置")]
    [SerializeField] private Material _fogMaterial; 
    [SerializeField] private Color _fogColor = Color.black; // 迷雾颜色，默认纯黑
    [SerializeField] private float _edgeSoftness = 0.02f; // 边缘柔化程度，0=硬边缘

    private Transform _player;
    private Camera _mainCamera;
    private bool _wasInLevelLastFrame = false; // 记录上一帧是否在关卡内

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _mainCamera = Camera.main;
    }

    private void Start()
    {
        AutoFindPlayer();
        // 游戏开始时隐藏迷雾
        SetFogEnabled(false);
        Debug.Log("战争迷雾系统初始化完成");
    }

    private void AutoFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            Debug.Log($"自动找到玩家：{playerObj.name}");
        }
        else
        {
            Debug.LogError("未找到标签为Player的游戏对象！");
        }
    }

    // 启用/禁用战争迷雾
    public void SetFogEnabled(bool enabled)
    {
        if (_fogMaterial != null)
        {
            _fogMaterial.SetColor("_MaskColor", enabled ? _fogColor : Color.clear);
        }
    }

    // 每帧更新迷雾遮罩的中心和半径
    private void UpdateFogMask()
    {
        if (_player == null)
        {
            AutoFindPlayer();
            if (_player == null) return;
        }

        // 材质和相机容错
        if (_fogMaterial == null || _mainCamera == null || PlayerBasicDataSystem.Instance == null)
            return;

        // 使用世界坐标
        Vector3 playerWorldPos = _player.position;

        float viewRadius = PlayerBasicDataSystem.Instance.CurrentViewRange;

        // 更新Shader
        _fogMaterial.SetVector("_PlayerWorldPos", playerWorldPos);
        _fogMaterial.SetFloat("_ViewRadius", viewRadius);
        _fogMaterial.SetFloat("_CameraHalfHeight", _mainCamera.orthographicSize);
        _fogMaterial.SetFloat("_AspectRatio", _mainCamera.aspect);
        _fogMaterial.SetFloat("_EdgeSoftness", _edgeSoftness);
    }

    // 清除所有迷雾（调试用）
    public void ClearAllFog()
    {
        SetFogEnabled(false);
        Debug.Log("所有迷雾已清除");
    }

    private void LateUpdate()
    {
        bool isInLevelNow = GameStateManager.Instance != null && GameStateManager.Instance.IsInLevel;

        // 检测关卡进入/退出事件
        if (isInLevelNow && !_wasInLevelLastFrame)
        {
            Debug.Log("检测到进入关卡，启用战争迷雾");
            SetFogEnabled(true);
        }
        else if (!isInLevelNow && _wasInLevelLastFrame)
        {
            Debug.Log("检测到退出关卡，禁用战争迷雾");
            SetFogEnabled(false);
        }

        // 只有在关卡内才更新迷雾
        if (isInLevelNow)
        {
            UpdateFogMask();
        }

        // 保存当前帧状态
        _wasInLevelLastFrame = isInLevelNow;
    }
}