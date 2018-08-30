using CamSlider.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RunPage : ContentPage
	{
		RunViewModel ViewModel;

		private static RunPage _Instance;
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

		RunCommand Command;
		bool Closed;

		public void Init(RunCommand cmd)
		{
			Command = cmd;
			Closed = false;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Init(Command);
		}

		private async void ViewModel_Stopped(object sender, EventArgs e)
		{
			if (!Closed)
			{
				await Navigation.PopModalAsync();
				Closed = true;
			}
		}

		private void OnStop(object sender, EventArgs e)
		{
			ViewModel.Stop();	// this invokes our Stopped event
		}

		private void OnPlay(object sender, EventArgs e)
		{

		}
	}
}