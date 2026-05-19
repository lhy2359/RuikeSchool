using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneTransitionSystem : MonoBehaviour
{
    public static SceneTransitionSystem Instance { get; private set; }

    [Header("基础设置")]
    [SerializeField] private float _transitionDelay = 0.1f;
    [SerializeField] private string _persistentSceneName = "PersistentScene";
    [SerializeField] private string _mainMapSceneName = "MainMapScene";

    private string _currentScene;
    private Vector3 spawnPosition;

    public string GetCurrentScene()
    {
        return _currentScene;
    }

    // 设置当前场景
    public void SetCurrentScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("设置当前场景失败：场景名称不能为空！");
            return;
        }
        if (sceneName == _currentScene)
        {
            Debug.LogError($"设置当前场景失败：目标场景 {sceneName} 已是当前活跃场景，无需重复设置！");
            return;
        }
        if (sceneName == _persistentSceneName)
        {
            Debug.LogError($"设置当前场景失败：不能将持久化场景 {_persistentSceneName} 设置为活跃场景！");
            return;
        }

        _currentScene = sceneName;
        Debug.Log($"已更新当前场景为：{_currentScene}");
        if (_currentScene == "PlainLevelScene")
        {
            PlainLevelSystem.Initialize();
            PlainLevelSystem.Instance.ChooseLeveMode(LevelModeType.ThesisMode);

            PlainLevelSystem.Instance.InitAllMarkPositions();

            // 读取已生成的出生点并暂时存储
            List<Vector3> spawnPositions = PlainLevelSystem.Instance.GetSpawnPositions();
            if (spawnPositions.Count == 1)
            {
                spawnPosition = spawnPositions[0];
                Debug.Log($"读取到出生点：{spawnPosition:F2}");
            }
            else if(spawnPositions.Count > 1)
            {
                Debug.LogError("出生点超过1个！");
            }
            else
            {
                Debug.LogError("没有生成到任何出生点！使用默认位置(0,0,0)");
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetCurrentScene(_mainMapSceneName);
        Debug.Log($"场景切换系统初始化完成，当前场景为 {GetCurrentScene()}");
    }

    // 跨场景传送
    public void TransitionToNewScene(string targetSceneName, string spawnPointTag = "PlayerSpawnPoint")
    {
        if (!IsSceneExists(targetSceneName))
        {
            Debug.LogError($"场景 {targetSceneName} 不存在！");
            return;
        }

        if (SceneManager.GetSceneByName(targetSceneName).isLoaded || targetSceneName == GetCurrentScene())
            return;

        StartCoroutine(LoadNewSceneCoroutine(targetSceneName, spawnPointTag));
    }

    // 加载新场景并传送玩家到指定出生点
    private IEnumerator LoadNewSceneCoroutine(string targetSceneName, string spawnPointTag)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(newScene);

        GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (spawnPoint != null && player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            player.transform.position = spawnPoint.transform.position;

            if (CameraControlSystem.Instance != null)
                CameraControlSystem.Instance.TeleportCamera(player.transform.position);

            if (PlayerControlSystem.Instance != null)
                PlayerControlSystem.Instance.UpdateMapBoundsByTargetPoint(player.transform.position);
        }

        yield return new WaitForSeconds(_transitionDelay);

        if (!string.IsNullOrEmpty(GetCurrentScene()) && GetCurrentScene() != _persistentSceneName)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(GetCurrentScene());
            while (!unloadOp.isDone) yield return null;
        }

        SetCurrentScene(targetSceneName);
    }

    // 跨场景传送并随机生成出生点
    public void TransitionToNewSceneWithRandomSpawn(string targetSceneName, string spawnTilemapName = "SpawnMarkPoint", float fadeTime = 1f)
    {
        StartCoroutine(DoTransitionCoroutine(targetSceneName, spawnTilemapName, fadeTime));
    }
    
    // 跨场景传送
    private IEnumerator DoTransitionCoroutine(string targetSceneName, string spawnTilemapName, float fadeTime)
    {
        Debug.Log($"开始跨场景传送：目标={targetSceneName}，fadeTime={fadeTime}");

        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager.Instance 为 null！");
            InputManager.Instance.SetCanGameInput(true);
            yield break;
        }

        if (!IsSceneExists(targetSceneName))
        {
            Debug.LogError($"场景 {targetSceneName} 未加入Build Settings！");
            yield return StartCoroutine(UIManager.Instance.FadeOut(fadeTime));
            InputManager.Instance.SetCanGameInput(true);
            yield break;
        }

        if (SceneManager.GetSceneByName(targetSceneName).isLoaded || targetSceneName == GetCurrentScene())
        {
            Debug.LogWarning($"目标场景已加载或同场景");
            yield return StartCoroutine(UIManager.Instance.FadeOut(fadeTime));
            InputManager.Instance.SetCanGameInput(true);
            yield break;
        }

        Debug.Log($"开始加载场景 {targetSceneName}");
        yield return LoadNewSceneWithRandomSpawnCoroutine(targetSceneName, spawnTilemapName);
        Debug.Log($"场景加载完成，当前场景={GetCurrentScene()}");

        Debug.Log($"准备执行FadeOut，时长={fadeTime}");
        yield return StartCoroutine(UIManager.Instance.FadeOut(fadeTime));
        Debug.Log($"FadeOut执行完成");

        if (UIManager.Instance.fadeMask != null && UIManager.Instance.fadeMask.color.a > 0.01f)
        {
            Debug.LogWarning($"FadeOut未走完，强制重置遮罩");
            Color c = UIManager.Instance.fadeMask.color;
            c.a = 0;
            UIManager.Instance.fadeMask.color = c;
            UIManager.Instance.fadeMask.gameObject.SetActive(false);
        }

        InputManager.Instance.SetCanGameInput(true);
        Debug.Log($"跨场景传送全部完成");
    }

    // 检查场景是否存在于Build Settings中
    private bool IsSceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    // 加载新场景并随机生成出生点
    private IEnumerator LoadNewSceneWithRandomSpawnCoroutine(string targetSceneName, string spawnTilemapName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        PlayerControlSystem playerCtrl = player.GetComponent<PlayerControlSystem>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (playerCtrl != null) playerCtrl.enabled = false;
        if (rb != null) { rb.simulated = false; rb.velocity = Vector2.zero; }

        // 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        // 卸载旧场景
        if (!string.IsNullOrEmpty(GetCurrentScene()) && GetCurrentScene() != _persistentSceneName)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(GetCurrentScene());
            while (!unloadOp.isDone) yield return null;
        }

        SetCurrentScene(targetSceneName);

        // 读取已生成的出生点并移动玩家
        player.transform.position = spawnPosition;

        if (CameraControlSystem.Instance != null)
            CameraControlSystem.Instance.TeleportCamera(player.transform.position);

        if (PlayerControlSystem.Instance != null)
            PlayerControlSystem.Instance.UpdateMapBoundsByTargetPoint(player.transform.position);

        // 恢复玩家控制
        if (playerCtrl != null) playerCtrl.enabled = true;
        if (rb != null) rb.simulated = true;

        Debug.Log($"场景切换完成！玩家已生成在：{spawnPosition:F2}");
    }
}