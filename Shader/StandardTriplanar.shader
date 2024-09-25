Shader "Custom/StandardTriplanar"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}

        _Scale ("Texture Scale", Range(0.1, 10)) = 1
    }

     SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma shader_feature _NORMALMAP

        sampler2D _MainTex;

        half _BumpScale;
        sampler2D _BumpMap;

        float _Scale;

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
            INTERNAL_DATA float3 worldNormal;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
        }
            
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate absolute vertex normal in world space
            float3 absNormal = abs(IN.worldNormal);

            // Calculate valueOne
            float3 valueOne = float3(0,0,1);
            if (absNormal.r > absNormal.g && absNormal.r > absNormal.b)
            {
                valueOne = float3(1, 0, 0); // Project from X axis
            }
            else if (absNormal.g > absNormal.r && absNormal.g > absNormal.b)
            {
                valueOne = float3(0, 1, 0); // Project from Y axis
            }
            else if (absNormal.b > absNormal.r && absNormal.b > absNormal.g)
            {
                valueOne = float3(0, 0, 1); // Project from Z axis
            }

            // Separate absolute world position into float2 variables
            float2 GB = float2(IN.worldPos.g, IN.worldPos.b);
            float2 RB = float2(IN.worldPos.r, IN.worldPos.b);
            float2 RG = float2(IN.worldPos.r, IN.worldPos.g);

            // Calculate products
            float2 productOne = valueOne.r * GB;
            float2 productTwo = valueOne.g * RB;
            float2 productThree = valueOne.b * RG;

            // Combine the products and multiply with the scale parameter
            float2 triplanarUV = productOne + productTwo + productThree;
            triplanarUV *= _Scale;

            // Sample textures
            fixed4 albedo = tex2D(_MainTex, triplanarUV);
            fixed4 normal = tex2D(_BumpMap, triplanarUV);

            o.Albedo = albedo.rgb;
            o.Alpha = albedo.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
