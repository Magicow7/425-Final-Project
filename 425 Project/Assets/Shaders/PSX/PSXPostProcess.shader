Shader "CustomPSX/PSXPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", float) = 8.0
        _DitherIntensity ("Dither Intensity", float) = 1.0
        _ColorsPerChannel ("Colors per channel", float) = 8.0
        _FogRange ("Fog Range", Vector) = (1.0,5.0,0,0)
        _FogColor ("Fog Color", Color) = (0.5,0.5,0.5,1)
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
            float _PixelSize;
            float _DitherIntensity;
            float _ColorsPerChannel;
            float2 _FogRange;
            float4 _FogColor;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float ClampColor(float inVal){
                return (floor(inVal * (_ColorsPerChannel - 1) + 0.5))/(_ColorsPerChannel - 1);
            }

            

            fixed4 frag (v2f i) : SV_Target
            {
                //pixelate
                //float2 pixelatedUV = i.uv;
                // Read the depth at the current UV
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);
                float2 pixelatedUV = floor(i.uv * screenSize / _PixelSize) * _PixelSize / screenSize;
                float4 color = tex2D(_MainTex, pixelatedUV);

                

                //dither

                // Sample the main texture
                float bayerMatrix[16] =
			    {
                    0.0, 0.5, 0.125, 0.625,
                    0.75, 0.25, 0.875, 0.375,
                    0.188, 0.688, 0.062, 0.562,
                    0.938, 0.438, 0.812, 0.312
                };

                // determine position in matrix
                int x = int(fmod(pixelatedUV.x * _ScreenParams.x/_PixelSize, 4));
                int y = int(fmod(pixelatedUV.y * _ScreenParams.y/_PixelSize, 4));

                float threshold = bayerMatrix[(4*x) + y];


                // Apply dithering by comparing color intensity with the threshold
                color.r *= color.r + threshold * _DitherIntensity;
                color.g *= color.g + threshold * _DitherIntensity;
                color.b *= color.b + threshold * _DitherIntensity;

                //apply color clamp
                color.r = ClampColor(color.r);
                color.g = ClampColor(color.g);
                color.b = ClampColor(color.b);

                float ratio = smoothstep(_FogRange.x, _FogRange.y, depth);
                //return ratio;
                fixed4 finalColor = color * ratio + _FogColor * (1-ratio);



                return finalColor;
            }
            ENDCG
        }
    }
}
