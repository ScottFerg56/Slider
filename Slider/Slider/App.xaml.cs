/*
   O
  OOO
 OO OO
OO   OO OO OOO  OO OOO
OO   OO  OO  OO  OO  OO
OOOOOOO  OO  OO  OO  OO
OO   OO  OO  OO  OO  OO
OO   OO  OOOOO   OOOOO
OO   OO  OO      OO
         OO      OO
        OOOO    OOOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using System;
using Xamarin.Forms;
using CamSlider.Views;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CamSlider
{
	public partial class App : Application
	{
		
		public App ()
		{
			InitializeComponent();


			MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
