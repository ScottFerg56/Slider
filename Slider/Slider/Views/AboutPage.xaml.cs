using System;
using System.Diagnostics;
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