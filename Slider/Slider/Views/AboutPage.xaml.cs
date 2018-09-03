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
				switch (SliderComm.Instance.State)
				{
					case BlueState.Disconnected:
						{
							CamSlider.Services.PlaySound.Play("down");
							//if (Parent.Parent is TabbedPage t && t.CurrentPage is NavigationPage n && n.CurrentPage is ManualPage)
							//{
							//	t.CurrentPage = t.Children.First(c => ((NavigationPage)c).CurrentPage is AboutPage);
							//}
						}
						break;
					case BlueState.Connected:
						CamSlider.Services.PlaySound.Play("up");
						break;
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