Shader "Custom/PortalWindow"
{
	SubShader
	{
		Zwrite Off
		ColorMask 0

		Stencil
		{
			Ref 1
			Pass Replace
		}
		
		Pass
		{
			
		}
	}
}
