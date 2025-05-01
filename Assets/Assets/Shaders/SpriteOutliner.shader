Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex          ("Sprite Texture", 2D) = "white" {}
        _OutlineColor     ("Outline Color",    Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness",Float) = 1.0
        _Color            ("Tint Color",       Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "OUTLINE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _OutlineColor;
            float    _OutlineThickness;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = _OutlineThickness * float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y);
                float a0 = tex2D(_MainTex, i.uv).a;
                float a1 = tex2D(_MainTex, i.uv + float2( texel.x, 0)).a;
                float a2 = tex2D(_MainTex, i.uv + float2(-texel.x, 0)).a;
                float a3 = tex2D(_MainTex, i.uv + float2(0,  texel.y)).a;
                float a4 = tex2D(_MainTex, i.uv + float2(0, -texel.y)).a;
                float edge = step(0.01, max(max(max(a0,a1),a2), max(a3,a4))) - a0;
                return _OutlineColor * edge;
            }
            ENDCG
        }

        Pass
        {
            Name "SPRITE"
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag2
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4    _Color;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag2(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                return c * _Color;
            }
            ENDCG
        }
    }
}
