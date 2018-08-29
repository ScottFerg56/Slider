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

		public void Init(RunCommand cmd)
		{
			Command = cmd;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Init(Command);
		}

		private void ViewModel_Stopped(object sender, EventArgs e)
		{
			OnStop(sender, e);
		}

		private async void OnStop(object sender, EventArgs e)
		{
			ViewModel.Stop();
			await Navigation.PopModalAsync();
		}

		private void OnPlay(object sender, EventArgs e)
		{

		}
	}
}