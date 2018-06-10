Shader "SuperPop/Character/PlayerRole_Normal" 
{
	Properties 
	{   
        _NotVisibleColor ("NotVisibleColor (RGB)", Color) = (1,1,1,1)
        _MainTex ("固有色贴图", 2D) = "white" {}
        _Illum ("高光贴图(R通道是反射，G通道是高光)", 2D) = "white" {}
        _SpecularColor ("高光颜色", Color) = (0.9191176,0.9191176,0.9191176,1)
        _Shininess ("高光强度", Range (0.03, 1)) = 0.078125
        _ShininessRange ("高光范围", Range (5, 200)) = 10
        _Normal ("法线贴图", 2D) = "bump" {}
        //_AmbientColor ("环境光颜色", Color) = (0.5,0.5,0.5,1)
        _RimLightColor ("边光颜色", Color) = (0.5,0.5,0.5,1)
			  _BackLightDirXYZGlossW ("边光方向(W是边光强度)", Vector) = (-1.3,0.3,1.5,0.7) 
			  _NormalIntensity ("法线强度系数", Range (0, 6)) = 1      		
	}
	
	SubShader 
	{
		//Tags { "IgnoreProjector"="True" "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
        //Lighting Off

       Pass
		{
            Tags { "LightMode" = "ForwardBase"}
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater
			ZWrite Off
			Lighting off	
            SetTexture [_NoTex] { ConstantColor [_NotVisibleColor] combine constant * texture  }	
		}
		
        Pass
		{
			ZTest Less
			Lighting On
			Blend SrcAlpha OneMinusSrcAlpha          
		}
		CGPROGRAM
		#pragma surface surf PlayerShader 
		//#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Normal;
    sampler2D _Illum;
    fixed4 _SpecularColor;
		fixed _Shininess;
		half _ShininessRange;
    //fixed4 _AmbientColor;
    half4 _BackLightDirXYZGlossW;
    fixed4 _RimLightColor;
    half _NormalIntensity;
		
		/*struct SurfaceOutputSkin
		{
		    fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            fixed3 Specular;
            fixed Alpha;
            float IllumG;
		};*/
		
		struct Input 
		{
			half2 uv_MainTex;
            half2 uv_Normal;
            half2 uv_Illum;
		};
		
	    inline fixed4 LightingPlayerShader (SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
        {
            viewDir = normalize ( viewDir );
            lightDir = normalize ( lightDir );
            fixed3 halfDirection = normalize ( lightDir + viewDir );			
			half NdotL = max(0, dot(  s.Normal, lightDir ));

            //float3 specularColor = _SpecularColor.rgb*s.IllumG*5.0;
            half3 directSpecular = _LightColor0.xyz * pow(max(0,dot(halfDirection,s.Normal)),_ShininessRange);
            half3 specular = directSpecular * s.Specular * _Shininess;

            //float3 indirectDiffuse = _AmbientColor.rgb;
            //float3 directDiffuse = NdotL * _LightColor0.rgb;
            half3 diffuse = (NdotL * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.xyz * 4) * s.Albedo;

            half normalDotView =1.0 - max(0,dot(s.Normal, viewDir));
            half3 emissive = dot(saturate(2.0*_BackLightDirXYZGlossW.rgb*s.Normal), normalDotView) *_RimLightColor.rgb;
					
            fixed4 c;
            c.rgb =  diffuse + specular*_NormalIntensity + emissive*_BackLightDirXYZGlossW.a*0.3;
            c.a = s.Alpha;
            return c;
        }		

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 normals = UnpackNormal(tex2D(_Normal, IN.uv_Illum));
            fixed3 illum = tex2D(_Illum, IN.uv_MainTex);	
			
			o.Normal = normalize(normals);
			o.Albedo = c.rgb*0.2;
			o.Specular = _SpecularColor.rgb*illum.g*5.0;
            //o.IllumG = illum.g;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
