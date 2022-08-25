#region MathNet SignalProcessing, Copyright ©2004 Christoph Ruegg

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
	/// Filter coefficient container.
	/// </summary>
	public class FilterCoefficients
	{
		private Complex[] coefficients;

		/// <summary>
		/// Filter coefficient container.
		/// </summary>
		public FilterCoefficients(Complex[] coefficients)
		{
			this.coefficients = coefficients;
		}

		/// <summary>
		/// Filter Coefficients
		/// </summary>
		public Complex[] Coefficients
		{
			get {return coefficients;}
		}

		/// <summary>
		/// Converts the coeffients to a System.String.
		/// </summary>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append('[');
			for(int i=0;i<coefficients.Length;i++)
			{
				if(i>0)
					sb.Append(",\n");
				sb.Append(coefficients[i].ToString());
			}
			sb.Append(']');
			return sb.ToString();
		}
	}
}
