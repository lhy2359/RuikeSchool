using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerControlSystem : MonoBehaviour
{
    public static PlayerControlSystem Instance { get; private set; }

    [Header("===== 碰撞检测设置 =====")]
    public LayerMask airwallLayer;
    private float _playerColliderHalfWidth;
    private float _playerColliderHeight;
    private float _playerColliderHalfHeight;
    private Vector2 _playerColliderSize => new Vector2(_playerColliderHalfWidth * 2, _playerColliderHeight);

    [Header("===== 内部组件 =====")]
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    [Header("===== 地图边界 =====")]
    private Bounds _mapBounds;
    private Vector3 _cellSize;

    private PlayerBasicDataSystem _playerBasicData;
    private InputManager _inputManager;

    // 缓存空气墙父物体
    private Transform _airwallParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (PlayerCollisionDetectionConfigSO.Instance)
        {
            _playerColliderHalfWidth = PlayerCollisionDetectionConfigSO.Instance.playerColliderHalfWidth;
            _playerColliderHalfHeight = PlayerCollisionDetectionConfigSO.Instance.playerColliderHalfHeight;
            _playerColliderHeight = _playerColliderHalfHeight * 2;
        }
        else
        {
            Debug.LogError("缺少PlayerCollisionDetectionConfigSO！");
        }

        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnAnySceneLoaded;
        SceneManager.sceneUnloaded += OnAnySceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
        SceneManager.sceneUnloaded -= OnAnySceneUnloaded;
    }

    private void Start()
    {
        _playerBasicData = PlayerBasicDataSystem.Instance;
        _inputManager = InputManager.Instance;

        _rb.freezeRotation = true;
    }

    private void OnAnySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PersistentScene" || scene.name == "UIScene") return;

        _airwallParent = GameObject.Find("Airwall")?.transform;

        if (_airwallParent != null)
        {
            UpdateMapBounds();
            Debug.Log($"成功找到地图[{scene.name}]的空气墙父物体");
        }
        else
        {
            Debug.LogWarning($"地图[{scene.name}]中未找到名为'Airwall'的父物体");
        }
    }

    private void OnAnySceneUnloaded(Scene scene)
    {
        if (scene.name != "PersistentScene" && scene.name != "UIScene")
        {
            _airwallParent = null;
            _mapBounds = new Bounds(transform.position, Vector3.one);
            Debug.Log($"【玩家】场景卸载，重置边界为：{_mapBounds}");
        }
    }

    // 合并所有激活的子空气墙边界
    public void UpdateMapBounds()
    {
        if (_airwallParent == null)
        {
            Debug.LogError("【玩家】空气墙父物体为空，无法更新边界！");
            return;
        }

        Debug.Log($"【玩家】开始更新边界，空气墙子物体数量：{_airwallParent.childCount}");

        // 遍历子物体，只取第一个激活的Tilemap
        foreach (Transform child in _airwallParent.transform)
        {
            Debug.Log($"【玩家】检查子物体：{child.name} | 激活状态：{child.gameObject.activeSelf}");
            if (!child.gameObject.activeSelf) continue;

            Tilemap tilemap = child.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogWarning($"【玩家】子物体{child.name}不是Tilemap，跳过");
                continue;
            }

            tilemap.CompressBounds();
            Bounds childBounds = tilemap.localBounds;
            Debug.Log($"【玩家】原始Tilemap本地边界：Min({childBounds.min.x:F2}, {childBounds.min.y:F2}) | Max({childBounds.max.x:F2}, {childBounds.max.y:F2})");

            // 转换为世界坐标
            childBounds.min = tilemap.transform.TransformPoint(childBounds.min);
            childBounds.max = tilemap.transform.TransformPoint(childBounds.max);
            _cellSize = tilemap.cellSize;

            Debug.Log($"【玩家】转换为世界坐标后：Min({childBounds.min.x:F2}, {childBounds.min.y:F2}) | Max({childBounds.max.x:F2}, {childBounds.max.y:F2})");
            Debug.Log($"【玩家】CellSize偏移：{_cellSize}");

            _mapBounds = new Bounds();
            _mapBounds.min = childBounds.min + _cellSize;
            _mapBounds.max = childBounds.max - _cellSize;

            // 打印独立地图边界
            Debug.Log($"左 (MinX)：{_mapBounds.min.x:F2}");
            Debug.Log($"右 (MaxX)：{_mapBounds.max.x:F2}");
            Debug.Log($"下 (MinY)：{_mapBounds.min.y:F2}");
            Debug.Log($"上 (MaxY)：{_mapBounds.max.y:F2}");


            // 找到第一个激活的就停止，不合并其他地图
            break;
        }

        if (CameraControlSystem.Instance)
        {
            Debug.Log($"【玩家】向相机发送新边界");
            CameraControlSystem.Instance.UpdateMapBounds(_mapBounds);
        }
        else
        {
            Debug.LogError("【玩家】相机实例为空，无法发送边界！");
        }
    }

    private void Update()
    {
        bool canInput = _inputManager && _inputManager.CanGameInput;
        if (canInput)
        { 
            // 如果可以移动,获取当前移动值
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; 
        }
        else 
        {
            // 不能移动则移动输入为0
            _moveInput = Vector2.zero; 
        }
        UpdatePlayerAnimation();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {

        float currentMoveSpeed = _playerBasicData ? _playerBasicData.CurrentMoveSpeed : 5f;
        Vector2 moveDelta = _moveInput * currentMoveSpeed * Time.fixedDeltaTime;
        Vector2 center = _rb.position + new Vector2(0, _playerColliderHalfHeight);

        if (Physics2D.OverlapBox(center + moveDelta, _playerColliderSize * 0.9f, 0f, airwallLayer))
        {
            if (Physics2D.OverlapBox(center + new Vector2(moveDelta.x, 0),
                new Vector2(_playerColliderSize.x * 0.9f, _playerColliderSize.y * 0.8f), 0f, airwallLayer))
                moveDelta.x = 0;

            if (Physics2D.OverlapBox(center + new Vector2(0, moveDelta.y),
                new Vector2(_playerColliderSize.x * 0.8f, _playerColliderSize.y * 0.9f), 0f, airwallLayer))
                moveDelta.y = 0;
        }

        Vector2 targetPos = _rb.position + moveDelta;

        // 计算玩家边界限制
        float limitMinX = _mapBounds.min.x + _playerColliderHalfWidth;
        float limitMaxX = _mapBounds.max.x - _playerColliderHalfWidth;
        float limitMinY = _mapBounds.min.y;
        float limitMaxY = _mapBounds.max.y - _playerColliderHeight;

        targetPos.x = Mathf.Clamp(targetPos.x, limitMinX, limitMaxX);
        targetPos.y = Mathf.Clamp(targetPos.y, limitMinY, limitMaxY);

        _rb.MovePosition(targetPos);
    }

    private void UpdatePlayerAnimation()
    {
        bool isMoving = _moveInput.magnitude > 0.1f;
        _animator.SetBool("IsMoving", isMoving);

        if (Mathf.Abs(_moveInput.y) > Mathf.Abs(_moveInput.x))
        {
            _animator.SetFloat("MoveY", Mathf.Sign(_moveInput.y));
            _animator.SetFloat("MoveX", 0);
        }
        else if (_moveInput.x != 0)
        {
            _animator.SetFloat("MoveX", Mathf.Sign(_moveInput.x));
            _animator.SetFloat("MoveY", 0);
        }
    }

    // 根据传送目标点，找到对应房间的空气墙并更新边界
    public void UpdateMapBoundsByTargetPoint(Vector3 targetPoint)
    {
        Debug.Log($"【传送调试】开始更新边界，目标点坐标：{targetPoint}");

        Transform airwallParent = GameObject.Find("Airwall")?.transform;
        if (airwallParent == null)
        {
            Debug.LogError("【玩家】未找到Airwall父物体");
            return;
        }

        Tilemap targetTilemap = null;
        // 优先检查【Airwall自身】是否挂载Tilemap
        Tilemap airwallSelfTilemap = airwallParent.GetComponent<Tilemap>();
        if (airwallSelfTilemap != null && airwallParent.gameObject.activeSelf)
        {
            targetTilemap = airwallSelfTilemap;
            Debug.Log($"【玩家】检测到Airwall根节点直接挂载Tilemap（关卡地图结构）");
        }
        // 如果根节点没有，再遍历子物体
        else
        {
            Debug.Log($"【玩家】Airwall根节点无Tilemap，遍历子物体查找");
            // 遍历所有激活的Tilemap，找到包含目标点的那个，而不是取第一个
            foreach (Transform child in airwallParent.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Debug.Log($"【传送调试】子物体{child.name}未激活，跳过");
                    continue;
                }

                Tilemap tilemap = child.GetComponent<Tilemap>();
                if (tilemap == null)
                {
                    Debug.Log($"【传送调试】子物体{child.name}不是Tilemap，跳过");
                    continue;
                }

                // 计算当前Tilemap的世界边界
                tilemap.CompressBounds();
                Bounds bound = tilemap.localBounds;
                bound.min = tilemap.transform.TransformPoint(bound.min);
                bound.max = tilemap.transform.TransformPoint(bound.max);

                Debug.Log($"【传送调试】检查Tilemap[{child.name}]：世界边界Min({bound.min.x:F2}, {bound.min.y:F2}) Max({bound.max.x:F2}, {bound.max.y:F2})");
                Debug.Log($"【传送调试】目标点是否在该Tilemap内：{bound.Contains(targetPoint)}");

                // 找到包含目标点的Tilemap，直接跳出循环
                if (bound.Contains(targetPoint))
                {
                    targetTilemap = tilemap;
                    Debug.Log($"【传送调试】找到匹配的Tilemap：{child.name}");
                    break;
                }
            }
        }

        // 未找到任何Tilemap
        if (targetTilemap == null)
        {
            Debug.LogError("【玩家】未找到任何有效Tilemap！");
            return;
        }

        // 计算边界
        targetTilemap.CompressBounds();
        Bounds roomBounds = targetTilemap.localBounds;
        roomBounds.min = targetTilemap.transform.TransformPoint(roomBounds.min);
        roomBounds.max = targetTilemap.transform.TransformPoint(roomBounds.max);


        // 判断目标点是否在这个地图范围内
        if (roomBounds.Contains(targetPoint))
        {
            _cellSize = targetTilemap.cellSize;
            // 计算最终边界（独立地图，不合并）
            _mapBounds = new Bounds();
            _mapBounds.min = roomBounds.min + _cellSize;
            _mapBounds.max = roomBounds.max - _cellSize;

            // 打印新地图边界
            Debug.Log($"【目标点匹配成功】地图边界更新完成");
            Debug.Log($"【新地图边界】左：{_mapBounds.min.x:F2} | 右：{_mapBounds.max.x:F2} | 下：{_mapBounds.min.y:F2} | 上：{_mapBounds.max.y:F2}");

            // 更新相机边界
            if (CameraControlSystem.Instance)
                CameraControlSystem.Instance.UpdateMapBounds(_mapBounds);
        }
        else
        {
            Debug.LogError("【玩家】目标点不在当前地图范围内！");
            Debug.LogError($"目标点：{targetPoint} | roomBounds：Min({roomBounds.min}, {roomBounds.max})");
        }
    }
}