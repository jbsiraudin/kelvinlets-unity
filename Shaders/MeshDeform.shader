Shader "Custom/MeshDeform"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0

        _SpringForce("Spring Force", float) = 20f
        _Damping("Damping", float) = 5f
        _UniformScale("Scale", float) = 1f

        _ModelOrigin("Model Origin", Vector) = (0,0,0,0)
        _ImpactOrigin("Impact Origin", Vector) = (-5,0,0,0)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows addshadow vertex:vert

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;

            float _SpringForce;
            float _Damping;
            float _UniformScale;

            float4 _ModelOrigin;
            float4 _ImpactOrigin;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            void vert(inout appdata_base v) {
                float4 world_space_vertex = mul(unity_ObjectToWorld, v.vertex);

                float4 direction = normalize(_ModelOrigin - _ImpactOrigin);
                float4 origin = _ImpactOrigin + _ControlTime * _ImpactSpeed * direction;

                //Get the distance in world space from our vertex to the wave origin.
                float dist = distance(world_space_vertex, origin);

                //Adjust our distance to be non-linear.
                dist = pow(dist, _WaveFalloff);

                //Set the max amount a wave can be distorted based on distance.
                dist = max(dist, _MaxWaveDistortion);

                //Convert direction and _ImpactOrigin to model space for later trig magic.
                float4 l_ImpactOrigin = mul(unity_WorldToObject, _ImpactOrigin);
                float4 l_direction = mul(unity_WorldToObject, direction);

                //Magic
                float impactAxis = l_ImpactOrigin + dot((v.vertex - l_ImpactOrigin), l_direction);

                v.vertex.xyz += v.normal * sin(impactAxis * _Frequency + _ControlTime * _WaveSpeed) * _Amplitude * (1 / dist);
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                // Metallic and smoothness come from slider variables
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
