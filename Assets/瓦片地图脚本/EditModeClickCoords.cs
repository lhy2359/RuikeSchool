using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditModeClickCoords : MonoBehaviour
{
    private bool _isEnabled = true;
    private KeyCode _toggleKey = KeyCode.F2;
    private Camera _mainCamera;

    [Header("原点标记配置")]
    [SerializeField] private Vector3 _originPos = Vector3.zero;
    [SerializeField] private float _originDotSize = 0.5f;
    [SerializeField] private Color _originDotColor = Color.yellow;

    [Header("相机四角标记配置")]
    [SerializeField] private float _cameraDotSize = 0.3f;
    [SerializeField] private Color _cameraDotColor = new Color(0.9f, 0.9f, 0.9f, 1f);


    void OnEnable()
    {
        _mainCamera = GetComponent<Camera>();
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        SceneView.duringSceneGui += OnSceneGUI;
        Debug.Log("坐标输出功能已开启 | 按 F2 切换开关");
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == _toggleKey)
        {
            _isEnabled = !_isEnabled;
            string status = _isEnabled ? "开启" : "关闭";
            Debug.Log($"坐标输出功能已{status} | 按 F2 切换");
            if (_isEnabled)
            {
                Debug.Log($"静态原点标记已显示（坐标：{_originPos}）");
                Debug.Log($"相机四角标记已显示");
            }
            else
            {
                Debug.Log($"静态原点标记已隐藏");
                Debug.Log($"相机四角标记已隐藏");
            }
            Event.current.Use();
        }

        if (_isEnabled && _mainCamera != null)
        {
            DrawOriginMarker();
            DrawCameraCorners();

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers == EventModifiers.None)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                float distance = 0 - ray.origin.z / ray.direction.z;
                Vector3 worldPos = ray.GetPoint(distance);
                worldPos.z = 0;

                Debug.Log($"精准坐标：X = {worldPos.x:F2}, Y = {worldPos.y:F2}");

                float markSize = 0.5f;
                Debug.DrawLine(worldPos - new Vector3(markSize, 0, 0), worldPos + new Vector3(markSize, 0, 0), Color.red, 10f);
                Debug.DrawLine(worldPos - new Vector3(0, markSize, 0), worldPos + new Vector3(0, markSize, 0), Color.red, 10f);

                e.Use();
            }
        }
    }

    private void DrawOriginMarker()
    {
        Handles.color = _originDotColor;
        Handles.DrawSolidDisc(_originPos, Vector3.forward, _originDotSize);
        Handles.Label(_originPos + new Vector3(1, 1, 0),
            $"原点 ({_originPos.x:F1},{_originPos.y:F1})",
            new GUIStyle()
            {
                normal = { textColor = _originDotColor },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });
        Handles.color = Color.white;
    }

    private void DrawCameraCorners()
    {
        Vector3[] cameraCorners = GetCameraOrthographicCorners();

        Vector3[] textOffsets = new Vector3[]
        {
            new Vector3(-1.2f, -1.2f, 0),
            new Vector3(1.2f, -1.2f, 0),
            new Vector3(1.2f, 1.2f, 0),
            new Vector3(-1.2f, 1.2f, 0)
        };
        string[] cornerNames = new string[] { "左下", "右下", "右上", "左上" };

        Handles.color = _cameraDotColor;
        for (int i = 0; i < 4; i++)
        {
            Vector3 cornerPos = cameraCorners[i];
            cornerPos.z = 0;

            Handles.DrawSolidDisc(cornerPos, Vector3.forward, _cameraDotSize);
            Handles.Label(cornerPos + textOffsets[i],
                $"{cornerNames[i]} ({cornerPos.x:F1},{cornerPos.y:F1})",
                new GUIStyle()
                {
                    normal = { textColor = _cameraDotColor },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                });
        }
        Handles.color = Color.white;

        DrawCameraBorder(cameraCorners);
    }

    private Vector3[] GetCameraOrthographicCorners()
    {
        Vector3[] corners = new Vector3[4];
        float camHeight = _mainCamera.orthographicSize * 2;
        float camWidth = camHeight * _mainCamera.aspect;

        Vector3 camCenter = _mainCamera.transform.position;
        camCenter.z = 0;

        corners[0] = camCenter - new Vector3(camWidth / 2, camHeight / 2, 0);
        corners[1] = camCenter + new Vector3(camWidth / 2, -camHeight / 2, 0);
        corners[2] = camCenter + new Vector3(camWidth / 2, camHeight / 2, 0);
        corners[3] = camCenter - new Vector3(camWidth / 2, camHeight / 2, 0);

        return corners;
    }

    private void DrawCameraBorder(Vector3[] corners)
    {
        Handles.color = new Color(_cameraDotColor.r, _cameraDotColor.g, _cameraDotColor.b, 0.2f);
        Handles.DrawLine(corners[0], corners[1]);
        Handles.DrawLine(corners[1], corners[2]);
        Handles.DrawLine(corners[2], corners[3]);
        Handles.DrawLine(corners[3], corners[0]);
        Handles.color = Color.white;
    }
}