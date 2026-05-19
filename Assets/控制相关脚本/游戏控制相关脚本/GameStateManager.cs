using UnityEngine;

/// <summary>
/// 状态机
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // 单例
    public static GameStateManager Instance { get; private set; }

    // 游戏状态枚举
    public enum GameState
    {
        GamePlay,   // 正常游玩
        UI,         // 打开UI
        Cutscene,   // 过剧情
        Pause,      // 暂停
        Loading     // 加载
    }

    // 仅标记是否在关卡
    private bool _isInLevel = false;
    public bool IsInLevel => _isInLevel;
    public void SetInLevel(bool value = true)
    {
        _isInLevel = value;
    }

    // 状态切换事件
    public static event System.Action<GameState> OnStateChanged;
    // UI事件
    public static event System.Action OnUIOpened;
    public static event System.Action OnUIClosed;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始状态：正常游戏
        ChangeState(GameState.GamePlay);
    }

    // 全局切换游戏状态
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log($"游戏状态切换：{CurrentState}");

        // 广播事件
        OnStateChanged?.Invoke(newState);
    }

    // 切换为UI状态
    public void SwitchToUIState()
    {
        ChangeState(GameState.UI);
        OnUIOpened?.Invoke();
        Debug.Log("状态 → UI");
    }

    /// 切回游戏状态
    public void SwitchToGamePlayState()
    {
        ChangeState(GameState.GamePlay);
        OnUIClosed?.Invoke();
        Debug.Log("状态 → 游戏游玩");
    }
}