using UnityEngine;

public class CameraControlSystem : MonoBehaviour
{
    public static CameraControlSystem Instance;

    public Transform target; // 玩家目标
    [Header("基础设置")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float defaultCameraViewSize = 5f;

    [Header("自由视角设置")]
    [SerializeField] private float _freeCameraSpeed = 12f; // 自由视角移动速度
    [SerializeField] private int _edgeScrollWidth = 20; // 屏幕边缘触发宽度

    private bool _isFreeCameraMode = false;
    private Vector3 _savedCameraPosition; // 进入自由视角前的相机位置

    private Camera _mainCamera;
    private float _cameraHalfWidth;
    private float _cameraHalfHeight;
    private Bounds _mapBounds; // 地图边界

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _mainCamera = GetComponent<Camera>();
        UpdateCameraViewRange(defaultCameraViewSize);

        AutoBindPlayer();
    }

    // 自动查找并绑定玩家
    private void AutoBindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            Debug.Log($"相机系统已自动绑定玩家：{playerObj.name}");
        }
        else
        {
            Debug.LogError("相机系统未找到标签为Player的游戏对象！");
        }
    }

    // 更新相机正交视野大小
    public void UpdateCameraViewRange(float viewRange)
    {
        _mainCamera.orthographicSize = viewRange;
        _cameraHalfHeight = _mainCamera.orthographicSize;
        _cameraHalfWidth = _cameraHalfHeight * _mainCamera.aspect;
    }

    // 更新地图边界（初始化/切换关卡时调用）
    public void UpdateMapBounds(Bounds bounds)
    {
        _mapBounds = bounds;
        Debug.Log($"相机地图边界已更新：{bounds}");
    }

    // 相机瞬间瞬移到目标位置（传送时用）
    public void TeleportCamera(Vector3 targetPos)
    {
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    private void Update()
    {
        // 只有在关卡内才能切换视角模式
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsInLevel)
        {
            // 按G切换自由视角模式
            if (Input.GetKeyDown(KeyCode.G))
            {
                ToggleFreeCameraMode();
            }

            // 按H一键回到玩家视角
            if (Input.GetKeyDown(KeyCode.H))
            {
                ReturnToPlayerCamera();
            }

            // 自由视角下处理鼠标滑动控制
            if (_isFreeCameraMode)
            {
                HandleFreeCameraMovement();
            }
        }
    }

    private void LateUpdate()
    {
        // 自由视角模式下不跟随玩家
        if (_isFreeCameraMode || target == null) return;

        // 原有相机平滑跟随+边界限制逻辑
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        // 限制相机在地图边界内
        if (_mapBounds.size.magnitude > 0)
        {
            float minX = _mapBounds.min.x + _cameraHalfWidth;
            float maxX = _mapBounds.max.x - _cameraHalfWidth;
            float minY = _mapBounds.min.y + _cameraHalfHeight;
            float maxY = _mapBounds.max.y - _cameraHalfHeight;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        // 平滑过渡
        Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        transform.position = smoothPos;
    }

    // 切换自由视角模式
    private void ToggleFreeCameraMode()
    {
        _isFreeCameraMode = !_isFreeCameraMode;

        if (_isFreeCameraMode)
        {
            // 进入自由视角：保存当前相机位置，禁用玩家输入
            _savedCameraPosition = transform.position;
            InputManager.Instance.SetCanGameInput(false);
            Debug.Log("进入自由视角模式 | 按H返回玩家视角");
        }
        else
        {
            // 退出自由视角：恢复玩家输入
            InputManager.Instance.SetCanGameInput(true);
            Debug.Log("退出自由视角模式");
        }
    }

    // 一键回到玩家视角
    private void ReturnToPlayerCamera()
    {
        if (!_isFreeCameraMode || target == null) return;

        _isFreeCameraMode = false;
        InputManager.Instance.SetCanGameInput(true);

        // 瞬间瞬移回玩家位置
        TeleportCamera(target.position);
        Debug.Log("已回到玩家视角");
    }

    // 处理自由视角下的鼠标滑动控制
    private void HandleFreeCameraMovement()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3 moveDir = Vector3.zero;

        // 左边缘
        if (mousePos.x < _edgeScrollWidth)
            moveDir.x = -1;
        // 右边缘
        else if (mousePos.x > Screen.width - _edgeScrollWidth)
            moveDir.x = 1;

        // 下边缘
        if (mousePos.y < _edgeScrollWidth)
            moveDir.y = -1;
        // 上边缘
        else if (mousePos.y > Screen.height - _edgeScrollWidth)
            moveDir.y = 1;

        // 归一化方向（防止斜着移动更快）
        if (moveDir.magnitude > 1)
            moveDir.Normalize();

        // 计算新位置
        Vector3 newPos = transform.position + moveDir * _freeCameraSpeed * Time.deltaTime;

        // 地图边界限制
        if (_mapBounds.size.magnitude > 0)
        {
            float minX = _mapBounds.min.x + _cameraHalfWidth;
            float maxX = _mapBounds.max.x - _cameraHalfWidth;
            float minY = _mapBounds.min.y + _cameraHalfHeight;
            float maxY = _mapBounds.max.y - _cameraHalfHeight;

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        }

        transform.position = newPos;
    }

    // 设置相机背景颜色
    public void SetBackgroundColor(Color color)
    {
        _mainCamera.backgroundColor = color;
    }
}