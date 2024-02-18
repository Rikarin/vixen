shader "Common/RenderShader" {
    properties {
        _MainTex ("Texture", 2D) = "white"
    }
    
    subshader {
        tags { "RenderType" = "Opaque" }
        LOD 100
        
        pass {
            HLSLPROGRAM

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

            VSOutput vert(VSInput input) {
                VSOutput output = (VSOutput)0;
                output.Pos = float4(input.a_Position.xy, 0, 1.0);
                output.TexCoord = input.a_TexCoord;

                return output;
            }

            [[vk::binding(0)]]
            RWTexture2D<float> u_Texture;

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