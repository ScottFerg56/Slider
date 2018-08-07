using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using Xamarin.Forms;

namespace CamSlider
{
	// First 4 entries must be the same as Android.Bluetooth.ProfileState
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

	public class BlueApp
	{
		IBlueDevice BlueDevice;
		List<string> Queue = new List<string>();
		Timer timer;

		public event EventHandler StateChange;
		public event EventHandler InputAvailable;

		public BlueApp()
		{
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

		private void BlueDevice_InputAvailable(object sender, EventArgs e)
		{
			InputAvailable?.Invoke(this, e);
		}

		public void Connect(string name) => BlueDevice.Connect(name);
		public void Disconnect() => BlueDevice.Disconnect();
		public BlueState State { get => BlueDevice.State; }
		public bool CanConnect { get => BlueDevice.CanConnect; }
		public string ErrorMessage { get => BlueDevice.ErrorMessage; }
		public bool ByteAvailable { get => BlueDevice.ByteAvailable; }
		public byte GetByte() => BlueDevice.GetByte();

		private void BlueDevice_StateChange(object sender, EventArgs e)
		{
			if (Queue.Count > 0)
			{
				Debug.WriteLine("++> Queue emptied on state change");
				Queue.Clear();
			}
			StateChange?.Invoke(this, e);
		}

		public void Write(string data, bool required = true)
		{
			if (Queue.Count > 0)
			{
				if (!required)
				{
				//	Debug.WriteLine($"++> Optional write ignored: {data}");
					return;
				}
			//	Debug.WriteLine($"++> Required write queued: {data}");
				Queue.Add(data);
				timer.Enabled = true;
				return;
			}
			if (BlueDevice.Write(data))
			{
			//	Debug.WriteLine($"++> Write succeeded: {data}");
			}
			else if (required)
			{
			//	Debug.WriteLine($"++> Required write failed/queued: {data}");
				Queue.Add(data);
				timer.Enabled = true;
			}
			else
			{
			//	Debug.WriteLine($"++> Optional write failed/not queued: {data}");
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Enabled = false;
			if (Queue.Count == 0)
				return;
			string data = Queue.Last();
			if (BlueDevice.Write(data))
			{
			//	Debug.WriteLine($"++> Queued write succeeded: {data}");
				Queue.RemoveAt(Queue.Count - 1);
			}
			else
			{
			//	Debug.WriteLine($"++> Queued write failed: {data}");
			}
			if (Queue.Count > 0)
			{
			//	Debug.WriteLine("++> Queue not emptied");
				timer.Enabled = true;
			}
		}
	}
}
