//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using CamSlider.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RunPage : ContentPage
	{
		RunViewModel ViewModel;

		private static RunPage _Instance;
		/// <summary>
		/// Get the single instance of the RunPage class.
		/// </summary>
		/// <remarks>
		/// Keep the RunPage around because it will be reused a lot.
		/// </remarks>
		public static RunPage Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new RunPage();
				return _Instance;
			}
		}

		protected RunPage()
		{
			InitializeComponent ();

			BindingContext = ViewModel = new RunViewModel();

			ViewModel.Stopped += ViewModel_Stopped;
		}

		/// <summary>
		/// The Run operation to complete.
		/// </summary>
		RunCommand Command;

		/// <summary>
		/// Track our closed state so we don't abuse it.
		/// </summary>
		bool Closed;

		/// <summary>
		/// Initialize for the next operation and the page appearing.
		/// </summary>
		/// <param name="cmd"></param>
		public void Init(RunCommand cmd)
		{
			Command = cmd;
			Closed = false;
		}

		/// <summary>
		/// Respond to the Back button being pressed by aborting the operation in progress
		/// and dismissing the modal RunPage.
		/// </summary>
		/// <returns>True</returns>
		protected override bool OnBackButtonPressed()
		{
			ViewModel.Stop();   // this invokes our Stopped event
			return true;
		}

		/// <summary>
		/// Initialize our ViewModel when our modal page appears.
		/// </summary>
		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Init(Command);
		}

		/// <summary>
		/// Respond to completion of the operation in progress.
		/// </summary>
		private async void ViewModel_Stopped(object sender, EventArgs e)
		{
			// make sure we're not already closed
			if (!Closed)
			{
				// close our modal page
				await Navigation.PopModalAsync();
				Closed = true;
			}
		}

		/// <summary>
		/// Respond to the Stop button click by stopping the current operation in progress.
		/// </summary>
		private void OnStop(object sender, EventArgs e)
		{
			ViewModel.Stop();	// this invokes our Stopped event
		}

		/// <summary>
		/// Respond to the Play button click by starting sequence to Run.
		/// </summary>
		private void OnPlay(object sender, EventArgs e)
		{
			ViewModel.Play();
		}
	}
}
