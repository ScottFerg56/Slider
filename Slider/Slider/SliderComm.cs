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

		private Sequence _Sequence;
		public Sequence Sequence
		{
			get
			{
				if (_Sequence == null)
				{
					_Sequence = Services.DataStore.LoadDataStore<Sequence>("sequence") ?? new Sequence();
				}
				return _Sequence;
			}
		}

		private Settings _Settings;
		public Settings Settings
		{
			get
			{
				if (_Settings == null)
				{
					_Settings = Services.DataStore.LoadDataStore<Settings>("settings") ?? new Settings();
				}
				return _Settings;
			}
		}

		public event EventHandler StateChange;

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

		protected SliderComm()
		{
			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
			Blue.InputAvailable += Blue_InputAvailable;
		}

		private void Input(string s)
		{
			// process Bluetooth input from the device
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
				var pos = Slide.Position;
				pos = Pan.Position;
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
		protected SliderComm Comm { get => SliderComm.Instance; }
		enum Properties { Prop_Position = 'p', Prop_Acceleration = 'a', Prop_Speed = 's', Prop_MaxSpeed = 'm', Prop_SpeedLimit = 'l', Prop_Homed = 'h' };

		public string Name;
		private readonly char Prefix;
		readonly int LimitMin;
		readonly int LimitMax;

		internal Stepper(string name, int limitMin, int limitMax)
		{
			Name = name;
			Prefix = char.ToLower(name[0]);
			LimitMin = limitMin;
			LimitMax = limitMax;
		}

		private static Stepper _Slide;
		internal static Stepper Slide
		{
			get
			{
				if (_Slide == null)
					_Slide = new Stepper("Slide", 0, 640);
				return _Slide;
			}
		}

		private static Stepper _Pan;
		internal static Stepper Pan
		{
			get
			{
				if (_Pan == null)
					_Pan = new Stepper("Pan", -360, 360);
				return _Pan;
			}
		}

		internal void Input(string s)
		{
			// process Bluetooth input from the device
			if (!double.TryParse(s.Substring(1), out double v))
				return;

			Debug.WriteLine($"++> {Name} {(Properties)s[0]} <- {v}");

			switch ((Properties)s[0])
			{
				case Properties.Prop_Position:
					Position = (int)Math.Round(v);
					break;
				case Properties.Prop_Acceleration:
					Acceleration = v;
					break;
				case Properties.Prop_Speed:
					Speed = v;
					break;
				case Properties.Prop_MaxSpeed:
					MaxSpeed = (int)Math.Round(v);
					break;
				case Properties.Prop_SpeedLimit:
					SpeedLimit = (int)Math.Round(v);
					break;
				case Properties.Prop_Homed:
					Homed = v != 0;
					break;
				default:
					break;
			}
		}

		void Command(string s, bool required = true)
		{
			SliderComm.Instance.Command($"{Prefix}{s}");
		}

		void SetDeviceProp(Properties prop, double v)
		{
			Command($"{(char)prop}{v:0.#}");
		}

		void RequestDeviceProp(Properties prop)
		{
			Command($"{(char)prop}?");
		}

		protected int? _Position;
		public int Position
		{
			get
			{
				if (!_Position.HasValue)
				{
					RequestDeviceProp(Properties.Prop_Position);
					return 0;
				}
				return _Position.Value;
			}
			internal set { SetProperty(ref _Position, value); }
		}

		protected double? _Acceleration;
		public double Acceleration
		{
			get
			{
				if (!_Acceleration.HasValue)
				{
					RequestDeviceProp(Properties.Prop_Acceleration);
					return 0;
				}
				return _Acceleration.Value;
			}
			internal set
			{
				SetProperty(ref _Acceleration, value, onChanged: () => SetDeviceProp(Properties.Prop_Acceleration, _Acceleration.Value));
			}
		}

		protected int? _MaxSpeed;
		public int MaxSpeed
		{
			get
			{
				if (!_MaxSpeed.HasValue)
				{
					RequestDeviceProp(Properties.Prop_MaxSpeed);
					return 0;
				}
				return _MaxSpeed.Value;
			}
			internal set
			{
				SetProperty(ref _MaxSpeed, value, onChanged: () => SetDeviceProp(Properties.Prop_MaxSpeed, _MaxSpeed.Value));
			}
		}

		protected int? _SpeedLimit;
		public int SpeedLimit
		{
			get
			{
				if (!_SpeedLimit.HasValue)
				{
					RequestDeviceProp(Properties.Prop_SpeedLimit);
					return 0;
				}
				return _SpeedLimit.Value;
			}
			internal set
			{
				SetProperty(ref _SpeedLimit, value, onChanged: () => SetDeviceProp(Properties.Prop_SpeedLimit, _SpeedLimit.Value));
			}
		}

		public double Vector
		{
			set
			{
				var speed = Math.Round(value, 1);
				Command($"v{speed:0.#}", Math.Abs(speed) < 0.01);
			}
		}

		protected double? _Speed;
		public double Speed
		{
			get
			{
				if (!_Speed.HasValue)
				{
					RequestDeviceProp(Properties.Prop_Speed);
					return 0;
				}
				return _Speed.Value;
			}
			internal set => SetProperty(ref _Speed, value);
		}

		public void Move(double position)
		{
			var speed = Prefix == 's' ? Comm.Settings.SlideMoveSpeed : Comm.Settings.PanMoveSpeed;
			MaxSpeed = speed;
			Command($"p{position:0.#}");
		}

		protected bool? _Homed = null;
		public bool Homed
		{
			get
			{
				Debug.Assert(Prefix == 's', "--> Homed only valid for Slide");
				if (!_Homed.HasValue)
				{
					RequestDeviceProp(Properties.Prop_Homed);
					return false;
				}
				return _Homed.Value;
			}
			set { SetProperty(ref _Homed, value); }
		}

		public void Zero()
		{
			Debug.Assert(Prefix == 'p', "--> Zero only valid for Pan");
			Command("z1");
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

		public int LimitValue(int v)
		{
			return Math.Max(Math.Min(v, LimitMax), LimitMin);
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
