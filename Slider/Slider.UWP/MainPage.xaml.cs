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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CamSlider.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new CamSlider.App());
        }
    }
}
