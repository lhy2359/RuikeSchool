using UnityEngine;

[ExecuteInEditMode]
public class MapGridHelper : MonoBehaviour
{
    [Header("===== 网格整体起点偏移（对齐你的地图左下角） =====")]
    [SerializeField] private float _startOffsetX = 0f;
    [SerializeField] private float _startOffsetY = 0f;

    [Header("===== 整张地图瓦片大小 =====")]
    [SerializeField] private int _mapWidth = 100;
    [SerializeField] private int _mapHeight = 100;

    [Header("===== 每个小区块格子数 =====")]
    [SerializeField] private int _blockSize = 10;

    [Header("===== 网格样式 =====")]
    [SerializeField] private Color _lineColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private bool _showGrid = true;

    private void OnEnable() { }
    private void OnDisable() { }

    private void OnDrawGizmos()
    {
        // 🔥 关键：脚本禁用 或 关闭显示，都不绘制网格
        if (!enabled || !_showGrid)
            return;

        Gizmos.color = _lineColor;

        for (int x = 0; x <= _mapWidth; x += _blockSize)
        {
            float worldX = _startOffsetX + x;
            Vector3 start = new Vector3(worldX, _startOffsetY, 0);
            Vector3 end = new Vector3(worldX, _startOffsetY + _mapHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= _mapHeight; y += _blockSize)
        {
            float worldY = _startOffsetY + y;
            Vector3 start = new Vector3(_startOffsetX, worldY, 0);
            Vector3 end = new Vector3(_startOffsetX + _mapWidth, worldY, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}