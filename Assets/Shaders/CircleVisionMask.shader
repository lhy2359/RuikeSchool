Shader "Unlit/CircleVisionMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskColor ("遮罩颜色", Color) = (0,0,0,1)
        _PlayerWorldPos ("玩家世界坐标", Vector) = (0,0,0,0)
        _ViewRadius ("视野半径", Float) = 5.0
        _CameraHalfHeight ("相机半高", Float) = 5.0
        _AspectRatio ("屏幕宽高比", Float) = 1.777778
        _EdgeSoftness ("边缘柔化", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1; // 新增：传递世界坐标
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MaskColor;
            float3 _PlayerWorldPos;
            float _ViewRadius;
            float _CameraHalfHeight;
            float _AspectRatio;
            float _EdgeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 计算每个像素的世界坐标
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基于世界坐标计算距离，和相机位置完全无关
                float2 delta = i.worldPos.xy - _PlayerWorldPos.xy;
                
                // 计算到玩家的世界距离（永远是正圆形）
                float distance = length(delta);

                // 计算透明度：距离小于视野半径时透明，大于时显示黑色
                float alpha = smoothstep(_ViewRadius, _ViewRadius + _EdgeSoftness, distance);

                return _MaskColor * alpha;
            }
            ENDCG
        }
    }
}