Shader "Custom/SpriteClippingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginSize ("Origin Size", Vector) = (32, 32, 0, 0)
        _ClipSize ("Clip Size", Vector) = (32, 32, 0, 0)   // 裁剪区域的像素尺寸
        _SpriteRect ("Sprite Rect", Vector) = (0, 0, 32, 32) // Sprite 在图集中的局部区域 (x, y, width, height)，绝对像素
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // UV 坐标范围是 (0, 0) 到 (1, 1)
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _OriginSize;
            float4 _ClipSize; // 裁剪区域的像素尺寸 (width, height, 0, 0)
            float4 _SpriteRect; // Sprite 在图集中的局部区域 (x, y, width, height)，绝对像素

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // 传递 UV 坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 将 UV 坐标转换为 Sprite 的局部像素坐标
                float2 spritePixelCoord = i.uv * _OriginSize.xy;

                // 计算裁剪区域的边界
                float2 clipCenter = _SpriteRect.xy + _SpriteRect.zw * 0.5; // Sprite 中心
                float2 clipMin = clipCenter - _ClipSize.xy * 0.5;
                float2 clipMax = clipCenter + _ClipSize.xy * 0.5;

                // 裁剪超出范围的部分
                if (spritePixelCoord.x < clipMin.x || spritePixelCoord.x > clipMax.x || 
                    spritePixelCoord.y < clipMin.y || spritePixelCoord.y > clipMax.y)
                    discard;

                // 采样纹理
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < 1)
                    discard;
                return col;
            }
            ENDCG
        }
    }
}