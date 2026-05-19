using UnityEngine;
using UnityEngine.Tilemaps;

public class MarkPointTilemapHider : MonoBehaviour
{
    private TilemapRenderer[] _tilemapRenderers;

    private void Awake()
    {
        // 找到所有子物体的TilemapRenderer组件
        _tilemapRenderers = GetComponentsInChildren<TilemapRenderer>(includeInactive: true);

        // 运行时直接禁用所有Renderer，游戏里就不会显示了
        foreach (var renderer in _tilemapRenderers)
        {
            renderer.enabled = false;
        }
    }
}