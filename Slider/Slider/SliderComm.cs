//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CamSlider
{
	/// <summary>
	/// Manages communication and control state with the camera slider device.
	/// </summary>
	public class SliderComm : IRemoteMaster
	{
		/// <summary>
		/// The platform-independent Bluetooth implementation.
		/// </summary>
		BlueApp Blue;

		private Sequence _Sequence;
		/// <summary>
		/// Gets the Sequence that defines the In point and Out point and other parameters for the camera slider movement.
		/// </summary>
		public Sequence Sequence
		{
			get
			{
				if (_Sequence == null)
				{
					// load from the data store
					_Sequence = Services.DataStore.LoadDataStore<Sequence>("sequence") ?? new Sequence();
				}
				return _Sequence;
			}
		}

		private Settings _Settings;
		/// <summary>
		/// Gets the Settings related to the camera slider hardware and its performance characteristics.
		/// </summary>
		public Settings Settings
		{
			get
			{
				if (_Settings == null)
				{
					// load from the data store
					_Settings = Services.DataStore.LoadDataStore<Settings>("settings") ?? new Settings();
				}
				return _Settings;
			}
		}

		/// <summary>
		/// Fired when the Bluetooth connection state changes.
		/// </summary>
		public event EventHandler StateChange;

		private static SliderComm _Instance;
		/// <summary>
		/// Get the single instance of the SliderComm class.
		/// </summary>
		public static SliderComm Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new SliderComm();
				return _Instance;
			}
		}

		/// <summary>Get the Global instance.</summary>
		public readonly GlobalElement Global;

		/// <summary>Get the Slide stepper instance.</summary>
		public readonly StepperElement Slide;

		/// <summary>Get the Pan stepper instance.</summary>
		public readonly StepperElement Pan;

		/// <summary>Get the Intervalometer stepper instance.</summary>
		public readonly IntervalometerElement Intervalometer;

		private List<RemoteElement> Elements = new List<RemoteElement>();

		protected SliderComm()
		{
			// initialize Bluetooth, but don't connect yet
			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
			Blue.InputAvailable += Blue_InputAvailable;
			Elements.Add(Global = new GlobalElement(this, "Global", 'g'));
			Elements.Add(Slide = new StepperElement(this, "Slide", 's', 0, 640));
			Elements.Add(Pan = new StepperElement(this, "Pan", 'p', -360, 360));
			Elements.Add(Intervalometer = new IntervalometerElement(this, "Intervalometer", 'i'));
		}

		/// <summary>
		/// Process an input string from the Bluetooth device.
		/// </summary>
		/// <param name="s">The input string.</param>
		/// <remarks>
		/// The first character in the string indicates the Prefix of the RemoteElement being accessed.
		/// The second character indicates the property to be accessed.
		/// The third character is a value for the property.
		/// </remarks>
		private void Input(string s)
		{
			if (string.IsNullOrEmpty(s))
				return;
			Debug.WriteLine($"Input string: {s}");
			var element = Elements.FirstOrDefault(e => e.Prefix == s[0]);
			if (element == null)
			{
				Debug.WriteLine($"--> Unknown device class: {s[0]}");
			}
			else
			{
				element.Input(s.Substring(1));
			}
		}

		private string Buffer = "";

		/// <summary>
		/// Process input characters from the Blutooth device.
		/// </summary>
		private void Blue_InputAvailable(object sender, EventArgs e)
		{
			while (Blue.ByteAvailable)
			{
				// buffer characters looking for a terminator
				char c = (char)Blue.GetByte();
				if (c == ';')
				{
					// ';' terminator found, process the input
					Input(Buffer);
					// clear the buffer
					Buffer = "";
				}
				else
				{
					// add to the buffer
					Buffer += c;
				}
			}
		}

		/// <summary>
		/// Connect to the remote device by name.
		/// </summary>
		/// <param name="name">The name of the device to connect to.</param>
		public void Connect(string name) => Blue.Connect(name);

		/// <summary>
		/// Disconnect from the device.
		/// </summary>
		public void Disconnect() => Blue.Disconnect();

		/// <summary>
		/// Gets the connection state.
		/// </summary>
		public BlueState State { get => Blue.State; }

		/// <summary>
		/// True if the device is in a state where connection is possible.
		/// </summary>
		public bool CanConnect { get => Blue.CanConnect; }

		/// <summary>
		/// Gets an error message associated with the last error.
		/// </summary>
		public string ErrorMessage { get => Blue.ErrorMessage; }

		/// <summary>
		/// Send a command string to the device.
		/// </summary>
		/// <param name="cmd">The command string.</param>
		/// <param name="required">False for non-essential data that can be skipped.</param>
		/// <remarks>
		/// The first character in the string indicates the Prefix of the RemoteElement being accessed.
		/// The second character indicates a property to be accessed.
		/// The third character and beyond may be a value for the property or a '?' to request the property value from the device.
		/// The final character is a ';' terminator.
		/// </remarks>
		public void Output(string cmd, bool required = true)
		{
		//	Debug.WriteLine($"++> Blue command: {cmd}");
			// terminate and write the command
			Blue.Write(cmd + ';', required);
		}

		/// <summary>
		/// Get a string combining the connection state with any available error message.
		/// </summary>
		public string StateText
		{
			get
			{
				string state = Blue.State.ToString();
				if (!string.IsNullOrWhiteSpace(ErrorMessage))
					state += " - " + ErrorMessage;
				return state;
			}
		}

		/// <summary>
		/// Respond to changes in the Bluetooth state.
		/// </summary>
		private void Blue_StateChange(object sender, EventArgs e)
		{
			if (State == BlueState.Connected)
			{
				// trigger updated values from the device
				foreach (var element in Elements)
				{
					element.RequestAllDeviceProps();
				}
			}
			// propagate state change to our clients
			StateChange?.Invoke(this, e);
		}
	}
}
