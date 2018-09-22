﻿using System;
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
			// try connecting as we start up
			Comm.Connect("SLIDER");
		}

		/// <summary>
		/// Look for changes in the Action property from the device to possibly resume
		/// from a problem after being disconnected during a sequence movement in the RunPage.
		/// </summary>
		private void Comm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Action")
				CheckAction();
		}

		/// <summary>
		/// Respond to changes in the Bluetooth state.
		/// </summary>
		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				// feedback for the user
				LabelBlueState.Text = Comm.StateText;
				var s = Comm.CanConnect ? "Connect" : "Disconnect";
				BlueAction.Text = s;
				// play a cute sound too!
				switch (Comm.State)
				{
					case BlueState.Disconnected:
						CamSlider.Services.PlaySound.Play("down");
						break;
					case BlueState.Connected:
						CamSlider.Services.PlaySound.Play("up");
						CheckAction();
						break;
				}
			});
		}

		/// <summary>
		/// Check to see if there is/was a run sequence action in progress we might need to resume.
		/// </summary>
		void CheckAction()
		{
			if (Comm.Action != SliderComm.Actions.None && Comm.Action != SliderComm.Actions.Unknown)
			{
				if (Parent.Parent is TabbedPage t && t.CurrentPage is NavigationPage n && !(n.CurrentPage is SequencePage))
				{
					// switch to the SequencePage so resuming on the modal RunPage will return there when finished
				//	Debug.WriteLine($"++> Switching to SequencePage for Action: {Comm.Action}");
					Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							t.CurrentPage = t.Children.First(c => ((NavigationPage)c).CurrentPage is SequencePage);
							((SequencePage)((NavigationPage)t.CurrentPage).CurrentPage).Resume();
						});
				}
			}
		}

		/// <summary>
		/// Respond to the Connect/Disconnect button.
		/// </summary>
		private void BlueAction_Clicked(object sender, EventArgs e)
		{
			if (Comm.CanConnect)
				Comm.Connect("SLIDER");
			else
				Comm.Disconnect();
		}
	}
}
