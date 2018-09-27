//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using CamSlider.ViewModels;
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