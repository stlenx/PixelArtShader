//----------------------------------||
//          Made by sariku          ||
//                                  ||
//          Copyright: Me           ||   
//          Licence: GPL2           ||
//                                  ||
//             For maker.           ||
//----------------------------------||

Shader "Hidden/PixelArt"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //Not modifying anything here
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //Pretty standard stuff
            sampler2D _MainTex;

            //Variables used for pixelation
            int _PixelDensity;
			float2 _AspectRatioMultiplier;

            //Amount of colors to be used
            int _ColorCount;
            //Color array
            uniform fixed4 _Colors[256];

            //This is where the fun begins
            fixed4 frag (v2f i) : SV_Target
            {
                //Pixelating
                const float2 pixelScaling = _PixelDensity * _AspectRatioMultiplier;
                i.uv = round(i.uv * pixelScaling)/ pixelScaling;

                //Color pallet

                //Get current pixel color
                const fixed3 original = tex2D (_MainTex, i.uv).rgb;

                //Initialize final color with just black
	   			fixed4 col = fixed4 (0,0,0,0);
                //Set record distance to something huge
	   			fixed dist = 10000000.0;

                //For each color in the array compare similarity with pixel color
	   			for (int s = 0; s < _ColorCount; s++) {
	   			    //Get color
	   				const fixed4 color = _Colors[s];
	   			    //Calculate the "distance" between the 2 colors (Imagine they're vector3s, you're getting the distance between their end points)
	   				const fixed d = distance(original, color);

	   			    //If the calculated distance is less than the previous record, we found a closer color.
	   				if (d < dist) {
	   					//Update the record distance
	   					dist = d;
	   					//Set the color to this closes one found so far
	   					col = color;
	   				}
	   			}

            	//Return the color
				return col;
            }
            ENDCG
        }
    }
}
