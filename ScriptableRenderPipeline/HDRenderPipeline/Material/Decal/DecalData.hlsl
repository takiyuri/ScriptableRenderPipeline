﻿//-------------------------------------------------------------------------------------
// Fill SurfaceData/Builtin data function
//-------------------------------------------------------------------------------------
#include "../../../Core/ShaderLibrary/SampleUVMapping.hlsl"
#include "../MaterialUtilities.hlsl"


void GetSurfaceAndBuiltinData(FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
	float3 positionWS = posInput.positionWS;
	float3 positionDS = mul(_WorldToDecal, float4(positionWS, 1.0f)).xyz;
	clip(positionDS < 0 ? -1 : 1);
	clip(positionDS > 1 ? -1 : 1);

	surfaceData.baseColor = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, positionDS.xz); 
	surfaceData.baseColor.w *= _DecalBlend;
	UVMapping texCoord;
	ZERO_INITIALIZE(UVMapping, texCoord);
	ZERO_INITIALIZE(BuiltinData, builtinData);
	texCoord.uv = positionDS.xz;
	//surfaceData.normalWS.xyz = mul((float3x3)_DecalToWorldR, SAMPLE_UVMAPPING_NORMALMAP(_NormalMap, sampler_NormalMap, texCoord, 1)) * 0.5f + 0.5f;
	surfaceData.normalWS.xyz = SAMPLE_UVMAPPING_NORMALMAP(_NormalMap, sampler_NormalMap, texCoord, 1) * 0.5f + 0.5f;
	surfaceData.normalWS.w = surfaceData.baseColor.w;
}
