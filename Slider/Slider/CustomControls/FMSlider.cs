//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using System;

namespace CamSlider.CustomControls
{
	/// <summary>
	/// A subclass of the Xamarin Slider to expose a StoppedTracking event.
	/// </summary>
	public class FMSlider : Xamarin.Forms.Slider
	{
		public FMSlider() : base()
		{
		}

		/// <summary>
		/// An event fired when the slider thumb is released after tracking.
		/// </summary>
		public event EventHandler StoppedTracking;

		public void OnStoppedTracking()
		{
			StoppedTracking?.Invoke(this, EventArgs.Empty);
		}
	}
}
