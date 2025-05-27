Shader "UI/GaussianBlur" { // ��˹ģ��
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {} // ������
        _BlurSize ("Blur Size", Float) = 1.0 // ģ���ߴ�(���������ƫ����)
    }
 
    SubShader {
        CGINCLUDE
        
        #include "UnityCG.cginc"
        
        sampler2D _MainTex; // ������
        half4 _MainTex_TexelSize; // _MainTex�����سߴ��С, float4(1/width, 1/height, width, height)
        float _BlurSize; // ģ���ߴ�(���������ƫ����)
          
        struct v2f {
            float4 pos : SV_POSITION; // ģ�Ϳռ䶥������
            half2 uv[5]: TEXCOORD0; // 5���������������
        };
          
        v2f vertBlurVertical(appdata_img v) { // ��ֱģ��������ɫ��
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex); // ģ�Ϳռ䶥������任���ü��ռ�, �ȼ���: mul(UNITY_MATRIX_MVP, v.vertex)
            half2 uv = v.texcoord;
            o.uv[0] = uv;
            o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;      
            return o;
        }
        
        v2f vertBlurHorizontal(appdata_img v) { // ˮƽģ��������ɫ��
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex); // ģ�Ϳռ䶥������任���ü��ռ�, �ȼ���: mul(UNITY_MATRIX_MVP, v.vertex)
            half2 uv = v.texcoord;
            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;      
            return o;
        }
 
        fixed4 fragBlur(v2f i) : SV_Target {
            float weight[3] = {0.4026, 0.2442, 0.0545}; // ��СΪ5��һά��˹�ˣ�ʵ��ֻ���¼3��Ȩֵ
            fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weight[0];
            for (int j = 1; j < 3; j++) {
                sum += tex2D(_MainTex, i.uv[j * 2 - 1]).rgb * weight[j]; // �����Ҳ���²������*Ȩֵ
                sum += tex2D(_MainTex, i.uv[j * 2]).rgb * weight[j]; // ���������ϲ������*Ȩֵ
            }
            return fixed4(sum, 1.0);
        }
            
        ENDCG
        
        ZTest Always Cull Off ZWrite Off
        
        Pass {
            NAME "GAUSSIAN_BLUR_VERTICAL"
            
            CGPROGRAM
              
            #pragma vertex vertBlurVertical  
            #pragma fragment fragBlur
              
            ENDCG  
        }
        
        Pass {  
            NAME "GAUSSIAN_BLUR_HORIZONTAL"
            
            CGPROGRAM  
            
            #pragma vertex vertBlurHorizontal  
            #pragma fragment fragBlur
            
            ENDCG
        }
    }
 
    FallBack "Diffuse"
}