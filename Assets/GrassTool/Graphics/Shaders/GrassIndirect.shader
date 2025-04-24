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
        [Toggle(INTERACT_ON)] _INTERACT_ON ("Enable interaction", Float) = 0
        [Toggle(USE_LIGHTNING)] _EnableLight ("Enable lightning", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }

        // Main Pass
        Pass
        {
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            AlphaToMask On 
            
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
            #pragma multi_compile _ INTERACT_ON 
            #pragma multi_compile _ USE_LIGHTNING 

            struct shaderParams
            {
                float3 position;
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

            #if _INTERACT_ON
                float4 _InteractPosition;
            #endif

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

                // Pobranie pozycji swiata
                float3 localPos = v.vertex.xyz;
                float3 worldPos = _ParamsBuffer[ID].position;

                
                #if _INTERACT_ON
                    float distanceToInteract = distance(worldPos,_InteractPosition);
                    distanceToInteract = distanceToInteract/5;
                    distanceToInteract =  distanceToInteract*distanceToInteract*distanceToInteract;
                    localPos *= clamp(distanceToInteract,0.1,1);
                #endif
                

                float3 pos = worldPos.xyz + float3(0, _YCorrection, 0);
                
                // Symulacja wiatru
                float windTime = _Time.y * _WindFrequency;
                float windOffset = sin(pos.x + windTime) * _WindStrength * localPos.y;
                localPos.x += windOffset * v.uv.y;
                
                // Billboardowanie tylko w osi Y
                float3 camPos = _WorldSpaceCameraPos;
                camPos.y = worldPos.y; // Utrzymuje poziom trawy
                float3 camForward = normalize(worldPos.xyz - camPos);
                float3 camRight = normalize(cross(float3(0, 1, 0), camForward));
                float3 camUp = float3(0, 1, 0); // Sta�e ustawienie osi Y
                float3 billboardPos = pos + (camRight * localPos.x) + (camUp * localPos.y);
                
                // Zapobieganie niestabilnemu sortowaniu
                float depthOffset = (v.uv.y * 0.005) - (windOffset * 0.001);
                float4 wpos = float4(billboardPos + float3(0, 0, depthOffset), 1);
                
                float4 vpos = mul(UNITY_MATRIX_V, wpos);
                float4 cpos = mul(UNITY_MATRIX_P, vpos);
                
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
                
                col.rgb = lerp(i.groundColor.rgb, _ColorTop.rgb, i.uv.y * i.uv.y * i.uv.y);
                col.rgb *= textureColor;
               
                #if USE_LIGHTNING
                    col.rgb *= i.lightning;
                #endif
                
                col.a = Unity_Remap(i.uv.y * col.a, _AlphaRemap.xy, _AlphaRemap.zw).x;
                col.a = clamp(col.a, 0, 1);
                
                return col;
            }
            ENDCG
        }
    }
}