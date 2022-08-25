using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LightFX
{
	 [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
	 internal struct NativeSettings
	 {
		  public byte ClrZone1;
		  public byte ClrZone2;
		  public byte ClrZone3;
		  public byte ClrZone4;

		  public byte IntLevel;
	 }
}
