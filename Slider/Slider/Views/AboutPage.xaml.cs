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
		protected SliderComm Comm { get => SliderComm.Instance; }

		public AboutPage ()
		{
			InitializeComponent ();

			Comm.StateChange += Blue_StateChange;
			Comm.PropertyChanged += Comm_PropertyChanged;
			BlueAction.Clicked += BlueAction_Clicked;
			Comm.Connect("SLIDER");
		}

		private void Comm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Action")
				CheckAction();
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				LabelBlueState.Text = Comm.StateText;
				var s = Comm.CanConnect ? "Connect" : "Disconnect";
				BlueAction.Text = s;
				switch (Comm.State)
				{
					case BlueState.Disconnected:
						{
							CamSlider.Services.PlaySound.Play("down");
						}
						break;
					case BlueState.Connected:
						CamSlider.Services.PlaySound.Play("up");
						CheckAction();
						break;
				}
			});
		}

		void CheckAction()
		{
			if (Comm.Action != SliderComm.Actions.None && Comm.Action != SliderComm.Actions.Unknown)
			{
				if (Parent.Parent is TabbedPage t && t.CurrentPage is NavigationPage n && !(n.CurrentPage is SequencePage))
				{
					Debug.WriteLine($"++> Switching to SequencePage for Action: {Comm.Action}");
					Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							t.CurrentPage = t.Children.First(c => ((NavigationPage)c).CurrentPage is SequencePage);
							((SequencePage)((NavigationPage)t.CurrentPage).CurrentPage).Resume();
						});
				}
			}
		}

		private void BlueAction_Clicked(object sender, EventArgs e)
		{
			if (Comm.CanConnect)
				Comm.Connect("SLIDER");
			else
				Comm.Disconnect();
		}
	}
}