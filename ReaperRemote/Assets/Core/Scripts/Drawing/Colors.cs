using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    // https://csharp.hotexamples.com/examples/UnityEngine/Color32/-/php-color32-class-examples.html
    // https://github.com/happyjiahan/colorus

    private static Color32 winter = new Color32(246, 243, 237, 255);
    public static Color32 Winter {get => winter;}
    private static Color32 spring = new Color32(170, 155, 42, 255);
    public static Color32 Spring {get => spring;}
    private static Color32 summer = new Color32(137, 182, 65, 255);
    public static Color32 Summer {get => summer;}
    private static Color32 autumn = new Color32(234, 157, 46, 255);
    public static Color32 Autumn {get => autumn;}
    private static Color32 black = new Color32(0,0,0,255);
    public static Color32 Black {get => black;}
    private static Color32 white = new Color32(255,255,255,255);
    public static Color32 White {get => white;}

    // https://docs.unity3d.com/ScriptReference/Color.RGBToHSV.html
    // https://docs.unity3d.com/ScriptReference/Color.HSVToRGB.html

    //https://forum.unity.com/threads/mixing-two-objects-color-creating-the-color-of-the-two-mixes.490431/
    // https://gamedev.stackexchange.com/questions/177209/what-colour-space-or-mixing-algorithm-should-i-use-for-watercolor-like-colour

    // On first glance I would say:
    //Create 3 ints r, g and b.
    //Add together the corresponding value from each colour. int r = Colour1.r + Colour2.r;
    //Then divide the 3 ints by 2: int r /= 2;
    //Set it to a new colour: Color newColour = new Color(r, g, b);

    // Solution upfront: How to average RGB colors

//You need to first square the colors and then add them together and then take the square root. Here is an example with 0 and 255 which is the highest in each spectrum.

//0 * 0 = 0

//255*255= 65025

//0 + 65025 = 65025

//65025/2 = 32512,5

//square root of 32512,5 = 180,31

//The average of 0 and 255 is NOT 127,5, but 180!

//And that is a huge difference, because if you just divided it by 2, you average would be much darker!
//The reason this is the case

//The reason why you can just take the average of two rgb colors by just divide it by the number of pixels is because, in the early days of computers, 
//the space was limited, so you had to find ways to save space! The human is also much better at distinguishing dark colors than white colors! 
//Hence they got the genius idea of taking the square root of the colors to save space, this way the could compress a big number like 65025 
//down to a much smaller number like 255 and save space!




    #region example code

    // public class HSVColor{
	// 	public float h;
	// 	public float s;
	// 	public float v;
	// 	public HSVColor(){
	// 		this.h = 0.0f;
	// 		this.s = 0.0f;
	// 		this.v = 0.0f;
	// 	}
		
	// 	public HSVColor(float h, float s, float v){
	// 		this.h = h;
	// 		this.s = s;
	// 		this.v = v;
	// 	}
		
	// 	public override string ToString(){
	// 		return "h:"+h.ToString()+" s:"+s.ToString()+" v:"+v.ToString();
	// 	}
	// }


    

    //     public static Color Hex2RGB(string hexColor){
	// 	//Remove # if present
	// 	if (hexColor.IndexOf('#') != -1)
	// 		hexColor = hexColor.Replace("#", "");
		
	// 	int red = 0;
	// 	int green = 0;
	// 	int blue = 0;
		
	// 	if (hexColor.Length == 6)
	// 	{
	// 		//#RRGGBB
	// 		red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
	// 		green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
	// 		blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
			
			
	// 	}
	// 	else if (hexColor.Length == 3)
	// 	{
	// 		//#RGB
	// 		red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
	// 		green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
	// 		blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
	// 	}

    //     var color = new Color32((byte)red, (byte)green, (byte)blue, 255);
    //     return color;
	// 	//return new Vector3 (red, green, blue);
	
	// }

    // h - 0->1
        // s  0->1
        // v  0-> 1
        // public static Color32 hsvToRgb(HSVColor hsvColor)
        // {
        //     return hsvToRgb(hsvColor.h,hsvColor.s,hsvColor.v);
        // }

        // public static Color32 hsvToRgb(float h, float s, float v)
        // {
        //     // 


        //     int H = (int)(h * 6);

        //     float f = h * 6 - H;
        //     float p = v * (1 - s);
        //     float q = v * (1 - f * s);
        //     float t = v * (1 - (1 - f) * s);

        //     float r=0;
        //     float g=0;
        //     float b=0;

        //     switch (H) {
        //     case 0:
        //         r = v; g = t; b = p;
        //         break;
        //     case 1:
        //         r = q; g = v; b = p;
        //         break;
        //     case 2:
        //         r = p; g = v; b = t;
        //         break;
        //     case 3:
        //         r = p; g = q; b = v;
        //         break;
        //     case 4:
        //         r = t; g = p; b = v;
        //         break;
        //     case 5:
        //         r = v; g = p; b = q;
        //         break;
        //     }
        //     return new UnityEngine.Color32((byte)(255*r),(byte)(255*g),(byte)(255*b),255);
        // }

        // public static HSVColor rgbToHsv(Color32 color)
        // {
        //     HSVColor result=new HSVColor();
        //     float min,max,delta;
        //     float r = (float)color.r/255.0f;
        //     float g = (float)color.g/255.0f;
        //     float b = (float)color.b/255.0f;

        //     min = Mathf.Min(r,g,b);
        //     max = Mathf.Max(r,g,b);
        //     result.v = max;
        //     delta = max - min;

        //     if( max != 0 )
        //         result.s = delta / max;		// s
        //     else {
        //         // r = g = b = 0		// s = 0, v is undefined
        //         result.s = 0;
        //         result.h = -1;
        //         return result;
        //     }
        //     if( r == max )
        //         result.h = ( g - b ) / delta;		// between yellow & magenta
        //     else if( g == max )
        //         result.h = 2 + ( b - r ) / delta;	// between cyan & yellow
        //     else
        //         result.h = 4 + ( r - g ) / delta;	// between magenta & cyan
        //     result.h *= 60;				// degrees
        //     if( result.h < 0 )
        //         result.h += 360;
        //     result.h /=360;
        //     return result;
        // }

        // // Unity EditorGUIUtility.RGBToHSV copy
        // public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
        // {
        //     if ((double) rgbColor.b > (double) rgbColor.g && (double) rgbColor.b > (double) rgbColor.r)
        //         RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
        //     else if ((double) rgbColor.g > (double) rgbColor.r)
        //         RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
        //     else
        //         RGBToHSVHelper(0.0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
        // }

        // static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
        // {
        //     V = dominantcolor;
        //     if ((double) V != 0.0){
        //         float num1 = (double) colorone <= (double) colortwo ? colorone : colortwo;
        //         float num2 = V - num1;
        //         if ((double) num2 != 0.0){
        //           S = num2 / V;
        //           H = offset + (colorone - colortwo) / num2;
        //         } else {
        //           S = 0.0f;
        //           H = offset + (colorone - colortwo);
        //         }
        //         H = H / 6f;
        //         if ((double) H >= 0.0)
        //             return;
        //         H = H + 1f;
        //     } else {
        //         S = 0.0f;
        //         H = 0.0f;
        //     }
        // }
    #endregion example code

}
