Shader "NAMSAN/Layered World Wall"
{
    Properties
    {
        _ColorTex("Surface color (sRGB)", 2D) = "white" {}
        _NormalTex("Surface normal RGB (linear)", 2D) = "bump" {}
        _MaskTex("R: AO  G: Roughness  B: Height (linear)", 2D) = "white" {}
        _StainTex("R: Runoff  G: Damp  B: Dirt (linear)", 2D) = "gray" {}
        _Tint("Surface tint", Color) = (1,1,1,1)
        _DirtTint("Dirt tint", Color) = (0.25,0.24,0.20,1)
        _TileMeters("Micro texture repeat (metres)", Range(.5,4)) = 2
        _NormalStrength("Surface relief", Range(0,2)) = 0.7
        _RoughnessOffset("Roughness adjustment", Range(-.3,.3)) = 0
        _StainStrength("Runoff strength", Range(0,1)) = .34
        _DampStrength("Lower wall damp", Range(0,1)) = .38
        _FloorHeight("Floor world height", Float) = 0
        _CeilingHeight("Ceiling world height", Float) = 3.1
        // Opaque URP depth/shadow passes use these standard properties.
        [HideInInspector] _BaseMap("Base map",2D)="white" {}
        [HideInInspector] _BaseColor("Base color",Color)=(1,1,1,1)
        [HideInInspector] _Cutoff("Cutoff",Float)=.5
        [HideInInspector] _Cull("Cull",Float)=2
        [HideInInspector] _ZWrite("ZWrite",Float)=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardWall"
            Tags { "LightMode"="UniversalForwardOnly" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
            TEXTURE2D(_ColorTex); SAMPLER(sampler_ColorTex);
            TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);
            TEXTURE2D(_StainTex); SAMPLER(sampler_StainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Tint, _DirtTint;
                float _TileMeters, _NormalStrength, _RoughnessOffset;
                float _StainStrength, _DampStrength, _FloorHeight, _CeilingHeight;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 lightmapUV:TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float3 positionWS:TEXCOORD0;
                half3 normalWS:TEXCOORD1;
                half4 fogAndVertexLight:TEXCOORD2;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV,vertexSH,3);
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                Varyings output=(Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input,output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=p.positionCS;
                output.positionWS=p.positionWS;
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.fogAndVertexLight=half4(ComputeFogFactor(p.positionCS.z),VertexLighting(p.positionWS,output.normalWS));
                OUTPUT_LIGHTMAP_UV(input.lightmapUV,unity_LightmapST,output.lightmapUV);
                OUTPUT_SH(output.normalWS,output.vertexSH);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 n=normalize(input.normalWS);
                float3 p=input.positionWS;
                float3 t,b;
                // Axis projection suits the existing planar walls and prevents stretched cube UVs.
                // Opposite faces use a consistent handed tangent basis, avoiding mirrored normal lighting.
                if(abs(n.y)>.707) { t=float3(1,0,0); b=float3(0,0,-sign(n.y)); }
                else if(abs(n.x)>abs(n.z)) { t=float3(0,0,-sign(n.x)); b=float3(0,1,0); }
                else { t=float3(sign(n.z),0,0); b=float3(0,1,0); }
                float2 meters=float2(dot(p,t),dot(p,b));
                float2 uv=meters/max(_TileMeters,.1);
                half3 color=SAMPLE_TEXTURE2D(_ColorTex,sampler_ColorTex,uv).rgb*_Tint.rgb;
                half3 mask=SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,uv).rgb;
                half3 normalTS=SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv).xyz*2-1;
                normalTS.xy*=_NormalStrength;
                float3 normalWS=normalize(t*normalTS.x+b*normalTS.y+n*max(normalTS.z,.1));
                // Large stains are sampled independently of the 2m micro surface.
                // World height anchors damp to the floor and leaks to the ceiling rather than each wall segment.
                float horizontal=abs(n.x)>abs(n.z)?p.z:p.x;
                float height=p.y-_FloorHeight;
                float2 stainUV=float2(horizontal/16+p.z*.013,height/3.2);
                half3 stains=SAMPLE_TEXTURE2D(_StainTex,sampler_StainTex,stainUV).rgb;
                half3 variation=SAMPLE_TEXTURE2D(_StainTex,sampler_StainTex,stainUV*float2(.41,.83)+float2(.37,.19)).rgb;
                float wallWeight=1-smoothstep(.3,.8,abs(n.y));
                float lower=(1-smoothstep(.04,.85,height+variation.g*.25))*wallWeight;
                float top=smoothstep(_CeilingHeight-1.6,_CeilingHeight-.05,p.y)*wallWeight;
                float runoff=stains.r*(.25+variation.r*.75)*top*_StainStrength;
                float damp=lower*(.25+stains.b*.75)*_DampStrength;
                float dirt=saturate(runoff+damp);
                color=lerp(color,color*_DirtTint.rgb,dirt);
                color*=lerp(.92,1.035,variation.g);
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=color;
                surface.metallic=0;
                surface.specular=half3(.04,.04,.04);
                surface.smoothness=1-saturate(mask.g+_RoughnessOffset-damp*.18);
                surface.normalTS=half3(0,0,1);
                surface.occlusion=mask.r;
                surface.alpha=1;
                InputData lighting=(InputData)0;
                lighting.positionWS=p;
                lighting.positionCS=input.positionCS;
                lighting.normalWS=normalWS;
                lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(p);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    lighting.shadowCoord=ComputeScreenPos(TransformWorldToHClip(p));
                #else
                    lighting.shadowCoord=TransformWorldToShadowCoord(p);
                #endif
                lighting.fogCoord=input.fogAndVertexLight.x;
                lighting.vertexLighting=input.fogAndVertexLight.yzw;
                lighting.bakedGI=SAMPLE_GI(input.lightmapUV,input.vertexSH,normalWS);
                lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask=SAMPLE_SHADOWMASK(input.lightmapUV);
                half4 result=UniversalFragmentPBR(lighting,surface);
                result.rgb=MixFog(result.rgb,lighting.fogCoord);
                return result;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
