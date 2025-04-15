Shader "Custom/DepthMasker"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-10" }
        
        ColorMask 0 // Don't render any color
        ZWrite On   // Write to the depth buffer
        //ZTest LEqual // Make sure it only affects areas where it is closer
        
        Pass {}
    }
}
