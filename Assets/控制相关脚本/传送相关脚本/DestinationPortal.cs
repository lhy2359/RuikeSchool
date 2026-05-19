using UnityEngine;
using System.Collections;

public class DestinationPortal : MonoBehaviour
{
    [Header("淡出时间")]
    public float fadeTime = 1f;

    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 碰到玩家且未触发过
        if (other.CompareTag("Player") && !isTriggered)
        {
            StartCoroutine(OnDestinationReached());
        }
    }

    IEnumerator OnDestinationReached()
    {
        isTriggered = true;
        Debug.Log("玩家已到达终点！关卡结束");

        // 禁用玩家输入
        InputManager.Instance.SetCanGameInput(false);

        // 设置不在关卡里
        GameStateManager.Instance.SetInLevel(false);

        // 黑屏淡入
        yield return StartCoroutine(UIManager.Instance.FadeIn(fadeTime));

        // 打开胜利UI
        // UIManager.Instance.OpenWinPanel();

        // 返回主地图
        SceneTransitionSystem.Instance.TransitionToNewScene("MainMapScene");

        Debug.Log("关卡完成！");

        // 淡出让画面显示
        yield return StartCoroutine(UIManager.Instance.FadeOut(fadeTime));

        // 恢复输入
        InputManager.Instance.SetCanGameInput(true); 
    }
}