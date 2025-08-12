Shader "Custom/UserMaskShader"
{
    Properties
    {
        _MainTex ("Camera Texture", 2D) = "white" {}
        _UserMask ("User Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _UserMask;
            float4 _MainTex_ST;
            float4 _UserMask_ST;

            StructuredBuffer<uint> _DepthFrame;
            int _textureWidth;
            int _textureHeight;
            float _maxDepthSensor;
            float4 _CameraPosition;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMain : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // Índice de profundidad
                int x = (int)(v.uv.x * _textureWidth);
                int y = (int)(v.uv.y * _textureHeight);
                uint rawIndex = y * _textureWidth + x;

                uint depthPairVal = _DepthFrame[rawIndex >> 1];
                uint depthVal = rawIndex % 2 != 0 ? depthPairVal >> 16 : (depthPairVal & 0xFFFF);

                float depth = 1.0 - (float(depthVal) / (_maxDepthSensor * 1000));
                if (depth == 1.0) depth = 0.0;

                // Aplicar desplazamiento en profundidad
                float4 deltaCam = _CameraPosition - v.vertex;
                float4 shiftToCamera = normalize(deltaCam) * depth;
                float4 newVertex = v.vertex + shiftToCamera * length(deltaCam);

                o.vertex = UnityObjectToClipPos(newVertex);

                // UVs de cámara y máscara
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                float2 uvMask = TRANSFORM_TEX(v.uv, _UserMask);
                uvMask.y = 1.0 - uvMask.y; // Flip solo en máscara
                o.uvMask = uvMask;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float user = tex2D(_UserMask, i.uvMask).r;
                clip(user - 0.05); // solo píxeles con usuario

                float4 color = tex2D(_MainTex, i.uvMain).bgra;
                return color;
            }
            ENDCG
        }
    }
}
