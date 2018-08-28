using Slider.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Slider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RunPage : ContentPage
	{
		public RunPage ()
		{
			InitializeComponent ();

			BindingContext = new RunViewModel();
		}

		private async void OnStop(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		private void OnPlay(object sender, EventArgs e)
		{

		}
	}
}