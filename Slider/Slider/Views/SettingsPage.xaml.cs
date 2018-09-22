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