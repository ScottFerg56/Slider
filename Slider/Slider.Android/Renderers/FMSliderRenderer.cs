using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSlider), typeof(CamSlider.Droid.Renderers.FMSliderRenderer))]
namespace CamSlider.Droid.Renderers
{
	public class FMSliderRenderer : SliderRenderer
	{
		SeekBar SeekBar;
		readonly SeekBarListenerFork Fork;

		public FMSliderRenderer(Context context) : base(context)
		{
			Fork = new SeekBarListenerFork(this);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Slider> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
			{
				SeekBar = new SeekBar(Context);
				SetNativeControl(SeekBar);
			}
			else
			{
				SeekBar = Control;
			}

			ShapeDrawable th = new ShapeDrawable(new OvalShape());
			th.SetIntrinsicWidth(100);
			th.SetIntrinsicHeight(100);
			th.SetColorFilter(Android.Graphics.Color.Red, Android.Graphics.PorterDuff.Mode.SrcOver);
			SeekBar.SetThumb(th);

			if (e.OldElement != null)
			{
				// Unsubscribe
				SeekBar.SetOnSeekBarChangeListener(null);
			}
			if (e.NewElement != null)
			{
				// Subscribe
				SeekBar.SetOnSeekBarChangeListener(Fork);
			}
		}
	}

	public class SeekBarListenerFork : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
	{
		FMSliderRenderer Renderer;

		public SeekBarListenerFork(FMSliderRenderer renderer)
		{
			Renderer = renderer;
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
			var slider = (CamSlider.CustomControls.FMSlider)Renderer.Element;
			slider.OnStoppedTracking();
		}
	}
}