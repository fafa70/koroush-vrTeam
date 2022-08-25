#region MathNet SignalProcessing, Copyright �2004 Christoph Ruegg

// MathNet SignalProcessing, part of MathNet
//
// Copyright (c) 2004,	Christoph Ruegg, http://www.cdrnet.net
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

using MathNet.Numerics;

namespace MathNet.SignalProcessing.Filter
{
	/// <summary>
	/// ShiftBuffer is a circular buffer behaving like a shift register (el. engineering)
	/// </summary>
	public class ShiftBuffer
	{
		int offset = 0;
		readonly int size;
		Complex[] buffer;

		/// <summary>
		/// Shift Buffer for discrete convolutions.
		/// </summary>
		public ShiftBuffer(int size)
		{
			this.size = size;
			buffer = new Complex[size];
		}
		/// <summary>
		/// Shift Buffer for discrete convolutions.
		/// </summary>
		public ShiftBuffer(Complex[] buffer)
		{
			this.size = buffer.Length;
			this.buffer = buffer;
		}

		/// <summary>
		/// Add a new sample on top of the buffer and discard the oldest entry (tail).
		/// </summary>
		public void ShiftAddHead(Complex sample)
		{
			offset = (offset!=0) ? offset-1 : size-1;
			buffer[offset] = sample;
		}

		/// <summary>
		/// Buffer Indexer. The newest (top) item has index 0.
		/// </summary>
		public Complex this[int index]
		{
			get {return buffer[(offset+index)%size];}
			set {buffer[(offset+index)%size] = value;}
		}

		/// <summary>
		/// Discrete Convolution. Evaluates the classic MAC operation to another buffer/vector (looped).
		/// </summary>
		/// <returns>The sum of the memberwiese sample products, sum(a[i]*b[i],i=0..size)</returns>
		public Complex MultiplyAccumulate(Complex[] samples)
		{
			int len = Math.Min(samples.Length,size);
			Complex sum = Complex.Zero;
			int j = offset;
			for(int i=0;i<len;i++)
			{
				sum += samples[i]*buffer[j];
				j = (j+1)%size;
			}
			return sum;
		}

		/// <summary>
		/// Discrete Convolution. Evaluates the classic MAC operation to another buffer/vector (looped).
		/// </summary>
		/// <returns>The sum of the memberwiese sample products, sum(a[i]*b[i],i=0..size)</returns>
		public Complex MultiplyAccumulate(Complex[] samples, int sampleOffset, int count)
		{
			int end = Math.Min(Math.Min(samples.Length,size+sampleOffset),count+sampleOffset);
			Complex sum = Complex.Zero;
			int j = offset;
			for(int i=sampleOffset;i<end;i++)
			{
				sum += samples[i]*buffer[j];
				j = (j+1)%size;
			}
			return sum;
		}

		/// <summary>
		/// Sets all buffer items to zero.
		/// </summary>
		public void Reset()
		{
			for(int i=0;i<size;i++)
				buffer[i] = Complex.Zero;
		}
	}
}
