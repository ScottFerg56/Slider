using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Xamarin.Forms;

namespace CamSlider.CustomControls
{
    public class HoldButton : Button
    {
		Timer timer;
		bool IsPressed;

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
			IsPressed = false;
		}

		private void HoldButton_Pressed(object sender, EventArgs e)
		{
			Held?.Invoke(this, EventArgs.Empty);
			IsPressed = true;
			timer.Enabled = true;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Interval = 100;
			if (IsPressed)
			{
				Held?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				timer.Enabled = false;
				timer.Interval = 500;
			}
		}
	}
}
