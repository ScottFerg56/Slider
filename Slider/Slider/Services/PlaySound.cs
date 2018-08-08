using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Slider.Services
{
    public static class PlaySound
    {
		public static void Play(string name)
		{
			var assembly = typeof(PlaySound).Assembly;
			System.IO.Stream audioStream = assembly.GetManifestResourceStream("Slider.Resources." + name + ".mp3");

			var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
			if (player.IsPlaying)
				player.Stop();
			try
			{
				player.Load(audioStream);
				player.Play();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Play exception: {ex}");
			}
		}
	}
}
