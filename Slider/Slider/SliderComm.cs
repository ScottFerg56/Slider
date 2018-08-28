using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CamSlider
{
	public class SliderComm : INotifyPropertyChanged
	{
		BlueApp Blue;

		public readonly Settings Settings;

		public event EventHandler StateChange;

		protected SliderComm()
		{
			Settings = Services.DataStore.LoadDataStore<Settings>("settings") ?? new Settings();

			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
			Blue.InputAvailable += Blue_InputAvailable;
		}

		private void Input(string s)
		{
			// process Bluetooth input from the device
			Debug.WriteLine($"++> Blue input: {s}");
			switch (s[0])
			{
				case 's':
					Slide.Input(s.Substring(1));
					break;
				case 'p':
					Pan.Input(s.Substring(1));
					break;
				default:
					break;
			}
		}

		private string Buffer = "";

		private void Blue_InputAvailable(object sender, EventArgs e)
		{
			// packet format: [ '=' | data length byte | section byte | data ]
			// data part may start with a section command character
			while (Blue.ByteAvailable)
			{
				char c = (char)Blue.GetByte();
				if (c == ';')
				{
					Input(Buffer);
					Buffer = "";
				}
				else
				{
					Buffer += c;
				}
			}
		}

		private static SliderComm _Instance;
		public static SliderComm Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new SliderComm();
				return _Instance;
			}
		}

		public Stepper Slide { get => Stepper.Slide; }
		public Stepper Pan { get => Stepper.Pan; }

		public void Connect(string name) => Blue.Connect(name);
		public void Disconnect() => Blue.Disconnect();
		public BlueState State { get => Blue.State; }
		public bool CanConnect { get => Blue.CanConnect; }
		public string ErrorMessage { get => Blue.ErrorMessage; }

		public void Command(string cmd, bool required = true)
		{
			Debug.WriteLine($"++> Blue command: {cmd}");
			Blue.Write(cmd + ';', required);
		}

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

		private void Blue_StateChange(object sender, EventArgs e)
		{
			if (SliderComm.Instance.State == BlueState.Connected)
			{
				// trigger updated values from the device
				var pos = Slide.Position.ToString();
				pos = Pan.Position.ToString();
				var homed = Slide.Homed;
			}
			StateChange?.Invoke(this, e);
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	public class Stepper : INotifyPropertyChanged
	{
		private readonly char Prefix;
		internal Stepper(char prefix)
		{
			Prefix = prefix;
		}

		private static Stepper _Slide;
		internal static Stepper Slide
		{
			get
			{
				if (_Slide == null)
					_Slide = new Stepper('s');
				return _Slide;
			}
		}

		private static Stepper _Pan;
		internal static Stepper Pan
		{
			get
			{
				if (_Pan == null)
					_Pan = new Stepper('p');
				return _Pan;
			}
		}

		internal void Input(string s)
		{
			// process Bluetooth input from the device
			switch (s[0])
			{
				case 'p':
					{
						// receiving current position from device
						if (double.TryParse(s.Substring(1), out double pos))
						{
							Position = pos;
						}
					}
					break;

				case 'h':
					{
						// receiving current homed value from device
						if (int.TryParse(s.Substring(1), out int homed))
						{
							Homed = homed != 0;
						}
					}
					break;
			}
		}

		protected double _Position = double.NaN;
		public double Position
		{
			get
			{
				if (double.IsNaN(_Position))
				{
					// send device query for position value for this stepper, by Prefix ('s' or 'p')
					SliderComm.Instance.Command($"{Prefix}p?");
					// client should be monitoring property change to update value when it comes in
					return 0.0;
				}
				return _Position;
			}
			internal set { SetProperty(ref _Position, value); }
		}

		public double Vector
		{
			set
			{
				var speed = Math.Round(value, 1);
				SliderComm.Instance.Command($"{Prefix}v{speed:0.#}", Math.Abs(speed) < 0.01);
			}
		}

		protected bool? _Homed = null;
		public bool Homed
		{
			get
			{
				Debug.Assert(Prefix == 's', "--> Homed only valid for Slide");
				if (!_Homed.HasValue)
				{
					// send device query for Homed value for this stepper, by Prefix ('s' or 'p')
					SliderComm.Instance.Command($"{Prefix}h?");
					// client should be monitoring property change to update value when it comes in
					return false;
				}
				return _Homed.Value;
			}
			set { SetProperty(ref _Homed, value); }
		}

		public void Zero()
		{
			Debug.Assert(Prefix == 'p', "--> Zero only valid for Pan");
			SliderComm.Instance.Command($"{Prefix}z");
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		public static int LimitSlideValue(int v)
		{
			return Math.Max(Math.Min(v, 640), 0);
		}

		public static int LimitPanValue(int v)
		{
			return Math.Max(Math.Min(v, 360), -360);
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
