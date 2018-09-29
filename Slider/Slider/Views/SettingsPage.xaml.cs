/*
 OOOOO             O       O       OO                           OOOOOO
OO   OO           OO      OO       OO                            OO  OO
OO   OO           OO      OO                                     OO  OO
 OO      OOOOO  OOOOOO  OOOOOO    OOO   OO OOO   OOO OO  OOOOO   OO  OO  OOOO    OOO OO  OOOOO
  OOO   OO   OO   OO      OO       OO    OOOOOO OO  OO  OO   OO  OOOOO      OO  OO  OO  OO   OO
    OO  OOOOOOO   OO      OO       OO    OO  OO OO  OO   OOO     OO      OOOOO  OO  OO  OOOOOOO
OO   OO OO        OO      OO       OO    OO  OO OO  OO     OOO   OO     OO  OO  OO  OO  OO
OO   OO OO   OO   OO OO   OO OO    OO    OO  OO  OOOOO  OO   OO  OO     OO  OO   OOOOO  OO   OO
 OOOOO   OOOOO     OOO     OOO    OOOO   OO  OO     OO   OOOOO  OOOO     OOO OO     OO   OOOOO
                                                OO  OO                          OO  OO
                                                 OOOO                            OOOO
	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

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