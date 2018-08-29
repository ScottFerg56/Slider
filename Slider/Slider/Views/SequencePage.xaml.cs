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
		SequenceViewModel SeqViewModel;

		public SequencePage ()
		{
			InitializeComponent ();

			BindingContext = SeqViewModel = new SequenceViewModel();

			ButtonMinsUp.Held += (s, e) => { SeqViewModel.DurationMins++; };
			ButtonMinsDn.Held += (s, e) => { SeqViewModel.DurationMins--; };
			ButtonSecsUp.Held += (s, e) => { SeqViewModel.DurationSecs++; };
			ButtonSecsDn.Held += (s, e) => { SeqViewModel.DurationSecs--; };

			ButtonPMinsUp.Held += (s, e) => { SeqViewModel.PlaybackMins++; };
			ButtonPMinsDn.Held += (s, e) => { SeqViewModel.PlaybackMins--; };
			ButtonPSecsUp.Held += (s, e) => { SeqViewModel.PlaybackSecs++; };
			ButtonPSecsDn.Held += (s, e) => { SeqViewModel.PlaybackSecs--; };
		}

		private async void OnRun(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.RunSequence);
			await Navigation.PushModalAsync(RunPage.Instance);
		}

		private async void OnMoveToIn(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToIn);
			await Navigation.PushModalAsync(RunPage.Instance);
		}

		private async void OnMoveToOut(object sender, EventArgs e)
		{
			RunPage.Instance.Init(RunCommand.MoveToOut);
			await Navigation.PushModalAsync(RunPage.Instance);
		}
	}
}