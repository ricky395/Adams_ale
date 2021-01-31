// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/rim" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Value("value", Range(-10, 10)) = 0
		_RimEffect("Rim effect", Range(0, 1)) = 0
	}
		SubShader{
		Pass{
		Tags{ "Queue" = "Transparent"  "RenderType" = "Transparent" }
		Blend One OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct v2f 
	{
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
		float3 viewDir : TEXCOORD1;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	fixed4 _Color;
	fixed _Value;
	fixed _RimEffect;

	v2f vert(appdata_full v) 
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal.xyz ));
		o.viewDir = normalize(_WorldSpaceCameraPos - mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
		o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

		return o;
	}


	fixed4 frag(v2f i) : COLOR
	{
	float t = tex2D(_MainTex, i.uv);
	float val = abs(dot(i.viewDir, i.normal)) *_RimEffect;

	//clip(_Color.a - _ClipColor);

	return _Color * _Color.a * val *val * t;
	}

		ENDCG
	}
	}
		FallBack "Diffuse"
}