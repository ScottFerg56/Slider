/*
  OOOO    OOO           OOO               OOO   OOOOOOO   OOO                                      O
 OO  OO    OO            OO                OO    OO  OO    OO                                     OO
OO    O    OO            OO                OO    OO   O    OO                                     OO
OO         OO    OOOOO   OOOO    OOOO      OO    OO O      OO    OOOOO  OOO OO   OOOOO  OO OOO  OOOOOO
OO         OO   OO   OO  OO OO      OO     OO    OOOO      OO   OO   OO OOOOOOO OO   OO  OOOOOO   OO
OO OOOO    OO   OO   OO  OO  OO  OOOOO     OO    OO O      OO   OOOOOOO OO O OO OOOOOOO  OO  OO   OO
OO   OO    OO   OO   OO  OO  OO OO  OO     OO    OO   O    OO   OO      OO O OO OO       OO  OO   OO
 OO  OO    OO   OO   OO  OO  OO OO  OO     OO    OO  OO    OO   OO   OO OO O OO OO   OO  OO  OO   OO OO
  OOO O   OOOO   OOOOO   OOOOO   OOO OO   OOOO  OOOOOOO   OOOO   OOOOO  OO   OO  OOOOO   OO  OO    OOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace CamSlider
{
	/// <summary>
	/// Represents Global variables to be communicated with the remote device.
	/// </summary>
	public class GlobalElement : RemoteElement
	{
		public GlobalElement(IRemoteMaster master, string name, char prefix) : base(master, name, prefix)
		{
		}

		/// <summary>
		/// Run Sequence Actions that are in progress.
		/// </summary>
		public enum Actions
		{
			/// <summary>no Action</summary>
			None,
			/// <summary>Moving to In point</summary>
			MovingToIn,
			/// <summary>Moving to Out point</summary>
			MovingToOut,
			/// <summary>Running Sequence</summary>
			Running,
			/// <summary>Unknown / not initialized</summary>
			Unknown
		};

		/// <summary>
		/// Get/set a Global variable saved on the device to be retrieved when recovering
		/// from a disconnection.
		/// </summary>
		[ElementProperty('a', readOnly: false)]
		public Actions Action { get => GetProperty<Actions>(); set => SetProperty(value); }
	}
}
