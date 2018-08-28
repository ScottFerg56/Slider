using CamSlider.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
		SettingsViewModel ViewModel;

		public SettingsPage ()
		{
			InitializeComponent ();

			BindingContext = ViewModel = new SettingsViewModel();
		}
	}
}