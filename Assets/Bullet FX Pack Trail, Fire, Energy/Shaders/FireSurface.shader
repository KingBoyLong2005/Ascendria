Shader "Custom/FireSurface"
{
    Properties
    {
        _MainTex ("Fire Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "black" {}
        _Speed ("Flame Speed", Float) = 2.0
        _Distortion ("Distortion", Float) = 0.1
        _Brightness ("Brightness", Float) = 5.0
        _AlphaBoost ("Alpha Boost", Float) = 2.0
        _HeightMin ("Min Height", Float) = -1.0
        _HeightMax ("Max Height", Float) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard alpha:blend fullforwardshadows

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        float _Speed;
        float _Distortion;
        float _Brightness;
        float _AlphaBoost;
        float _HeightMin;
        float _HeightMax;
        float _Cutoff;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            
            float time = _Time.y * _Speed;
            float noise = tex2D(_NoiseTex, uv * 2 + time * 0.1).r;
            float wave = sin(time * 2.0 + uv.y * 5.0) * 0.05;

            uv.y += frac(time + noise * 0.5);
            uv.x += sin(time * 3.0) * _Distortion * (noise - 0.5);
            uv.x += wave;

            fixed4 col = tex2D(_MainTex, uv);
            col.rgb *= _Brightness * (0.8 + noise * 0.2);
            
            col.rgb = max(col.rgb, fixed3(0.5, 0.2, 0.0));
            
            float heightFactor = saturate((IN.worldPos.y - _HeightMin) / (_HeightMax - _HeightMin));

            o.Albedo = col.rgb;
            o.Emission = col.rgb;
            o.Alpha = saturate(col.a * _AlphaBoost * heightFactor);

            if (o.Alpha < _Cutoff) discard;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
