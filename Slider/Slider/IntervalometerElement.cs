using System;
using System.Collections.Generic;
using System.Text;

namespace CamSlider
{
	/// <summary>
	/// Represents an intervalometer function on the remote device.
	/// </summary>
	public class IntervalometerElement : RemoteElement
	{
		public IntervalometerElement(IRemoteMaster master, string name, char prefix) : base(master, name, prefix)
		{
		}

		/// <summary>Get/set the delay required between triggering camera focus and tripping the shutter.</summary>
		[ElementProperty('d', readOnly: false, noRequest: true)]
		public uint FocusDelay { get => GetProperty<uint>(); set => SetProperty(value); }

		/// <summary>Get/set the hold time required for the signal tripping the shutter.</summary>
		[ElementProperty('s', readOnly: false, noRequest: true)]
		public uint ShutterHold { get => GetProperty<uint>(); set => SetProperty(value); }

		/// <summary>Get/set the interval time between frames, in milliseconds.</summary>
		[ElementProperty('i', readOnly: false)]
		public uint Interval { get => GetProperty<uint>(); set => SetProperty(value); }

		/// <summary>Get/set the number of frames to be captured for the intervalometer operation.</summary>
		/// <remarks>Setting Frames to a non-zero value will start the intrvalometer operation on the device.</remarks>
		[ElementProperty('f', readOnly: false)]
		public uint Frames { get => GetProperty<uint>(); set => SetProperty(value); }
	}
}
