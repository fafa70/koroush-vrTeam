#region MathNet SignalProcessing, Copyright ©2004 Christoph Ruegg, Ben Houston 

// MathNet SignalProcessing, part of MathNet
//
// Copyright (c) 2004,	Christoph Ruegg, http://www.cdrnet.net,
// Based on Exocortex.DSP, Copyright Ben Houston, http://www.exocortex.org
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free Software
// Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using MathNet.Numerics;
using MathNet.Numerics.Transformations;

namespace MathNet.SignalProcessing.DataSources
{
	/// <summary>
	/// Specifies the component of a complex number to work with.
	/// </summary>
	public enum ComplexPart
	{
		/// <summary>The cartesian real part.</summary>
		Real,
		/// <summary>The cartesian imaginary part.</summary>
		Imaginary,
		/// <summary>The polar modulus/absolute part.</summary>
		Modulus,
		/// <summary>The polar argument/angle part.</summary>
		Argument
	}

	/// <summary>
	/// Specifies the color channel of a bitmap to work with.
	/// </summary>
	public enum ColorChannel : int
	{
		/// <summary>Grayscale, the algebraic mean of red, green and blue.</summary>
		Gray = -1,
		/// <summary>Alpha/Transparency channel.</summary>
		Alpha = 3,
		/// <summary>Red part of the RGB scheme.</summary>
		Red = 2,
		/// <summary>Green part of the RGB scheme.</summary>
		Green = 1,
		/// <summary>Blue part of the RGB scheme.</summary>
		Blue = 0,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct PixelData
	{
		public byte blue;
		public byte green;
		public byte red;
		public byte alpha;
	}

	/// <summary>
	/// BitmapConverter, provides conversion methods between complex arrays and bitmaps.
	/// </summary>
	public sealed class BitmapConverter
	{
		private static Size currentSize;
		private static int currentLength;
		private static Bitmap currentBitmap;
		private static bool currentBitmapPrivate;
		private static BitmapData currentData;
#warning: The Class BitmapConverter is obsolete in the Feb 2008 version

		#region Read Operations
		/// <summary>
		/// Converts a bitmap to complex arrays representing red, green and blue channels.
		/// </summary>
		public static unsafe void ReadChannel(Bitmap bitmap, out Complex[] red, out Complex[] green, out Complex[] blue)
		{
			PixelData* p = LoadBitmap(bitmap);
			red = new Complex[currentLength];
			green = new Complex[currentLength];
			blue = new Complex[currentLength];

			double shift8 = 0.00390625d;
			for(int i=0;i<currentLength;i++,p++)
			{
				red[i].Real = ((double)p->red)*shift8;
				green[i].Real = ((double)p->green)*shift8;
				blue[i].Real = ((double)p->blue)*shift8;
			}

			CleanUp(true);
		}

		/// <summary>
		/// Converts a bitmap to complex arrays representing red, green, blue and alpha (transparency) channels.
		/// </summary>
		public static unsafe void ReadChannel(Bitmap bitmap, out Size size, out Complex[] red, out Complex[] green, out Complex[] blue, out Complex[] alpha)
		{
			PixelData* p = LoadBitmap(bitmap);
			red = new Complex[currentLength];
			green = new Complex[currentLength];
			blue = new Complex[currentLength];
			alpha = new Complex[currentLength];
			size = currentSize;

			double shift8 = 0.00390625d;
			for(int i=0;i<currentLength;i++,p++)
			{
				alpha[i].Real = ((double)p->alpha)*shift8;
				red[i].Real = ((double)p->red)*shift8;
				green[i].Real = ((double)p->green)*shift8;
				blue[i].Real = ((double)p->blue)*shift8;
			}

			CleanUp(true);
		}

		/// <summary>
		/// Converts a bitmap a complex array representing a selectable channel.
		/// </summary>
		public static unsafe void ReadChannel(Bitmap bitmap, out Size size, out Complex[] data, ColorChannel channel)
		{
			PixelData* p = LoadBitmap(bitmap);
			data = new Complex[currentLength];
			size = currentSize;

			double shift8 = 0.00390625d;
			double shift8_3 = 0.00130208d;
			switch(channel)
			{
				case ColorChannel.Alpha:
					for(int i=0;i<currentLength;i++,p++)
						data[i].Real = ((double)p->alpha)*shift8;
					break;
				case ColorChannel.Red:
					for(int i=0;i<currentLength;i++,p++)
						data[i].Real = ((double)p->red)*shift8;
					break;
				case ColorChannel.Green:
					for(int i=0;i<currentLength;i++,p++)
						data[i].Real = ((double)p->green)*shift8;
					break;
				case ColorChannel.Blue:
					for(int i=0;i<currentLength;i++,p++)
						data[i].Real = ((double)p->blue)*shift8;
					break;
				case ColorChannel.Gray:
					for(int i=0;i<currentLength;i++,p++)
						data[i].Real = ((double)(p->red+p->green+p->blue))*shift8_3;
					break;
			}

			CleanUp(true);
		}
		#endregion

		#region Write Operations
		/// <summary>
		/// Converts complex arrays representing red, green and blue channels to a bitmap.
		/// </summary>
		public static unsafe Bitmap WriteChannel(Size size, Complex[] red, ComplexPart redPart, Complex[] green, ComplexPart greenPart, Complex[] blue, ComplexPart bluePart)
		{
			PixelData* p = CreateBitmap(size);
			double scale = 256d;
			byte[] r = ConvertToByte(red,redPart,scale);
			byte[] g = ConvertToByte(green,greenPart,scale);
			byte[] b = ConvertToByte(blue,bluePart,scale);

			for(int i=0;i<currentLength;i++,p++)
			{
				p->alpha = byte.MaxValue;
				p->red = r[i];
				p->green = g[i];
				p->blue = b[i];
			}

			CleanUp(false);
			return currentBitmap;
		}

		/// <summary>
		/// Converts complex arrays representing red, green, blue and alpha (transparency) channels to a bitmap.
		/// </summary>
		public static unsafe Bitmap WriteChannel(Size size, Complex[] red, ComplexPart redPart, Complex[] green, ComplexPart greenPart, Complex[] blue, ComplexPart bluePart, Complex[] alpha, ComplexPart alphaPart)
		{
			PixelData* p = CreateBitmap(size);
			double scale = 256d;
			byte[] r = ConvertToByte(red,redPart,scale);
			byte[] g = ConvertToByte(green,greenPart,scale);
			byte[] b = ConvertToByte(blue,bluePart,scale);
			byte[] a = ConvertToByte(alpha,alphaPart,scale);

			for(int i=0;i<currentLength;i++,p++)
			{
				p->alpha = a[i];
				p->red = r[i];
				p->green = g[i];
				p->blue = b[i];
			}

			CleanUp(false);
			return currentBitmap;
		}

		/// <summary>
		/// Converts a complex array representing a selectable channel to a bitmap.
		/// </summary>
		public static unsafe Bitmap WriteChannel(Size size, Complex[] data, ComplexPart dataPart, ColorChannel channel)
		{
			PixelData* p = CreateBitmap(size);
			byte[] d = ConvertToByte(data,dataPart,256d);

			switch(channel)
			{
				case ColorChannel.Alpha:
					for(int i=0;i<currentLength;i++,p++)
					{
						p->alpha = d[i];
						p->red = byte.MinValue;
						p->green = byte.MinValue;
						p->blue = byte.MinValue;
					}
					break;
				case ColorChannel.Red:
					for(int i=0;i<currentLength;i++,p++)
					{
						p->alpha = byte.MaxValue;
						p->red = d[i];
						p->green = byte.MinValue;
						p->blue = byte.MinValue;
					}
					break;
				case ColorChannel.Green:
					for(int i=0;i<currentLength;i++,p++)
					{
						p->alpha = byte.MaxValue;
						p->red = byte.MinValue;
						p->green = d[i];
						p->blue = byte.MinValue;
					}
					break;
				case ColorChannel.Blue:
					for(int i=0;i<currentLength;i++,p++)
					{
						p->alpha = byte.MaxValue;
						p->red = byte.MinValue;
						p->green = byte.MinValue;
						p->blue = d[i];
					}
					break;
				case ColorChannel.Gray:
					for(int i=0;i<currentLength;i++,p++)
					{
						p->alpha = byte.MaxValue;
						p->red = d[i];
						p->green = d[i];
						p->blue = d[i];
					}
					break;
			}

			CleanUp(false);
			return currentBitmap;
		}

		private static byte[] ConvertToByte(Complex[] data, ComplexPart part, double scale)
		{
			byte[] d = new byte[data.Length];
			if(part == ComplexPart.Imaginary)
			{
				for(int i=0;i<data.Length;i++)
					d[i] = (byte)Math.Max(0,Math.Min(255,(int)(data[i].Imag*scale)));
				return d;
			}
			if(part == ComplexPart.Modulus)
			{
				for(int i=0;i<data.Length;i++)
					d[i] = (byte)Math.Max(0,Math.Min(255,(int)(data[i].Modulus*scale)));
				return d;
			}
			if(part == ComplexPart.Argument)
			{
				double s = scale/(2*Math.PI);
				for(int i=0;i<data.Length;i++)
					d[i] = (byte)Math.Max(0,Math.Min(255,(int)((Math.PI+data[i].Argument)*s)));
				return d;
			}
			for(int i=0;i<data.Length;i++)
				d[i] = (byte)Math.Max(0,Math.Min(255,(int)(data[i].Real*scale)));
			return d;
		}
		#endregion

		#region Lead & Tail Helpers
		private unsafe static PixelData* LoadBitmap(Bitmap bitmap)
		{
			//currentSize = new Size(
			//	Fn.IntPow2((int)Math.Ceiling(Math.Log(bitmap.Width,2))),
			//	Fn.IntPow2((int)Math.Ceiling(Math.Log(bitmap.Height,2))));
            currentSize = bitmap.Size;
			if(currentSize != bitmap.Size)
			{
				currentBitmapPrivate = true;
				currentBitmap = new Bitmap(bitmap,currentSize);
            }
			else
			{
				currentBitmapPrivate = false;
				currentBitmap = bitmap;
			}
			currentLength = currentSize.Width * currentSize.Height;
			Rectangle rect = new Rectangle(0,0,currentSize.Width,currentSize.Height);
   
            try {
                currentData = currentBitmap.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			}
            catch (SystemException e) {
                Console.WriteLine(e.ToString());
            }
                
                return (PixelData*)currentData.Scan0.ToPointer();
		}

		private unsafe static PixelData* CreateBitmap(Size size)
		{
			currentSize = size;
			currentLength = size.Width * size.Height;
			currentBitmap = new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			Rectangle rect = new Rectangle(0,0,size.Width,size.Height);
			currentData = currentBitmap.LockBits(rect,ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
			return (PixelData*)currentData.Scan0.ToPointer();
		}

		private static void CleanUp(bool disposeBitmap)
		{
            if (currentBitmap != null)
            {

                currentBitmap.UnlockBits(currentData);
                currentData = null;
                if (disposeBitmap)
                {
                    if (currentBitmapPrivate)
                        currentBitmap.Dispose();
                    currentBitmap = null;
                }
            }
		}
		#endregion
	}
}
