/*
OOOOOOO OO   OO  OOOOO             OO      O            OOO
 OO  OO OOO OOO OO   OO            OO     OO             OO
 OO   O OOOOOOO OO   OO                   OO             OO
 OO O   OOOOOOO  OO     OO   OO   OOO   OOOOOO   OOOOO   OO OO
 OOOO   OO O OO   OOO   OO   OO    OO     OO    OO   OO  OOO OO
 OO O   OO   OO     OO  OO O OO    OO     OO    OO       OO  OO
 OO     OO   OO OO   OO OO O OO    OO     OO    OO       OO  OO
 OO     OO   OO OO   OO  OOOOO     OO     OO OO OO   OO  OO  OO
OOOO    OO   OO  OOOOO    O O     OOOO     OOO   OOOOO  OOO  OO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

namespace CamSlider.CustomControls
{
	/// <summary>
	/// A subclass of the Xamarin Switch to expose custom labels for ON and OFF.
	/// </summary>
	public class FMSwitch : Xamarin.Forms.Switch
	{
		public FMSwitch() : base()
		{
		}

		/// <summary>
		/// The string label for the ON position.
		/// </summary>
		public string TextOn { get; set; } = "ON";

		/// <summary>
		/// The string label for the OFF position.
		/// </summary>
		public string TextOff { get; set; } = "OFF";
	}
}
