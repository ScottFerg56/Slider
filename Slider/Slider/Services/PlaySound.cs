/*
OOOOOO    OOO                    OOOOO                             OOO
 OO  OO    OO                   OO   OO                             OO
 OO  OO    OO                   OO   OO                             OO
 OO  OO    OO    OOOO   OO   OO  OO      OOOOO  OO  OO  OO OOO    OOOO
 OOOOO     OO       OO  OO   OO   OOO   OO   OO OO  OO   OOOOOO  OO OO
 OO        OO    OOOOO  OO   OO     OO  OO   OO OO  OO   OO  OO OO  OO
 OO        OO   OO  OO  OO   OO OO   OO OO   OO OO  OO   OO  OO OO  OO
 OO        OO   OO  OO   OOOOOO OO   OO OO   OO OO  OO   OO  OO OO  OO
OOOO      OOOO   OOO OO      OO  OOOOO   OOOOO   OOO OO  OO  OO  OOO OO
                            OO
                        OOOOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using System;
using System.Diagnostics;

namespace CamSlider.Services
{
	/// <summary>
	/// Uses the Xam.Plugin.SimpleAudioPlayer to play sounds.
	/// </summary>
	/// <remarks>
	/// https://www.nuget.org/packages/Xam.Plugin.SimpleAudioPlayer
	/// </remarks>
	public static class PlaySound
    {
		/// <summary>
		/// Play an MP3 sound file from the Resources folder.
		/// </summary>
		/// <param name="name">Base name (without extension) of the sound file to play.</param>
		public static void Play(string name)
		{
			// get the MP3 as a resource stream
			var assembly = typeof(PlaySound).Assembly;
			System.IO.Stream audioStream = assembly.GetManifestResourceStream("Slider.Resources." + name + ".mp3");

			// get the player and stop anything still going
			var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
			if (player.IsPlaying)
				player.Stop();
			try
			{
				// load and play the sound
				player.Load(audioStream);
				player.Play();
			}
			catch (Exception ex)
			{
				// getting the occasional/flaky exception here with very little information to investigate
				// not critical, so just ignoring for now
				Debug.WriteLine($"--> Play exception: {ex.Message}");
			}
		}
	}
}
