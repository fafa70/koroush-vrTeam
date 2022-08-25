using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace LightFX
{
	 public class Led : IDisposable
	 {
		  private bool disposed = false;
		  private Capability capability;
		  private IntPtr handle = new IntPtr(0);

		  public Led()
		  {
				capability = XPSLightFX.InitSDK();
		  }

		  public void SetLed(Setting setting)
		  {
				SetLed(setting, false);
		  }

		  public void SetLed(Setting setting, bool makePermanant)
		  {
				XPSLightFX.SetCurrentSettings(setting, capability, makePermanant);
		  }

		  public void SetEffectFile(string fileName)
		  {
				if (!File.Exists(fileName))
				{
					 throw new ArgumentException(string.Format("Effect file {0} does not exist", fileName));
				}

				StopEffect();
				handle = XPSLightFX.SetEffect(fileName);
		  }

         public Capability GetCapability()
         {
             return capability;
         }

		  public Setting GetLedSettings()
		  {
				return XPSLightFX.GetCurrentSettings(capability);
		  }

		  public void StopEffect()
		  {
				if (!handle.Equals(new IntPtr(0)))
				{
					 XPSLightFX.SuspendEffect(handle);
					 handle = new IntPtr(0);
				}
		  }

		  public void Close()
		  {
				Dispose();
		  }

		  #region IDisposable Members
		  public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
					 StopEffect();

					 XPSLightFX.ReleaseSDK();

					 disposed = true;
            }
        }

		  ~Led()
        {
            Dispose(false);
        }
		  #endregion
	 }
}
