//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Widget;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSlider), typeof(CamSlider.Droid.Renderers.FMSliderRenderer))]
namespace CamSlider.Droid.Renderers
{
	/// <summary>
	/// A subclass of the Xamarin SliderRenderer to support our FMSlider control subclass to expose a StoppedTracking event.
	/// </summary>
	public class FMSliderRenderer : SliderRenderer
	{
		/*
		 * Ideally we'd just connect to the StopTrackingTouch event of the base SeekBar control.
		 * Unfortunately this fails and the basic thumb tracking of the control stops working.
		 * This is probably because the SeekBar event mechanism sets the SeekBarChangeListener,
		 * and listeners can only have one client, so it disconnects itself in the process!
		 * We avoid that problem here by setting our own listener so we can capture the
		 * event we want, and then blindly forward everything to our base class.
		 */

		readonly SeekBarListenerFork Fork;

		public FMSliderRenderer(Context context) : base(context)
		{
			// create a listener for forking off the event we're interested in
			Fork = new SeekBarListenerFork(this);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Slider> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
				throw new Exception("Control is null!");

			// while we're here, set the thumb circle size to something we can actually see and use!!
			// all this is excitingly flexible, yet depressingly difficult to discover!
			ShapeDrawable th = new ShapeDrawable(new OvalShape());
			th.SetIntrinsicWidth(100);
			th.SetIntrinsicHeight(100);
			th.SetColorFilter(Android.Graphics.Color.Red, Android.Graphics.PorterDuff.Mode.SrcOver);
			Control.SetThumb(th);

			if (e.OldElement != null)
			{
				// Unsubscribe
				Control.SetOnSeekBarChangeListener(null);
			}
			if (e.NewElement != null)
			{
				// Subscribe
				Control.SetOnSeekBarChangeListener(Fork);
			}
		}
	}

	/// <summary>
	/// A listener for forking off the event we're interested in.
	/// </summary>
	public class SeekBarListenerFork : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
	{
		FMSliderRenderer Renderer;

		public SeekBarListenerFork(FMSliderRenderer renderer)
		{
			Renderer = renderer;	// remember the renderer we're servicing
		}

		public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			// fork the event off to the base renderer
			(Renderer as SeekBar.IOnSeekBarChangeListener)?.OnProgressChanged(seekBar, progress, fromUser);
		}

		public void OnStartTrackingTouch(SeekBar seekBar)
		{
			// fork the event off to the base renderer
			(Renderer as SeekBar.IOnSeekBarChangeListener)?.OnStartTrackingTouch(seekBar);
		}

		public void OnStopTrackingTouch(SeekBar seekBar)
		{
			// fork the event off to the base renderer
			(Renderer as SeekBar.IOnSeekBarChangeListener)?.OnStopTrackingTouch(seekBar);
			// FINALLY, fire the StoppedTracking event on our custom FMSlider control!
			var slider = (CamSlider.CustomControls.FMSlider)Renderer.Element;
			slider.OnStoppedTracking();
		}
	}
}
