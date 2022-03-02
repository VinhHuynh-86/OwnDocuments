
void MainLight_half(float3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten, out half Attention)
{
#if SHADERGRAPH_PREVIEW
   Direction = half3(0.5, 0.5, 0);
   Color = half3(1, 1, 1);
   DistanceAtten = 1;
   ShadowAtten = 1;
   Attention = 0.5;
#else
#if SHADOWS_SCREEN
   half4 clipPos = TransformWorldToHClip(WorldPos);
   half4 shadowCoord = ComputeScreenPos(clipPos);
#else
   half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
   Light mainLight = GetMainLight(shadowCoord);
   Direction = mainLight.direction;
   Color = mainLight.color;
   DistanceAtten = mainLight.distanceAttenuation;
   ShadowAtten = mainLight.shadowAttenuation;
   Attention = MainLightRealtimeShadow(shadowCoord);
#endif
}

void DirectSpecular_half(half3 Specular, half Smoothness, half3 Direction, half3 Color, half3 WorldNormal, half3 WorldView, out half3 Out)
{
#if SHADERGRAPH_PREVIEW
   Out = 0;
#else
   Smoothness = exp2(10 * Smoothness + 1);
   WorldNormal = normalize(WorldNormal);
   WorldView = SafeNormalize(WorldView);
   Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, half4(Specular, 0), Smoothness);
#endif
}

void BlinnPhongLight_half (half SpecularValue, half GlossValue,half3 LightColor, half3 SpecularColor, half3 Albedo ,half3 WorldNormal ,half3 ViewDir, half3 LightDir, out half4 Out)
{
    half3 h = normalize (LightDir + ViewDir);

    half diff = max (0, dot (WorldNormal, LightDir));

    float nh = max (0, dot (WorldNormal, h));
    float spec = pow (nh, SpecularValue*128.0) * GlossValue;

    //Out.rgb = LightColor.rgb * (Albedo * diff + SpecularColor * spec);
    Out.rgb = LightColor.rgb * (Albedo * diff + SpecularColor * spec);
    Out.a = 1.0;

    //Out.rgb = Albedo;
}

void MainLightShadowStrength_half(out half ShadowStrength)
{
   #if SHADERGRAPH_PREVIEW
   ShadowStrength = 1.0;
   #else
   // ShadowParams
   // x: ShadowStrength
   // y: 1.0 if shadow is soft, 0.0 otherwise
   ShadowStrength = GetMainLightShadowStrength();
   #endif
}