/*
OO   OO            OO           OOOOOO
OOO OOO            OO            OO  OO
OOOOOOO                          OO  OO
OOOOOOO  OOOO     OOO   OO OOO   OO  OO  OOOO    OOO OO  OOOOO
OO O OO     OO     OO    OOOOOO  OOOOO      OO  OO  OO  OO   OO
OO   OO  OOOOO     OO    OO  OO  OO      OOOOO  OO  OO  OOOOOOO
OO   OO OO  OO     OO    OO  OO  OO     OO  OO  OO  OO  OO
OO   OO OO  OO     OO    OO  OO  OO     OO  OO   OOOOO  OO   OO
OO   OO  OOO OO   OOOO   OO  OO OOOO     OOO OO     OO   OOOOO
                                                OO  OO
                                                 OOOO
	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : TabbedPage
	{
		public MainPage ()
		{
			InitializeComponent ();
		}
	}
}