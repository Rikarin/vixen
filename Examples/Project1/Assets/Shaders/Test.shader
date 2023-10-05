shader "Common/FoobarShader" {
    properties {
        _MainTex ("Texture", 2D) = "white"
    }
    
    subshader {
        tags { "RenderType" = "Opaque" }
        LOD 100
        
        pass {
            HLSLPROGRAM
            #include "Common/Base"

            struct Particle {
                float4 pos;
                float4 vel;
                float4 uv;
                float4 normal;
                float pinned;
            };

            [[vk::binding(8)]]
            StructuredBuffer<Particle> particleIn;
            [[vk::binding(1)]]
            RWStructuredBuffer<Particle> particleOut;


            struct PushConstants {
                uint calculateNormals;
                float someVariable;
                float4x4 someMatrix;
            };



            struct ScreenData {
                float2 InvFullResolution;
                float2 FullResolution;
                float2 InvHalfResolution;
                float2 HalfResolution;
            };
            [[vk::binding(2,2)]] ConstantBuffer<ScreenData> u_ScreenData;

            cbuffer MyContantBuffer : register(b2, space6)
            {
                float4x4 matW;
            }

            struct VSInput
            {
                [[vk::location(7)]]float2 Pos : POSITION0;
                [[vk::location(1)]]float2 UV : TEXCOORD0;
            };

            struct VSOutput
            {
                float4 Pos : SV_POSITION;
                float2 test;
                [[vk::location(4)]]float2 UV : TEXCOORD0;
            };

            [[vk::push_constant]]
            ConstantBuffer<PushConstants> pushConstants;

            void compute() {}

            VSOutput vert(VSInput input)
            {
                VSOutput output = (VSOutput)0;
                output.Pos = float4(input.Pos, pushConstants.calculateNormals, 1.0);
                output.UV = input.UV;
                output.test = u_ScreenData.FullResolution;

                return output;
            }


            struct InputFromFrag
            {
                float3 color : COLOR;
            };

            float4 frag(in InputFromFrag input) : COLOR {
                return float4(input.color, 1.0);
            }

            
            ENDHLSL
        }
    }
}