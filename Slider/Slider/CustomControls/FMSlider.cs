using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.CustomControls
{
	public class FMSlider : Xamarin.Forms.Slider
	{
		public FMSlider() : base()
		{
		}

		public event EventHandler StoppedTracking;
		public void OnStoppedTracking()
		{
			StoppedTracking?.Invoke(this, EventArgs.Empty);
		}
	}
}
