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
                float4x4 ViewProjectionMatrix;
            };

            struct VSInput {
                [[vk::location(0)]]
                float3 a_Position : POSITION0;

                [[vk::location(1)]]
                float2 a_TexCoord : TEXCOORD0;
            };

            struct VSOutput {
                [[vk::location(0)]]
                float4 Position : POSITION0;
                
                [[vk::location(1)]]
                float2 TexCoord : TEXCOORD0;
            };

            class PushConstants {
                float4x4 Transform;
            };
            
            class Settings {
//                float4x4 Transform;

                float Scale;
                float Size;
            };

            class Fill1 { float palceholder; };
            class Fill2 { float palceholder; };
            
            [[vk::binding(0, 0)]]
            ConstantBuffer<Fill1> _fill1;
            
            [[vk::binding(0, 1)]]
            ConstantBuffer<Fill2> _fill2;

            [[vk::binding(0, 2)]]
            ConstantBuffer<Camera> u_Camera;
            
            [[vk::push_constant]]
            ConstantBuffer<PushConstants> u_Renderer;
            
            [[vk::push_constant]]
            ConstantBuffer<Settings> u_Settings;
            
            
            VSOutput vert(VSInput input) {
                VSOutput output = (VSOutput)0;
                //output.Position = u_Renderer.Transform * float4(input.a_Position, 1.0);
                output.Position = u_Camera.ViewProjectionMatrix * u_Renderer.Transform * float4(input.a_Position, 1.0);
//                output.Position = float4(input.a_Position, 1.0);
                output.TexCoord = input.a_TexCoord;

                return output;
            }

            struct InputFromFrag {
                [[vk::location(1)]]
                float2 TexCoord;
            };

            float4 frag(in InputFromFrag input) : COLOR {
                return float4(1, 1 * u_Settings.Scale, 0, 1);
//                return float4(1, 1, 0, 1);
            }
            ENDHLSL
        }
    }
}