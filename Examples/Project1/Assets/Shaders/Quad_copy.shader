shader "Common/Quad" {
    properties {
        _MainTex ("Texture", 2D) = "white"
    }
    
    subshader {
        tags { "RenderType" = "Opaque" }
        LOD 100
        
        pass {
            HLSLPROGRAM

            struct Camera {
                [[vk::location(0)]]
                float4x4 Position;
                
                [[vk::location(1)]]
                float3 Test123;
            };

            struct Camera2 {
                [[vk::location(0)]]
                float4x4 Position;
                
                [[vk::location(1)]]
                float3 Test123;
            };

            struct VSInput {
                [[vk::location(0)]]
                float3 a_Position : POSITION0;

                [[vk::location(1)]]
                float2 a_TexCoord : TEXCOORD0;
            };

            struct VSOutput {
                float4 Pos : SV_POSITION;

                [[vk::location(0)]]
                float2 TexCoord : TEXCOORD0;
            };

            struct PushConstants {
                uint calculateNormals;
                float someVariable;
                float4x4 someMatrix;
            };


//            [[vk::binding(0)]]
//            RWTexture2D<float> u_Texture;
            
            [[vk::binding(0, 0)]]
            ConstantBuffer<Camera2> u_RandomFloat;
            
            [[vk::binding(0, 1)]]
            ConstantBuffer<Camera> u_Camera;
            
            [[vk::push_constant]]
            ConstantBuffer<PushConstants> pushConstants;

            
            VSOutput vert(VSInput input) {
                VSOutput output = (VSOutput)0;
                output.Pos = float4(input.a_Position.xy, 0, 1.0);
                output.Pos = float4(u_RandomFloat.Test123, pushConstants.calculateNormals);
                output.Pos = float4(u_Camera.Test123.x, 0, pushConstants.someVariable, u_RandomFloat.Test123.x);
                output.TexCoord = input.a_TexCoord;

                return output;
            }

            struct InputFromFrag {
                float2 TexCoord;
            };

            float4 frag(in InputFromFrag input) : COLOR {
                return float4(0,0,0,0);
                // return texture(u_Texture, input.TexCoord);
            }
            ENDHLSL
        }
    }
}