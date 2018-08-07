using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CamSlider
{
	public class SliderComm
	{
		BlueApp Blue;

		public event EventHandler StateChange;

		protected SliderComm()
		{
			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
			Blue.InputAvailable += Blue_InputAvailable;
		}

		private void Command(string s)
		{
			Debug.WriteLine($"Blue input: {s}");
			switch (s[0])
			{
				case 's':
					Stepper.Slide.Command(s.Substring(1));
					break;
				case 'p':
					Stepper.Pan.Command(s.Substring(1));
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
					Command(Buffer);
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

		public void Connect(string name) => Blue.Connect(name);
		public void Disconnect() => Blue.Disconnect();
		public BlueState State { get => Blue.State; }
		public bool CanConnect { get => Blue.CanConnect; }
		public string ErrorMessage { get => Blue.ErrorMessage; }

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

		public void SetSlideVector(double speed)
		{
			if (SliderComm.Instance.State != BlueState.Connected)
				return;
			speed = Math.Round(speed, 1);
			Blue.Write($"sv{speed:0.#};", Math.Abs(speed) < 0.01);
		}

		public void SetPanVector(double speed)
		{
			if (SliderComm.Instance.State != BlueState.Connected)
				return;
			speed = Math.Round(speed, 1);
			Blue.Write($"pv{speed:0.#};", Math.Abs(speed) < 0.01);
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			StateChange?.Invoke(this, e);
		}
	}

	public class Stepper : INotifyPropertyChanged
	{
		protected Stepper()
		{
		}

		private static Stepper _Slide;
		public static Stepper Slide
		{
			get
			{
				if (_Slide == null)
					_Slide = new Stepper();
				return _Slide;
			}
		}

		private static Stepper _Pan;
		public static Stepper Pan
		{
			get
			{
				if (_Pan == null)
					_Pan = new Stepper();
				return _Pan;
			}
		}

		internal void Command(string s)
		{
			switch (s[0])
			{
				default:
					{
						if (double.TryParse(s, out double pos))
						{
							SetProperty(ref _Position, pos, nameof(Position));
						}
					}
					break;
			}
		}

		protected double _Position;
		public double Position
		{
			get { return _Position; }
			set { SetProperty(ref _Position, value); }
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
}
