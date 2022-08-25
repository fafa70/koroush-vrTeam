using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace LightFX
{
	 /// <summary>
	 /// Wrapper class around GamingSDK.dll which is written in C++
	 /// </summary>
	 internal class XPSLightFX
	 {
		  #region Properties
		  internal static Capability Capability
		  {
				get
				{
					 Capability capability = Capability.None;

					 GetCapabilities(out capability);

					 return capability;
				}
		  }
		  #endregion

		  #region DLL Imports
		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  internal static extern Capability InitSDK();

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  internal static extern void ReleaseSDK();

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern bool GetLEDSettings(IntPtr pSettings, Capability ledCaps);

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern bool SetPowerOnDefaults(IntPtr pSettings, Capability ledCaps);

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern bool SetLEDSettings(IntPtr pSettings, Capability ledCaps, int bMakePerm);

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern void GetCapabilities(out Capability ledCaps);

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern IntPtr SetEffect(string effect, Capability ledCaps);

		  [DllImport("GamingSDK.dll", CallingConvention = CallingConvention.Cdecl)]
		  private static extern bool StopEffect(IntPtr threadHandle); 
		  #endregion

		  #region Memory Management
		  /// <summary>
		  /// Restore the data stored in memory to a Setting struct
		  /// </summary>
		  /// <param name="nativeSettings"></param>
		  /// <param name="settingPointer"></param>
		  /// <returns></returns>
		  private static Setting Restore(IntPtr settingsPointer)
		  {
				Setting settings = new Setting();

				//pull the native structure out of memory
				NativeSettings nativeSettings = (NativeSettings)Marshal.PtrToStructure(settingsPointer, typeof(NativeSettings));

				//free up the allocated memory
				Marshal.FreeHGlobal(settingsPointer);

				//build the new settings structure
				settings.Fans = (Color)nativeSettings.ClrZone1;
				settings.Speakers = (Color)nativeSettings.ClrZone2;
				settings.Lid = (Color)nativeSettings.ClrZone3;
				settings.TouchPad = (TouchPad)nativeSettings.ClrZone4;
				settings.Intensity = (Intensity)nativeSettings.IntLevel;

				return settings;
		  }

		  /// <summary>
		  /// Save a Settings structure to memory
		  /// </summary>
		  /// <param name="settings"></param>
		  /// <returns>IntPtr reference to the setting in memory</returns>
		  private static IntPtr Save(Setting settings)
		  {
				//conver the Settings to the native object
				NativeSettings nativeSettings = new NativeSettings();
				nativeSettings.IntLevel = (byte)settings.Intensity;
				nativeSettings.ClrZone1 = (byte)settings.Fans;
				nativeSettings.ClrZone2 = (byte)settings.Speakers;
				nativeSettings.ClrZone3 = (byte)settings.Lid;
				nativeSettings.ClrZone4 = (byte)settings.TouchPad;

				//allocate memory for the object
				IntPtr settingsPointer = Marshal.AllocHGlobal(Marshal.SizeOf(nativeSettings));

				//save the object in memory at the pointer
				Marshal.StructureToPtr(nativeSettings, settingsPointer, false);

				//return the pointer
				return settingsPointer;
		  } 
		  #endregion

		  /// <summary>
		  /// Get the current settings for the LED's
		  /// </summary>
		  /// <param name="capability"></param>
		  /// <returns></returns>
		  internal static Setting GetCurrentSettings(Capability capability)
		  {
				//get the size of a nativesettings object for memory
				int size = Marshal.SizeOf(new NativeSettings());

				//allocate the memory space and return a pointer
				IntPtr settingsPointer = Marshal.AllocHGlobal(size);

				//place the LED settings into memory at the pointer
				if (!GetLEDSettings(settingsPointer, capability))
				{
					 //error on failure
					 throw new System.ApplicationException(String.Format("{0} failed.", MethodBase.GetCurrentMethod().Name));
				}

				//restore the settings in memory into a setting structure
				return Restore(settingsPointer);
		  }

		  /// <summary>
		  /// Set the Power On Defaults for the LED's
		  /// </summary>
		  /// <param name="settings">The settings to apploy</param>
		  /// <param name="capability"></param>
		  internal static void SetDefaults(Setting settings, Capability capability)
		  {
				//place the setting into memory and get the pointer
				IntPtr settingsPointer = Save(settings);

				//set the settings as the power on defaults
				bool result = SetPowerOnDefaults(settingsPointer, capability);

				//clean up the used memory
				Marshal.FreeHGlobal(settingsPointer);

				//throw an error for failure
				if (!result)
				{
					 throw new System.ApplicationException(String.Format("{0} failed.", MethodBase.GetCurrentMethod().Name));
				}
		  }

		  /// <summary>
		  /// Set the current LED settings
		  /// </summary>
		  /// <param name="settings">The settings to apply</param>
		  /// <param name="capability"></param>
		  /// <param name="setAsDefault">Set to true to save as Power On Defaults</param>
		  internal static void SetCurrentSettings(Setting settings, Capability capability, bool setAsDefault)
		  {
				//place the setting into memory and get the pointer
				IntPtr settingsPointer = Save(settings);

				//Set the settings as the current LED settings
				bool result = SetLEDSettings(settingsPointer, capability, System.Convert.ToInt32(setAsDefault));

				//free up the used memory
				Marshal.FreeHGlobal(settingsPointer);

				//throw error on failure
				if (!result)
				{
					 throw new System.ApplicationException(String.Format("{0} failed.", MethodBase.GetCurrentMethod().Name));
				}
		  }

		  /// <summary>
		  /// Run an effect file (.xml)
		  /// </summary>
		  /// <param name="effect">The Xml file to run</param>
		  /// <returns>IntPtr pointer to the thread</returns>
		  internal static IntPtr SetEffect(string fileName)
		  {
				//Set and run the effect file
				return SetEffect(fileName, XPSLightFX.Capability);
		  }

		  /// <summary>
		  /// Stop a running effect
		  /// </summary>
		  /// <param name="threadPointer">The IntPtr pointer to the thread</param>
		  internal static void SuspendEffect(IntPtr threadPointer)
		  {
				if (!StopEffect(threadPointer))
				{
					 throw new System.ApplicationException(String.Format("{0} failed.", MethodBase.GetCurrentMethod().Name));
				}
		  }
	 }
}
