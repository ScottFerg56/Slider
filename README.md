![Slider Photo](https://www.forestmoon.com/Piwigo/_data/i/galleries/Projects/Slider/Android%20UI%20-%20Sequence%20Page-me.jpg)

# Slider
**Slider** is a Cross-platform (Android and UWP) implementation of a control app for the DIY automated camera slider, including pan and intervalometer functions, implemented for Arduino in the mSlider project.

The software is provided as a [Visual Studio](https://visualstudio.microsoft.com/free-developer-offers/) solution, with a free Community version of the tools available.

The [**Slider gallery**](https://www.forestmoon.com/Piwigo/index.php?/category/Slider) Shows lots of construction and overview information, including screen shots of the control program. See details in the captions.

This project requires other reusable libraries found in the **Libs** companion project:
* **Elements** - A framework for organizing and communicating with hardware components implemented on Arduino using the Applet framework found in the mLibs project.
* **Platforms/Portable** - Reusable components for functionality that is platform-independent.
* **Platforms/Android** - Reusable components for functionality that is dependent on the Android platform.
* **Platforms/UWP** - Reusable components for functionality that is dependent on the Universal Windows Platform (UWP).

Place the Slider and Libs projects in sibling subdirectories in your file system to insure proper compilation.

The **Slider** acts as a controlling application that communicates via Bluetooth LE with the Slider hardware using Android software implemented in the **mSlider** companion project.
