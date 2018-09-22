using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CamSlider
{
	/// <summary>
	/// Manages communication and control state with the camera slider device.
	/// </summary>
	public class SliderComm : INotifyPropertyChanged
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

		protected SliderComm()
		{
			// initialize Bluetooth, but don't connect yet
			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
			Blue.InputAvailable += Blue_InputAvailable;
		}

		/// <summary>
		/// Process an input string from the Bluetooth device.
		/// </summary>
		/// <param name="s">The input string.</param>
		/// <remarks>
		/// The Command string will be recognized with an initial character valid for related devices.
		/// Valid first characters:
		///		'g' - global variables
		///		'c' - camera
		///		's' - Slide
		///		'p' - Pan
		/// The second character indicates a property to be accessed or an action.
		/// The third character will be a value for the property.
		/// </remarks>
		private void Input(string s)
		{
			switch (s[0])
			{
				case 'g':	// Global
					GlobalInput(s.Substring(1));
					break;
				case 's':	// Slide
					Slide.Input(s.Substring(1));
					break;
				case 'p':	// Pan
					Pan.Input(s.Substring(1));
					break;
				case 'c':	// Camera
					Camera.Input(s.Substring(1));
					break;
				default:
					Debug.WriteLine($"--> Unknown device class: {s[0]}");
					break;
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

		/// <summary>Global properties exposed to the Command interface.</summary>
		/// <remarks>The enum values represent the character codes used in the Command strings.</remarks>
		enum Globals { Global_Action = 'a' };

		/// <summary>
		/// Process input for Global properties.
		/// </summary>
		/// <param name="s">The input string, starting with the character indicating the property involved.</param>
		internal void GlobalInput(string s)
		{
			// parse the value that follows the property character
			if (!double.TryParse(s.Substring(1), out double v))
				return;

		//	Debug.WriteLine($"++> Global {(Globals)s[0]} <- {v}");

			// Invoke SetProperty directly without the onChanged action
			// so we don't needlessly reflect the same value back to the device.
			switch ((Globals)s[0])
			{
				case Globals.Global_Action:
					SetProperty(ref _Action, (Actions)(int)v, "Action");
					break;
			}
		}

		/// <summary>
		/// Send a Global property value to the device.
		/// </summary>
		/// <param name="prop">The property to send.</param>
		/// <param name="v">The value to send.</param>
		void SetDeviceGlobal(Globals prop, double v)
		{
			Command($"g{(char)prop}{v:0.#}");
		}

		/// <summary>
		/// Request the value of a Global property from the device.
		/// </summary>
		/// <param name="prop">The property to request.</param>
		void RequestDeviceGlobal(Globals prop)
		{
			Command($"g{(char)prop}?");
		}

		/// <summary>
		/// Request the values of all Global properties from the device.
		/// </summary>
		public void RequestAllDeviceGlobals()
		{
			foreach (var prop in (Globals[])Enum.GetValues(typeof(Globals)))
			{
				RequestDeviceGlobal(prop);
			}
		}

		/// <summary>
		/// Run Sequence Actions that are in progress.
		/// </summary>
		public enum Actions
		{
			/// <summary>no Action</summary>
			None,
			/// <summary>Moving to In point</summary>
			MovingToIn,
			/// <summary>Moving to Out point</summary>
			MovingToOut,
			/// <summary>Running Sequence</summary>
			Running,
			/// <summary>Unknown / not initialized</summary>
			Unknown
		};

		protected Actions _Action = Actions.Unknown;	// unknown/uninitiaized
		/// <summary>
		/// Get/set a Global variable saved on the device to be retrieved when recovering
		/// from a disconnection.
		/// </summary>
		public Actions Action
		{
			get
			{
				if (_Action == Actions.Unknown)
				{
					RequestDeviceGlobal(Globals.Global_Action);
					return 0;
				}
				return _Action;
			}
			internal set => SetProperty(ref _Action, value, onChanged: () => SetDeviceGlobal(Globals.Global_Action, (int)value));
		}

		/// <summary>
		/// Get the Slide stepper instance.
		/// </summary>
		public Stepper Slide { get => Stepper.Slide; }

		/// <summary>
		/// Get the Pan stepper instance.
		/// </summary>
		public Stepper Pan { get => Stepper.Pan; }

		/// <summary>
		/// Get the Camera object.
		/// </summary>
		public Camera Camera { get => Camera.Cam; }

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
		/// The Command string begins with an initial character indicating the desired device:
		///		'g' - global variables
		///		'c' - camera
		///		's' - Slide
		///		'p' - Pan
		/// The second character indicates a property to be accessed or an action.
		/// The third character and beyond may be a value for the property (or action) or a '?' to retrieve the property value.
		/// The final character is a ';' terminator.
		/// </remarks>
		public void Command(string cmd, bool required = true)
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
				RequestAllDeviceGlobals();
				Slide.RequestAllDeviceProps();
				Pan.RequestAllDeviceProps();
				Camera.RequestAllDeviceProps();
			}
			// propagate state change to our clients
			StateChange?.Invoke(this, e);
		}

		/// <summary>
		/// Process the setting of a property including setting the backing store,
		/// notifying INotifyPropertyChanged clients and performing
		/// other arbitrary actions when a change is detected.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="backingStore">A reference to the property's backing store.</param>
		/// <param name="value">The value to be assigned.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="onChanged">An Action to be invoked when a change is detected.</param>
		/// <returns>True if the value changed.</returns>
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
		/// <summary>
		/// Fired when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fire an event for a property changing.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	/// <summary>
	/// Represents a stepper used to control the Slide and Pan functions on the device.
	/// </summary>
	public class Stepper : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		/// <summary>Stepper properties exposed to the Command interface.</summary>
		/// <remarks>The enum values represent the character codes used in the Command strings.</remarks>
		enum Properties
		{
			Prop_Position = 'p',
			Prop_Acceleration = 'a',
			Prop_Speed = 's',
			Prop_MaxSpeed = 'm',
			Prop_SpeedLimit = 'l',
			Prop_Homed = 'h',
			Prop_TargetPosition = 't'
		};

		public string Name;				// friendly name for the stepper
		private readonly char Prefix;	// character prefix used in interface command strings
		readonly int LimitMin;			// lower limit for movement
		readonly int LimitMax;			// upper limit for movement

		protected Stepper(string name, int limitMin, int limitMax)
		{
			Name = name;
			Prefix = char.ToLower(name[0]);
			LimitMin = limitMin;
			LimitMax = limitMax;
		}

		private static Stepper _Slide;
		/// <summary>
		/// Get the Slide stepper instance.
		/// </summary>
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
		/// <summary>
		/// Get the Pan stepper instance.
		/// </summary>
		internal static Stepper Pan
		{
			get
			{
				if (_Pan == null)
					_Pan = new Stepper("Pan", -360, 360);
				return _Pan;
			}
		}

		/// <summary>
		/// Process an input string from the Bluetooth device.
		/// </summary>
		/// <param name="s">The input string.</param>
		/// <remarks>
		/// The first character of the string indicates a property to be accessed or an action.
		/// The remainder of the string will be a value for the property.
		/// </remarks>
		internal void Input(string s)
		{
			// parse the value from the remainder of the string
			if (!double.TryParse(s.Substring(1), out double v))
				return;

			var prop = (Properties)s[0];

		//	Debug.WriteLine($"++> {Name} {prop} <- {v}");
			// keeping track of properties we've requested values for, to avoid redundant requests
			if (requestedProps.Contains(prop))
				requestedProps.Remove(prop);

			// Invoke SetProperty directly without the onChanged action
			// so we don't needlessly reflect the same value back to the device.
			switch (prop)
			{
				case Properties.Prop_Position:
					SetProperty(ref _Position, (int)Math.Round(v), "Position");
					break;
				case Properties.Prop_Acceleration:
					SetProperty(ref _Acceleration, v, "Acceleration");
					break;
				case Properties.Prop_Speed:
					SetProperty(ref _Speed, v, "Speed");
					break;
				case Properties.Prop_MaxSpeed:
					SetProperty(ref _MaxSpeed, v, "MaxSpeed");
					break;
				case Properties.Prop_SpeedLimit:
					SetProperty(ref _SpeedLimit, (uint)Math.Round(v), "SpeedLimit");
					break;
				case Properties.Prop_Homed:
					SetProperty(ref _Homed, v != 0, "Homed");
					break;
				case Properties.Prop_TargetPosition:
					SetProperty(ref _TargetPosition, (int)Math.Round(v), "TargetPosition");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Send a command string to the device.
		/// </summary>
		/// <param name="cmd">The command string.</param>
		/// <param name="required">False for non-essential data that can be skipped.</param>
		void Command(string cmd, bool required = true)
		{
			Comm.Command($"{Prefix}{cmd}", required);
		}

		/// <summary>
		/// Send a property value to the device.
		/// </summary>
		/// <param name="prop">The property to be set.</param>
		/// <param name="v">The value to be set.</param>
		void SetDeviceProp(Properties prop, double v)
		{
			Command($"{(char)prop}{v:0.#}");
		}

		/// <summary>
		/// A list to track requested property values to avoid redundant requests
		/// that would burden the Bluetooth bandwidth.
		/// </summary>
		List<Properties> requestedProps = new List<Properties>();

		/// <summary>
		/// Request a property value from the device.
		/// </summary>
		/// <param name="prop">The property to request.</param>
		void RequestDeviceProp(Properties prop)
		{
			if (requestedProps.Contains(prop))	// skip a redundant request
				return;
			requestedProps.Add(prop);			// note the request has been made
			// the '?' following the property code requests the value rather than set it
			Command($"{(char)prop}?");
		}

		/// <summary>
		/// Request all properties from the device.
		/// </summary>
		public void RequestAllDeviceProps()
		{
			requestedProps.Clear();		// force requests for all
			foreach (var prop in (Properties[])Enum.GetValues(typeof(Properties)))
			{
				RequestDeviceProp(prop);
			}
		}

		protected int? _Position;
		/// <summary>
		/// Gets the Position of the stepper.
		/// </summary>
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

		protected int? _TargetPosition;
		/// <summary>
		/// Gets the Target Position the stepper is moving toward.
		/// </summary>
		public int TargetPosition
		{
			get
			{
				if (!_TargetPosition.HasValue)
				{
					RequestDeviceProp(Properties.Prop_TargetPosition);
					return 0;
				}
				return _TargetPosition.Value;
			}
			internal set => SetProperty(ref _TargetPosition, value);
		}

		protected double? _Acceleration;
		/// <summary>
		/// Get/set the Acceleration used by the stepper.
		/// </summary>
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
		/// <summary>
		/// Get/set the Max Speed the stepper will accelerate to for movements.
		/// </summary>
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

		protected uint? _SpeedLimit;
		/// <summary>
		/// Get the upper limit for MaxSpeed.
		/// </summary>
		public uint SpeedLimit
		{
			get
			{
				if (!_SpeedLimit.HasValue)
				{
					RequestDeviceProp(Properties.Prop_SpeedLimit);
					return 9999;
				}
				return _SpeedLimit.Value;
			}
		}

		/// <summary>
		/// Set the Velocity vector, a signed speed to move the stepper in a given direction,
		/// specified as a percentage of the SpeedLimit.
		/// </summary>
		public double Velocity
		{
			set
			{
				var speed = Math.Round(value, 1);
				Command($"v{speed:0.#}", speed == 0);
			}
		}

		protected double? _Speed;
		/// <summary>
		/// Get the current Speed of the stepper.
		/// </summary>
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

		/// <summary>
		/// Move the stepper to a position, accelerating to a desired speed.
		/// </summary>
		/// <param name="position">The desired target position.</param>
		/// <param name="speed">An optional desired speed, defaulting to the stepper's MoveSpeed from Settings.</param>
		public void Move(int position, double? speed = null)
		{
			if (!speed.HasValue)
				speed = Prefix == 's' ? Comm.Settings.SlideMoveSpeed : Comm.Settings.PanMoveSpeed;
			// make sure the Acceleration is what's specified in the settings
			Acceleration = Prefix == 's' ? Comm.Settings.SlideAcceleration : Comm.Settings.PanAcceleration;
			MaxSpeed = speed.Value;
			TargetPosition = position;
			Command($"p{position}");
		}

		protected bool? _Homed = null;
		/// <summary>
		/// Get Homed status, true if stepper has been calibrated with a move to it's limit switch.
		/// </summary>
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

		/// <summary>
		/// Set the current stepper position as the 'zero' position.
		/// </summary>
		public void Zero()
		{
			Debug.Assert(Prefix == 'p', "--> Zero only valid for Pan");
			Command("z1");
		}

		/// <summary>
		/// Calibrate the stepper with a move to it's Home limit switch.
		/// </summary>
		public void Home()
		{
			Debug.Assert(Prefix == 's', "--> Home only valid for Slider");
			Command("h0");
			TargetPosition = 0;
		}

		/// <summary>
		/// Calculate the speed required to move a specified distance over a specified time
		/// with the acceleration already specified.
		/// </summary>
		/// <param name="distance">The signed distance to move, in logical units.</param>
		/// <param name="seconds">The duration, in seconds, desired for the move.</param>
		/// <returns>The target cruising speed for the move, for use with SetMaxSpeed.</returns>
		/// <remarks>
		/// Initial and final velocities are assumed equal to zero and acceleration and deceleration
		/// values are assumed equal (using the value from SetAcceleration).
		/// </remarks>
		public double MaxSpeedForDistanceAndTime(double distance, double seconds)
		{
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

		/// <summary>
		/// Given the current state of the stepper, calculate the time required to move to the
		/// specified distance, arriving with the desired speed.
		/// </summary>
		/// <param name="distance">The signed distance to move.</param>
		/// <param name="speed2">The desired arrival speed, defaulting to 0.</param>
		/// <returns>The number of seconds required to move.</returns>
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

		/// <summary>
		/// Process the setting of a property including setting the backing store,
		/// notifying INotifyPropertyChanged clients and performing
		/// other arbitrary actions when a change is detected.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="backingStore">A reference to the property's backing store.</param>
		/// <param name="value">The value to be assigned.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="onChanged">An Action to be invoked when a change is detected.</param>
		/// <returns>True if the value changed.</returns>
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

		/// <summary>
		/// Limit a Position value to within the valid range for the stepper.
		/// </summary>
		/// <param name="v">The Position value.</param>
		/// <returns>The Position value after limiting to the valid range.</returns>
		public int LimitValue(int v)
		{
			return Math.Max(Math.Min(v, LimitMax), LimitMin);
		}

		#region INotifyPropertyChanged
		/// <summary>
		/// Fired when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fire an event for a property changing.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	/// <summary>
	/// Represents the Camera and its intervalometer functions.
	/// </summary>
	public class Camera : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		/// <summary>Camera properties exposed to the Command interface.</summary>
		/// <remarks>The enum values represent the character codes used in the Command strings.</remarks>
		enum CamProperties
		{
			Cam_FocusDelay = 'd',
			Cam_ShutterHold = 's',
			Cam_Interval = 'i',
			Cam_Frames = 'f'
		};

		protected Camera()
		{
		}

		private static Camera _Cam;
		/// <summary>
		/// Get the Camera instance.
		/// </summary>
		internal static Camera Cam
		{
			get
			{
				if (_Cam == null)
					_Cam = new Camera();
				return _Cam;
			}
		}

		/// <summary>
		/// Process an input string from the Bluetooth device.
		/// </summary>
		/// <param name="s">The input string.</param>
		/// <remarks>
		/// The first character of the string indicates a property to be accessed or an action.
		/// The remainder of the string will be a value for the property.
		/// </remarks>
		internal void Input(string s)
		{
			// parse the value from the remainder of the string
			if (!uint.TryParse(s.Substring(1), out uint v))
				return;

			var prop = (CamProperties)s[0];

		//	Debug.WriteLine($"++> Camera {prop} <- {v}");
			// keeping track of properties we've requested values for, to avoid redundant requests
			if (requestedProps.Contains(prop))
				requestedProps.Remove(prop);

			// Invoke SetProperty directly without the onChanged action
			// so we don't needlessly reflect the same value back to the device.
			switch (prop)
			{
				case CamProperties.Cam_FocusDelay:
					SetProperty(ref _FocusDelay, v, "FocusDelay");
					break;
				case CamProperties.Cam_ShutterHold:
					SetProperty(ref _ShutterHold, v, "ShutterHold");
					break;
				case CamProperties.Cam_Interval:
					SetProperty(ref _Interval, v, "Interval");
					break;
				case CamProperties.Cam_Frames:
					SetProperty(ref _Frames, v, "Frames");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Send a command string to the device.
		/// </summary>
		/// <param name="cmd">The command string.</param>
		/// <param name="required">False for non-essential data that can be skipped.</param>
		void Command(string cmd, bool required = true)
		{
			Comm.Command($"c{cmd}", required);
		}

		/// <summary>
		/// Send a property value to the device.
		/// </summary>
		/// <param name="prop">The property to be set.</param>
		/// <param name="v">The value to be set.</param>
		void SetDeviceProp(CamProperties prop, uint v)
		{
			Command($"{(char)prop}{v}");
		}

		/// <summary>
		/// A list to track requested property values to avoid redundant requests
		/// that would burden the Bluetooth bandwidth.
		/// </summary>
		List<CamProperties> requestedProps = new List<CamProperties>();

		/// <summary>
		/// Request a property value from the device.
		/// </summary>
		/// <param name="prop">The property to request.</param>
		void RequestDeviceProp(CamProperties prop)
		{
			if (requestedProps.Contains(prop))
				return;
			requestedProps.Add(prop);
			// the '?' following the property code requests the value rather than set it
			Command($"{(char)prop}?");
		}

		/// <summary>
		/// Request all properties from the device.
		/// </summary>
		public void RequestAllDeviceProps()
		{
			requestedProps.Clear();     // force requests for all
			foreach (var prop in (CamProperties[])Enum.GetValues(typeof(CamProperties)))
			{
				RequestDeviceProp(prop);
			}
		}

		protected uint? _FocusDelay;
		/// <summary>
		/// Get/set the delay required between triggering camera focus and tripping the shutter.
		/// </summary>
		public uint FocusDelay
		{
			get
			{
				if (!_FocusDelay.HasValue)
				{
					RequestDeviceProp(CamProperties.Cam_FocusDelay);
					return 0;
				}
				return _FocusDelay.Value;
			}
			set
			{
				SetProperty(ref _FocusDelay, value, onChanged: () => SetDeviceProp(CamProperties.Cam_FocusDelay, _FocusDelay.Value));
			}
		}

		protected uint? _ShutterHold;
		/// <summary>
		/// Get/set the hold time required for the signal tripping the shutter.
		/// </summary>
		public uint ShutterHold
		{
			get
			{
				if (!_ShutterHold.HasValue)
				{
					RequestDeviceProp(CamProperties.Cam_ShutterHold);
					return 0;
				}
				return _ShutterHold.Value;
			}
			set
			{
				SetProperty(ref _ShutterHold, value, onChanged: () => SetDeviceProp(CamProperties.Cam_ShutterHold, _ShutterHold.Value));
			}
		}

		protected uint? _Interval;
		/// <summary>
		/// Get/set the interval time between frames, in milliseconds.
		/// </summary>
		public uint Interval
		{
			get
			{
				if (!_Interval.HasValue)
				{
					RequestDeviceProp(CamProperties.Cam_Interval);
					return 0;
				}
				return _Interval.Value;
			}
			set
			{
				SetProperty(ref _Interval, value, onChanged: () => SetDeviceProp(CamProperties.Cam_Interval, _Interval.Value));
			}
		}

		protected uint? _Frames;
		/// <summary>
		/// Get/set the number of frames to be captured for the intervalometer operation.
		/// </summary>
		/// <remarks>Setting Frames to a non-zero value will start the intrvalometer operation on the device.</remarks>
		public uint Frames
		{
			get
			{
				if (!_Frames.HasValue)
				{
					RequestDeviceProp(CamProperties.Cam_Frames);
					return 0;
				}
				return _Frames.Value;
			}
			set
			{
				SetProperty(ref _Frames, value, onChanged: () => SetDeviceProp(CamProperties.Cam_Frames, _Frames.Value));
			}
		}

		/// <summary>
		/// Process the setting of a property including setting the backing store,
		/// notifying INotifyPropertyChanged clients and performing
		/// other arbitrary actions when a change is detected.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="backingStore">A reference to the property's backing store.</param>
		/// <param name="value">The value to be assigned.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="onChanged">An Action to be invoked when a change is detected.</param>
		/// <returns>True if the value changed.</returns>
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
		/// <summary>
		/// Fired when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fire an event for a property changing.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
