using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using CamSlider.CustomControls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSwitch), typeof(CamSlider.Droid.Renderers.FMSwitchRenderer))]
namespace CamSlider.Droid.Renderers
{
	/// <summary>
	/// A subclass of the Xamarin SliderRenderer to support our FMSwitch control subclass to expose expose custom labels for ON and OFF.
	/// </summary>
	public class FMSwitchRenderer : SwitchRenderer
	{
		public FMSwitchRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
				throw new Exception("Control is null!");

			// set the labels on the platform's control
			Control.TextOn = (e.NewElement as FMSwitch).TextOn;
			Control.TextOff = (e.NewElement as FMSwitch).TextOff;
			Control.ShowText = true;	// and yeah, there's this too!

			// while we're here, change the shape to something not ugly!
			ShapeDrawable th = new ShapeDrawable(new RectShape());
			th.SetIntrinsicWidth(140);
			th.SetIntrinsicHeight(70);
			th.SetColorFilter(Android.Graphics.Color.DarkGray, Android.Graphics.PorterDuff.Mode.SrcOver);
			Control.ThumbDrawable = th;
		}
	}
}
