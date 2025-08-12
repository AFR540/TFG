Shader "Unlit/ARNuitrack_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform StructuredBuffer<uint> _DepthFrame : register(t1);
            int _textureWidth;
            int _textureHeight;

            float _maxDepthSensor;

            float4 _CameraPosition;

            v2f vert (appdata v)
            {
                v2f o;

                uint rawIndex = _textureWidth * (v.uv.y * _textureHeight) + (v.uv.x * _textureWidth);
                uint depthPairVal = _DepthFrame[rawIndex >> 1];

                uint depthVal  = rawIndex % 2 != 0 ? depthPairVal >> 16 : (depthPairVal << 16) >> 16;

                float depth = 1 - (float(depthVal) / (_maxDepthSensor * 1000));

                if (depth == 1)
                    depth = 0;

                    float4 deltaCam = _CameraPosition - v.vertex;
                    float4 shiftToCamera = normalize(deltaCam) * depth;
                    float4 newVertex = v.vertex + shiftToCamera * length(deltaCam);

                o.vertex = UnityObjectToClipPos(newVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv).bgra;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
