/*
 OOOOO                                                          OOOOOO
OO   OO                                                          OO  OO
OO   OO                                                          OO  OO
 OO      OOOOO   OOO OO OO  OO   OOOOO  OO OOO   OOOOO   OOOOO   OO  OO  OOOO    OOO OO  OOOOO
  OOO   OO   OO OO  OO  OO  OO  OO   OO  OOOOOO OO   OO OO   OO  OOOOO      OO  OO  OO  OO   OO
    OO  OOOOOOO OO  OO  OO  OO  OOOOOOO  OO  OO OO      OOOOOOO  OO      OOOOO  OO  OO  OOOOOOO
OO   OO OO      OO  OO  OO  OO  OO       OO  OO OO      OO       OO     OO  OO  OO  OO  OO
OO   OO OO   OO  OOOOO  OO  OO  OO   OO  OO  OO OO   OO OO   OO  OO     OO  OO   OOOOO  OO   OO
 OOOOO   OOOOO      OO   OOO OO  OOOOO   OO  OO  OOOOO   OOOOO  OOOO     OOO OO     OO   OOOOO
                    OO                                                          OO  OO
                   OOOO                                                          OOOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using CamSlider.ViewModels;
using System;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SequencePage : ContentPage
	{
		SequenceViewModel ViewModel;
		protected SliderComm Comm { get => SliderComm.Instance; }

		/// <summary>
		/// Timer to delay launching the of the modal RunPage during Resume operations.
		/// </summary>
		Timer timer = new Timer() { Interval = 1000, Enabled = false };

		public SequencePage ()
		{
			InitializeComponent ();

			BindingContext = ViewModel = new SequenceViewModel();

			// connect all the '+' and '-' buttons to do their thing for their related values

			ButtonMinsUp.Held += (s, e) => { ViewModel.DurationMins++; };
			ButtonMinsDn.Held += (s, e) => { ViewModel.DurationMins--; };
			ButtonSecsUp.Held += (s, e) => { ViewModel.DurationSecs++; };
			ButtonSecsDn.Held += (s, e) => { ViewModel.DurationSecs--; };

			ButtonPMinsUp.Held += (s, e) => { ViewModel.PlaybackMins++; };
			ButtonPMinsDn.Held += (s, e) => { ViewModel.PlaybackMins--; };
			ButtonPSecsUp.Held += (s, e) => { ViewModel.PlaybackSecs++; };
			ButtonPSecsDn.Held += (s, e) => { ViewModel.PlaybackSecs--; };

			timer.Elapsed += Timer_Elapsed;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Enabled = false;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
			{
				// invoke the modal RunPage for a Resume operation
				RunPage.Instance.Init(RunCommand.Resume);
				await Navigation.PushModalAsync(RunPage.Instance, false);
			});
		}

		public void Resume()
		{
			// We apparently cannot rely on OnAppearing being called in this case
			// and pushing the modal RunPage before appearing causes lots of problems
			// so set a timer and launch the RunPage from there
			timer.Enabled = true;
		}

		/// <summary>
		/// Respond to the Play button click to Run the Sequence.
		/// </summary>
		private async void OnRun(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.RunSequence);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		/// <summary>
		/// Respond to the MoveToIn button click to move to the In point.
		/// </summary>
		private async void OnMoveToIn(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToIn);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		/// <summary>
		/// Respond to the MoveToOut button click to move to the Out point.
		/// </summary>
		private async void OnMoveToOut(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToOut);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		/// <summary>
		/// Respond to the button click to set the In point from the current position.
		/// </summary>
		private void SetInFromCurrent(object sender, EventArgs e)
		{
			ViewModel.SlideIn = (int)Math.Round(Comm.Slide.Position);
			ViewModel.PanIn = (int)Math.Round(Comm.Pan.Position);
		}

		/// <summary>
		/// Respond to the button click to set the Out point from the current position.
		/// </summary>
		private void SetOutFromCurrent(object sender, EventArgs e)
		{
			ViewModel.SlideOut = (int)Math.Round(Comm.Slide.Position);
			ViewModel.PanOut = (int)Math.Round(Comm.Pan.Position);
		}
	}
}