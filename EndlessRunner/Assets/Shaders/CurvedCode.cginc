// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


#include "UnityCG.cginc" // for UnityObjectToWorldNormal
#include "UnityLightingCommon.cginc" // for _LightColor0

struct appdata
{
	float4 tangent : TANGENT;
    float3 normal : NORMAL;
    fixed4 color : COLOR;
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	float4 diff : COLOR; // diffuse lighting color
	float4 vertex : SV_POSITION;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _CurveStrengthX;
float _CurveStrengthY;


v2f vert(appdata v)
{
	v2f o;

	float _Horizon = 100.0f;
	float _FadeDist = 50.0f;

	o.vertex = UnityObjectToClipPos(v.vertex);


	float dist = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z);

	o.vertex.y -= _CurveStrengthY * dist * dist * _ProjectionParams.x;
	o.vertex.x -= _CurveStrengthX * dist * dist * _ProjectionParams.y;

	o.uv = TRANSFORM_TEX(v.uv, _MainTex);

	o.diff = v.color;


                o.uv = v.uv;
                // // get vertex normal in world space
                // half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // // dot product between normal and light direction for
                // // standard diffuse (Lambert) lighting
                // half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // // factor in the light color
                // o.diff = nl * _LightColor0;


	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// sample texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // multiply by lighting
                col *= i.diff;

	// sample the texture
	// fixed4 col = tex2D(_MainTex, i.uv) * i.color;
// apply fog
UNITY_APPLY_FOG(i.fogCoord, col);
return col;
}