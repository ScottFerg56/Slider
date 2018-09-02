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
				var i = Slide.Position;
				i = Pan.Position;
				var d = Slide.Speed;
				d = Pan.Speed;
				d = Slide.MaxSpeed;
				d = Pan.MaxSpeed;
				d = Slide.Acceleration;
				d = Pan.Acceleration;
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
					MaxSpeed = v;
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

		protected int _GoalPosition;
		public int GoalPosition
		{
			get => _GoalPosition;
			internal set => SetProperty(ref _GoalPosition, value);
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

		protected double? _MaxSpeed;
		public double MaxSpeed
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

		public void Move(int position, double? speed = null)
		{
			if (!speed.HasValue)
				speed = Prefix == 's' ? Comm.Settings.SlideMoveSpeed : Comm.Settings.PanMoveSpeed;
			MaxSpeed = speed.Value;
			GoalPosition = position;
			Command($"p{position}");
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

		public double MaxSpeedForDistanceAndTime(double distance, double seconds)
		{
			//
			// Calculate the speed required to move a specified distance over a specified time
			// with the acceleration already specified.
			//
			// Initial and final velocities are assumed equal to zero and acceleration and deceleration
			// values are assumed equal, though this should be easy to generalize for other cases.
			//
			// The area under a graph of velocity versus time is the distance traveled.
			//
			// The graph of velocity while accelerating is an upward-sloping line
			// and the area under the graph is a right triangle with height 'S' (the speed to solve for)
			// and length 'Ta' (the time spent accelerating)
			//
			// The graph of velocity while decelerating at the same rate is a mirror of the
			// acceleration case. So the total distance traveled while accelerating and decelerating
			// is the sum of the area of the two triangles:
			//      Dad = (1/2 • S•Ta) • 2
			//         = S•Ta
			//
			// The graph of the constant velocity between acceleration and deceleration is a horizontal
			// line and the area under the graph is a rectangle with height 'S' and length 'Ts':
			//      Ds = S•Ts
			//
			// The total distance traveled is:
			//      D = Dad + Ds
			//        = S•Ta + S•Ts
			//
			// Knowing the acceleration 'A' we can calculate Ta as:
			//      Ta = S / A
			//
			// And expressing total time as 'T':
			//      Ts = T - 2•Ta
			//
			// With substitution:
			//      D = S • (S / A) + S • (T - 2 • (S / A) )
			//        = S^2 / A + S•T - 2 • S^2 / A
			//        = -S^2 / A + S•T
			//
			// or, in quadratic form:
			//      S^2 / A - S•T + D = 0
			//
			// Solving for S as quadratic roots:
			//      S = (T ± SQRT(T^2 - 4•D/A) ) / (2 / A)
			//
			// The value of the discriminant [ T^2 - 4•D/A ] determines the number of solutions:
			//      < 0  -- No solution: not enough time to reach the distance
			//              with the given acceleration
			//      == 0 -- One solution: Ts == 0 and all the time is spent
			//              accelerating and decelerating
			//      > 0  -- Two solutions:
			//              The larger value represents an invalid solution where Ts < 0
			//              The smaller value represents a valid solution where Ts > 0
			//
			// NOTE: Using the discriminant we can calculate the minimum amount of time
			// required to travel the specified distance:
			//      T = SQRT(4 • D / A)
			//

			distance = Math.Abs(distance);
			// calculate the discriminant
			double disc = seconds * seconds - 4 * distance / Acceleration;
			double speed;
			if (disc == 0.0)
			{
				// one solution - all accelerating and decelerating
				speed = seconds / 2.0 * Acceleration;
			}
			else if (disc > 0.0)
			{
				// two solutions - use only the smaller, valid solution
				speed = (seconds - Math.Sqrt(disc)) / 2.0 * Acceleration;
			}
			else
			{
				// no solution - not enough time to get there
				// as a consolation, determine the minimum time required
				// and calculate the speed we hit after accelerating

				// calculate the time that makes the discriminant zero
				seconds = Math.Sqrt(4.0 * distance / Acceleration);
				// same as above, but the discriminant is zero
				// return a negative value so the caller knows we actually failed
				speed = -seconds / 2.0 * Acceleration; // one solution - all accelerating and decelerating
			}
			// note that this speed may exceed our SpeedLimit
			return speed;
		}

		public double TimeRemaining(double distance, double speed2 = 0)
		{
			if (Speed != 0 && distance != 0 && Math.Sign(distance) != Math.Sign(Speed))
			{
				Debug.WriteLine($"--> {Name} Not moving in the right direction, Distance: {distance} Speed: {Speed}");
				return 0;
			}
			distance = Math.Abs(distance);
			if (distance == 0)
			{
			//	Debug.WriteLine($"--> {Name} Distance is 0");
				return 0;
			}
			var speed1 = Math.Abs(Speed);
			if (speed1 == 0)
			{
			//	Debug.WriteLine($"--> {Name} Speed is 0");
				return 0;
			}
			if (Acceleration == 0)
			{
			//	Debug.WriteLine($"--> {Name} Acceleration not loaded");
				return 0;
			}

			// calculate changes in velocity for the acceleration and deceleration phases
			var deltaV1 = (MaxSpeed > speed1) ? MaxSpeed - speed1 : 0;
			var deltaV2 = (MaxSpeed > speed2) ? MaxSpeed - speed2 : 0;

			// calculate time spent in the acceleration and deceleration phases
			var ta = deltaV1 / Acceleration;
			var td = deltaV2 / Acceleration;

			// calculate distance covered by the acceleration and deceleration phases
			var da = deltaV1 * ta * 0.5;
			var dd = deltaV2 * td * 0.5;

			// calculate the distance spent traveling at the constant MaxSpeed
			var dm = distance - da - dd;
			if (dm < 0)
			{
				//	Debug.WriteLine($"--> {Name} dm < 0: {dm}");
				// there is no constant velocity phase
				// all time is spent accelerating and decelerating

				/*
				the total distance to be traveled will equal
				the sum of the distances from the acceleration and deceleration phases
				leaving an equation that can be solved for Vm = the max speed reached

				D = (1/2 • A • Ta^2) + (1/2 • A • Td^2)
					where
						D = distance to move
						A = acceleration/deceleration
						Ta = acceleration time
						Td = deceleration time
				D = (1/2 • A • (ΔVa/A)^2) + (1/2 • A • (ΔVd/A)^2)
					where
						ΔVa = change in velocity while accelerating
						ΔVd = change in velocity while decelerating
				D = (1/2 • ΔVa^2 / A) + (1/2 • ΔVd^2 / A)
				D = (1/2 • (Vm - V1)^2 / A) + (1/2 • (Vm - V2)^2 / A)
					where
						Vm is the max speed after acceleration
						V1 is the initial speed
						V2 is the final speed
				2 • D = (Vm - V1)^2 / A + (Vm - V2)^2 / A
				2 • D • A = (Vm - V1)^2 + (Vm - V2)^2
				2 • D • A = Vm^2 - 2 • Vm • V1 + V1^2 + Vm^2 - 2 • Vm • V2 + V2^2
				2 • D • A = 2 • Vm^2 - 2 • Vm • V1 - 2 • Vm • V2 + V1^2 + V2^2
				2 • D • A = 2 • Vm^2 - 2 • (V1 + V2) • Vm + V1^2 + V2^2
				0 = 2 • Vm^2 - 2 • (V1 + V2) • Vm + V1^2 + V2^2 - 2 • D • A
				*/

				// the parameters of the quadratic solution for Vm
				var a = 2;
				var b = -2 * (speed1 + speed2);
				var c = speed1 * speed1 + speed2 * speed2 - distance * 2 * Acceleration;
				// the quadratic discriminant
				var disc = b * b - 4 * a * c;

				double max;		// solving for Vm (max speed)
				if (disc == 0)
				{
					// one solution
					max = -b / (2 * a);
				//	Debug.WriteLine($"--> {Name} max: {max}");
				}
				else if (disc > 0)
				{
					// two solutions; the lesser (with the negative term) being invalid
					max = (-b + Math.Sqrt(disc)) / (2 * a);
				//	var max2 = (-b - Math.Sqrt(disc)) / (2 * a);
				//	Debug.WriteLine($"--> {Name} max1: {max}");
				}
				else
				{
					// no solution - but very close to the finish
				//	Debug.WriteLine($"--> {Name} disc: {disc}");
					return 0;
				}
				// calculate changes in velocity for the acceleration and deceleration phases
				deltaV1 = (max > speed1) ? max - speed1 : 0;
				deltaV2 = (max > speed2) ? max - speed2 : 0;
				// calculate time spent in the acceleration and deceleration phases
				ta = deltaV1 / Acceleration;
				td = deltaV2 / Acceleration;
			//	da = deltaV1 * ta * 0.5;
			//	dd = deltaV2 * td * 0.5;
			//	dm = distance - da - dd;	// should be nearly 0!
				// total the times
				var tx = ta + td;
			//	Debug.WriteLine($"--> {Name} dm: {dm} tx: {tx}");
				return tx;
			}

			// calculate the time spent at constant velocity
			var tm = dm / MaxSpeed;
			// total the times of all phases
			var t = ta + tm + td;
			if (double.IsNaN(t))
				return 0;
			return t;
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
