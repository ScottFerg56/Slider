/*
OO   OO            OO              O               O       OO              OO      O
OOO OOO            OO             OOO             OO       OO              OO     OO
OOOOOOO                          OO OO            OO                              OO
OOOOOOO  OOOO     OOO   OO OOO  OO   OO  OOOOO  OOOOOO    OOO    OO  OO   OOO   OOOOOO  OO   OO
OO O OO     OO     OO    OOOOOO OO   OO OO   OO   OO       OO    OO  OO    OO     OO    OO   OO
OO   OO  OOOOO     OO    OO  OO OOOOOOO OO        OO       OO    OO  OO    OO     OO    OO   OO
OO   OO OO  OO     OO    OO  OO OO   OO OO        OO       OO    OO  OO    OO     OO    OO   OO
OO   OO OO  OO     OO    OO  OO OO   OO OO   OO   OO OO    OO     OOOO     OO     OO OO  OOOOOO
OO   OO  OOO OO   OOOO   OO  OO OO   OO  OOOOO     OOO    OOOO     OO     OOOO     OOO       OO
                                                                                            OO
                                                                                        OOOOO
	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;

namespace CamSlider.Droid
{
    [Activity(Label = "Slider", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

			if (this.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
			{
				// required for Bluetooth LE
				RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation }, 0);
			}

			global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}
