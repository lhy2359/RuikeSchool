using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    public enum TriggerType
    {
        Teleport,           // 同场景固定出生点传送
        OpenRepairShop,     // 打开维修店UI
        CrossSceneTeleport  // 跨场景随机瓦片出生点
    }

    public TriggerType triggerType;

    [Header("=== 同场景传送设置（仅Teleport用） ===")]
    public Transform targetSpawnPoint;
    public float fadeTime = 1f;

    [Header("=== 跨场景传送设置（仅CrossSceneTeleport用） ===")]
    [SerializeField] private string _targetSceneName;
    [SerializeField] private string _spawnMarkTilemapName = "SpawnMarkPoint";

    private bool isTeleporting = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TriggerSequence(other.transform));
        }
    }

    IEnumerator TriggerSequence(Transform player)
    {
        isTeleporting = true;

        // 打开维修店UI
        if (triggerType == TriggerType.OpenRepairShop)
        {
            UIManager.Instance.OpenRepairShop();
            isTeleporting = false;
            yield break;
        }

        // 同场景传送
        if (triggerType == TriggerType.Teleport)
        {
            InputManager.Instance.SetCanGameInput(false);
            yield return StartCoroutine(UIManager.Instance.FadeIn(fadeTime));

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            if (PlayerControlSystem.Instance != null)
            {
                PlayerControlSystem.Instance.UpdateMapBoundsByTargetPoint(targetSpawnPoint.position);
            }

            player.position = targetSpawnPoint.position;
            Debug.Log($"【同场景传送】强制设置玩家坐标：{player.position:F2}");

            if (CameraControlSystem.Instance != null)
            {
                CameraControlSystem.Instance.TeleportCamera(targetSpawnPoint.position);
            }

            InputManager.Instance.SetCanGameInput(true);
            yield return StartCoroutine(UIManager.Instance.FadeOut(fadeTime));
        }
        // 跨场景传送：只做FadeIn，调用场景系统独立协程
        else if (triggerType == TriggerType.CrossSceneTeleport)
        {
            InputManager.Instance.SetCanGameInput(false);
            yield return StartCoroutine(UIManager.Instance.FadeIn(fadeTime));

            SceneTransitionSystem.Instance.TransitionToNewSceneWithRandomSpawn(
                _targetSceneName,
                _spawnMarkTilemapName,
                fadeTime
            );

            Debug.Log($"【跨场景传送】已请求传送至 {_targetSceneName}");
        }

        isTeleporting = false;
    }
}