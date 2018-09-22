using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using Xamarin.Forms;

namespace CamSlider
{
	/// <summary>
	/// State of the Bluetooth connection.
	/// </summary>
	public enum BlueState
	{
		Disconnected = 0,
		Connecting = 1,
		Connected = 2,
		Disconnecting = 3,
		Searching = 4,
		Found = 5,
		NotFound = 6,
	}

	/// <summary>
	/// Platform-independent interface for serial Bluetooth communications platform implementations.
	/// </summary>
	public interface IBlueDevice
	{
		void Connect(string name);
		void Disconnect();
		BlueState State { get; }
		bool CanConnect { get; }
		string ErrorMessage { get; }
		bool ByteAvailable { get; }
		byte GetByte();
		bool Write(params byte[] data);
		bool Write(string data);
		event EventHandler StateChange;
		event EventHandler InputAvailable;
	}

	/// <summary>
	/// Serial Bluetooth communications wrapper over the platform implementations.
	/// </summary>
	public class BlueApp
	{
		/// <summary>
		/// The platform-specific Bluetooth implementation.
		/// </summary>
		IBlueDevice BlueDevice;
		/*
		 * The Android Bluetooth implementation for output to the device has trouble keeping up with the
		 * rate of usage coming from the Slider control on the ManualPage. This causes writes to fail,
		 * presumably because the output queue is overflowing. Thus this layer introduces the concept
		 * of writes that are not essential/required (such as the flood of values from the Slider control).
		 * Required writes are queued in order until they succeed.
		 * Non-required writes are ignored if there are queued entries or if the write fails.
		 * A timer is used to periodically retry queued writes.
		 * This approach is emperically satisfying and responsive enough.
		 */
		List<string> Queue = new List<string>();
		Timer timer;
		static TraceSwitch sw = new TraceSwitch("BlueApp", "BlueApp") { Level = TraceLevel.Warning };

		/// <summary>
		/// Fired when the Bluetooth connection State changes.
		/// </summary>
		public event EventHandler StateChange;

		/// <summary>
		/// Fired when input is available for reading from the Bluetooth connection.
		/// </summary>
		public event EventHandler InputAvailable;

		public BlueApp()
		{
			// get the platform-dependent implementation from the DependencyService
			BlueDevice = DependencyService.Get<IBlueDevice>();
			BlueDevice.StateChange += BlueDevice_StateChange;
			BlueDevice.InputAvailable += BlueDevice_InputAvailable;
			timer = new Timer
			{
				Enabled = false,
				Interval = 100
			};
			timer.Elapsed += Timer_Elapsed;
		}

		/// <summary>
		/// Propagate the InputAvailable event.
		/// </summary>
		private void BlueDevice_InputAvailable(object sender, EventArgs e)
		{
			InputAvailable?.Invoke(this, e);
		}

		/// <summary>
		/// Connect to the remote device by name.
		/// </summary>
		/// <param name="name">The name of the device to connect to.</param>
		public void Connect(string name) => BlueDevice.Connect(name);

		/// <summary>
		/// Disconnect from the device.
		/// </summary>
		public void Disconnect() => BlueDevice.Disconnect();

		/// <summary>
		/// Gets the connection state.
		/// </summary>
		public BlueState State { get => BlueDevice.State; }

		/// <summary>
		/// True if the device is in a state where connection is possible.
		/// </summary>
		public bool CanConnect { get => BlueDevice.CanConnect; }

		/// <summary>
		/// Gets an error message associated with the last error.
		/// </summary>
		public string ErrorMessage { get => BlueDevice.ErrorMessage; }

		/// <summary>
		/// True if there is a byte available to be read.
		/// </summary>
		public bool ByteAvailable { get => BlueDevice.ByteAvailable; }

		/// <summary>
		/// Get the next byte from the Bluetooth device.
		/// </summary>
		/// <returns>The next byte that has already been received.</returns>
		/// <remarks>There must be a ByteAvailable for this to succeed.</remarks>
		public byte GetByte() => BlueDevice.GetByte();

		/// <summary>
		/// Handle state change from the Bluetooth device.
		/// </summary>
		private void BlueDevice_StateChange(object sender, EventArgs e)
		{
			lock (Queue)
			{
				if (Queue.Count > 0)
				{
					// clear the write queue
					Debug.WriteLineIf(sw.TraceVerbose, "++> Queue emptied on state change");
					Queue.Clear();
				}
			}
			// propagate the change to our clients
			StateChange?.Invoke(this, e);
		}

		/// <summary>
		/// Write a string to the Bluetooth device.
		/// </summary>
		/// <param name="data">The string to write.</param>
		/// <param name="required">False for non-essential data that can be skipped.</param>
		public void Write(string data, bool required = true)
		{
			if (State != BlueState.Connected)
			{
				// skip if not connected
				Debug.WriteLineIf(sw.TraceError, $"--> Write ignored while not connected: {data}");
				return;
			}
			lock (Queue)
			{
				if (Queue.Count > 0)
				{
					// other items in the queue
					if (!required)
					{
						// skip non-essential data
						Debug.WriteLineIf(sw.TraceVerbose, $"++> Optional write ignored: {data}");
						return;
					}
					// queue an essential write and make sure the timer is enabled
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Required write queued: {data}");
					Queue.Add(data);
					timer.Enabled = true;
					return;
				}
				// the queue is empty - try the write
				if (BlueDevice.Write(data))
				{
					// success
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Write succeeded: {data}");
				}
				else if (required)
				{
					// failure writing essential data - add to the queue
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Required write failed/queued: {data}");
					Queue.Add(data);
					timer.Enabled = true;
				}
				else
				{
					// failure writing non-essential data - skip it
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Optional write failed/not queued: {data}");
				}
			}
		}

		/// <summary>
		/// Periodically manage the write queue.
		/// </summary>
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (Queue)
			{
				timer.Enabled = false;
				// check if there's anything to do
				if (Queue.Count == 0)
					return;
				// if not connected just clear the queue
				if (State != BlueState.Connected)
				{
					Queue.Clear();
					return;
				}
				// try writing the first essential data item
				string data = Queue.First();
				if (BlueDevice.Write(data))
				{
					// success - remove it from the queue
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Queued write succeeded: {data}");
					Queue.RemoveAt(0);
				}
				else
				{
					// failure -- leave it to try again next time
					Debug.WriteLineIf(sw.TraceVerbose, $"++> Queued write failed: {data}");
				}
				if (Queue.Count > 0)
				{
					// still entries to process - leave the timer enabled
					Debug.WriteLineIf(sw.TraceVerbose, "++> Queue not emptied");
					timer.Enabled = true;
				}
			}
		}
	}
}
