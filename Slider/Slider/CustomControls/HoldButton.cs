//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using System;
using System.Timers;
using Xamarin.Forms;

namespace CamSlider.CustomControls
{
	/// <summary>
	/// A subclass of the Xamarin Button to expose an event that fires periodically
	/// while the button remains pressed.
	/// </summary>
	public class HoldButton : Button
    {
		Timer timer;		// timer to repeat the Held event
		bool IsPressed;		// tracking pressed state of button

		/// <summary>
		/// An event that fires every tenth second while the button remains pressed.
		/// </summary>
		public event EventHandler Held;

		public HoldButton()
		{
			timer = new Timer
			{
				Enabled = false,
				Interval = 500
			};
			timer.Elapsed += Timer_Elapsed;
			this.Pressed += HoldButton_Pressed;
			this.Released += HoldButton_Released;
		}

		private void HoldButton_Released(object sender, EventArgs e)
		{
			IsPressed = false;		// note button no longer pressed
			// let the timer disable itself
		}

		private void HoldButton_Pressed(object sender, EventArgs e)
		{
			IsPressed = true;						// note button is pressed
			Held?.Invoke(this, EventArgs.Empty);	// fire the event at least once at the start
			timer.Enabled = true;					// start timer w/initial half-second startup delay
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (IsPressed)
			{
				Held?.Invoke(this, EventArgs.Empty);	// fire the event
				timer.Interval = 100;					// sebsequent events fire every tenth second
			}
			else
			{
				timer.Enabled = false;					// kill the timer, we're done
				timer.Interval = 500;					// reset to startup delay
			}
		}
	}
}
