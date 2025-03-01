Shader "Custom/GrassIndirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTop("Color Top", Color) = (1, 1, 1, 1)
        _WindStrength ("Wind Strength", Float) = 1.0
        _WindFrequency ("Wind Frequency", Float) = 1.0
        _YCorrection("Y position correction",Float) = 0.0
        _AlphaRemap("Alpha Remap",Vector) = (0,1,0,1)
    }
     SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest"}

        // Main Pass
        Pass
        {
            ZWrite On
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha 
            AlphaToMask On

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct shaderParams
            {
                float4x4 tranformMatrix;
                float light;
                float4 textureColor;
            };
            StructuredBuffer<shaderParams> _ParamsBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                uint vertexId : SV_VertexID;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float lightning : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 groundColor : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorTop;
            float _WindStrength;
            float _WindFrequency;
            float _YCorrection;
            float4 _AlphaRemap;

            float4 Unity_Remap(float4 In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            v2f vert(appdata v, uint instance_id : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                uint cmdID = GetCommandID(0);
                uint ID = GetIndirectInstanceID(instance_id);

                v2f o;

                //get position
                float3 localPos = v.vertex.xyz;
                float4 worldPos = mul(_ParamsBuffer[ID].tranformMatrix, float4(0, 0, 0, 1));
                float3 pos = worldPos.xyz+float3(0,_YCorrection,0);
                //wind
                float windTime = _Time.y * _WindFrequency;
                float windOffset = sin(pos.x + windTime) * _WindStrength * localPos.y;
                localPos.x += windOffset*v.uv.y;

                //billbord
                float3 camForward = normalize(_WorldSpaceCameraPos - mul(_ParamsBuffer[ID].tranformMatrix, float4(0, 0, 0, 1)).xyz);
                camForward = -camForward;
                float3 camRight = normalize(cross(float3(0, 1, 0), camForward));
                float3 camUp = normalize(cross(camForward, camRight));
                float3 billboardPos = pos + (camRight * localPos.x) + (camUp * localPos.y);
                float4 wpos = float4(billboardPos,1);

                float4 vpos = mul(UNITY_MATRIX_V, wpos);
                float4 cpos = mul(UNITY_MATRIX_P, vpos);

                // assign values
                o.vertex = cpos;
                o.uv = v.uv;
                o.lightning = _ParamsBuffer[ID].light;
                o.worldPos = worldPos.xyz;
                o.groundColor = _ParamsBuffer[ID].textureColor;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uvFix = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                fixed4 textureColor = tex2D(_MainTex, i.uv);
                fixed4 col = float4(1,1,1,textureColor.a);

                col.rgb = lerp(i.groundColor.rgb,_ColorTop.rgb,i.uv.y*i.uv.y*i.uv.y);
                col.rgb *= textureColor;
                col.rgb *= i.lightning;

                col.a = Unity_Remap(i.uv.y*col.a,_AlphaRemap.xy,_AlphaRemap.zw).x;
                col.a = clamp(col.a,0,1);
   
                return col;
            }
            ENDCG
        }
    }
}
