using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();

			SliderComm.Instance.StateChange += Blue_StateChange;
			BlueAction.Clicked += BlueAction_Clicked;
			SliderComm.Instance.Connect("SLIDER");
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				LabelBlueState.Text = SliderComm.Instance.StateText;
				var s = SliderComm.Instance.CanConnect ? "Connect" : "Disconnect";
				BlueAction.Text = s;
				this.Title = s;
				if (this.Parent is NavigationPage p)
					p.Title = s;
				if (SliderComm.Instance.State == BlueState.Connected || SliderComm.Instance.State == BlueState.Disconnected)
				{
					var assembly = typeof(App).Assembly;
					var file = SliderComm.Instance.State == BlueState.Connected ? "up" : "down";
					System.IO.Stream audioStream = assembly.GetManifestResourceStream("Slider.Resources." + file + ".mp3");

					var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
					if (player.IsPlaying)
						player.Stop();
					try
					{
						player.Load(audioStream);
						player.Play();

					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Play exception: {ex}");
					}
				}
				if (SliderComm.Instance.State == BlueState.Connected)
				{
					// switch to the ManualPage once we're connected
					if (Parent.Parent is TabbedPage t)
					{
						// ANDROID HACK::
						// There's apparently some kind of timing problem such that the ManualPage
						// has already appeared (before connection) so it gets no OnAppearing notification
						// from this navigation (after conection)
						// So it's request for updated position values fails and doesn't get re-triggered
						// and we must do it here

						// trigger updated values from the device
						var pos = Stepper.Slide.Position.ToString();
						pos = Stepper.Pan.Position.ToString();
						// our Parent is a NavigationPage and his Parent is the TabbedPage (MainPage)
						// we'll find the ManualPage among the TabbedPage's grandchildren
						t.CurrentPage = t.Children.First(c => ((NavigationPage)c).CurrentPage is ManualPage);
					}
				}
			});
		}

		private void BlueAction_Clicked(object sender, EventArgs e)
		{
			if (SliderComm.Instance.CanConnect)
				SliderComm.Instance.Connect("SLIDER");
			else
				SliderComm.Instance.Disconnect();
		}
	}
}