using CamSlider.Models;
using CamSlider.ViewModels;
using CamSlider.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		Timer timer = new Timer() { Interval = 1000, Enabled = false };

		public SequencePage ()
		{
			InitializeComponent ();

			BindingContext = ViewModel = new SequenceViewModel();

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

		private async void OnRun(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.RunSequence);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		private async void OnMoveToIn(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToIn);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		private async void OnMoveToOut(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToOut);
			await Navigation.PushModalAsync(RunPage.Instance, false);
		}

		private void SetInFromCurrent(object sender, EventArgs e)
		{
			ViewModel.SlideIn = Comm.Slide.Position;
			ViewModel.PanIn = Comm.Pan.Position;
		}

		private void SetOutFromCurrent(object sender, EventArgs e)
		{
			ViewModel.SlideOut = Comm.Slide.Position;
			ViewModel.PanOut = Comm.Pan.Position;
		}
	}
}