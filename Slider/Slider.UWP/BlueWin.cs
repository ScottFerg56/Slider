using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

[assembly: Xamarin.Forms.Dependency(typeof(CamSlider.UWP.BlueWin))]
namespace CamSlider.UWP
{
	public class BlueWin : IBlueDevice
	{
		public BlueState _State = BlueState.Disconnected;
		public BlueState State
		{
			get { return _State; }
			protected set
			{
				if (_State == value)
					return;
				_State = value;
				Debug.WriteLine($"++> State change: {State}");
				StateChange(this, EventArgs.Empty);
			}
		}

		private string TargetDeviceName;
		public event EventHandler StateChange = delegate { };
		public event EventHandler InputAvailable = delegate { };

		private DeviceWatcher deviceWatcher;
		private DeviceInformation DeviceInfo;
		private BluetoothLEDevice bluetoothLeDevice;
		private GattDeviceService Service;
		private GattCharacteristic _TX;
		private GattCharacteristic _RX;
		private readonly Guid uuidService = Guid.Parse("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
		private readonly Guid uuidTX = Guid.Parse("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
		private readonly Guid uuidRX = Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		private readonly Guid uuidCharacteristicConfig = Guid.Parse("00002902-0000-1000-8000-00805f9b34fb");

		protected List<byte[]> BytesRead = new List<byte[]>();
		protected int BytesIndex = 0;

		public BlueWin()
		{
		}

		public bool CanConnect
		{
			get
			{
				switch (State)
				{
					case BlueState.Disconnected:
					case BlueState.Disconnecting:
					case BlueState.NotFound:
						return true;
					case BlueState.Connected:
					case BlueState.Connecting:
					case BlueState.Searching:
					case BlueState.Found:
						return false;
					default:
						Debug.WriteLine("--> Unknown State value");
						return false;
				}
			}
		}

		public string ErrorMessage { get; protected set; }

		private void StartBleDeviceWatcher()
		{
			if (deviceWatcher == null)
			{
				// Additional properties we would like about the device.
				// Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
				string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

				// BT_Code: Example showing paired and non-paired in a single query.
				string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

				deviceWatcher =
						DeviceInformation.CreateWatcher(
							aqsAllBluetoothLEDevices,
							requestedProperties,
							DeviceInformationKind.AssociationEndpoint);

				// Register event handlers before starting the watcher.
				deviceWatcher.Added += DeviceWatcher_Added;
				deviceWatcher.Updated += DeviceWatcher_Updated;
				deviceWatcher.Removed += DeviceWatcher_Removed;
				deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
				deviceWatcher.Stopped += DeviceWatcher_EnumerationCompleted;
			}
			State = BlueState.Searching;
			// Start the watcher.
			deviceWatcher.Start();
		}

		private void StopBleDeviceWatcher()
		{
			if (deviceWatcher != null)
			{
				if (deviceWatcher.Status == DeviceWatcherStatus.Started)
				{
					// Stop the watcher.
					deviceWatcher.Stop();
				}
				deviceWatcher = null;
			}
		}

		private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
		{
			// Protect against race condition if the task runs after the app stopped the deviceWatcher.
			if (sender == deviceWatcher)
			{
				Debug.WriteLine("Device found: {0}", deviceInfo.Name);
				// Make sure device name isn't blank or already present in the list.
				if (DeviceInfo == null && deviceInfo.Name == TargetDeviceName)
				{
					DeviceInfo = deviceInfo;
					StopBleDeviceWatcher();
					State = BlueState.Found;
					await SetupDevice();
				}
			}
		}

		async Task SetupDevice()
		{
			State = BlueState.Connecting;

			try
			{
			//	Debug.WriteLine("++> Getting Bluetooth LE device");
				// BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
				bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(DeviceInfo.Id);
			}
			catch (Exception ex) when ((uint)ex.HResult == 0x800710df)
			{
				ErrorMessage = "ERROR_DEVICE_NOT_AVAILABLE because the Bluetooth radio is not on.";
			}
			catch (Exception ex)
			{
				ErrorMessage = "FromIdAsync ERROR: " + ex.Message;
			}

			if (bluetoothLeDevice == null)
			{
				Disconnect();
				return;
			}

			var sr = await bluetoothLeDevice.GetGattServicesForUuidAsync(uuidService, BluetoothCacheMode.Uncached);
			if (sr.Status != GattCommunicationStatus.Success || !sr.Services.Any())
			{
				ErrorMessage = "Can't find service: " + sr.Status.ToString();
				Disconnect();
				return;
			}
			Service = sr.Services.First();
			// Debug.WriteLine("++> GattService found");

			var accessStatus = await Service.RequestAccessAsync();
			if (accessStatus != DeviceAccessStatus.Allowed)
			{
				// Not granted access
				ErrorMessage = "Error accessing service:" + accessStatus.ToString();
				Disconnect();
				return;
			}
			var charResult = await Service.GetCharacteristicsForUuidAsync(uuidTX, BluetoothCacheMode.Uncached);
			if (charResult.Status != GattCommunicationStatus.Success || !charResult.Characteristics.Any())
			{
				ErrorMessage = "Error getting TX characteristic: " + charResult.Status.ToString();
				Disconnect();
				return;
			}
			_TX = charResult.Characteristics.First();
			charResult = await Service.GetCharacteristicsForUuidAsync(uuidRX, BluetoothCacheMode.Uncached);
			if (charResult.Status != GattCommunicationStatus.Success || !charResult.Characteristics.Any())
			{
				ErrorMessage = "Error getting RX characteristic: " + charResult.Status.ToString();
				Disconnect();
				return;
			}
			_RX = charResult.Characteristics.First();
			try
			{
			//	Debug.WriteLine("++> Setting Notify descriptor");
				var res = await _RX.WriteClientCharacteristicConfigurationDescriptorAsync(
										GattClientCharacteristicConfigurationDescriptorValue.Notify);
				if (res != GattCommunicationStatus.Success)
				{
					ErrorMessage = "Error setting RX notify: " + charResult.Status.ToString();
					Disconnect();
					return;
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Exception setting RX notify: " + ex.Message;
				Disconnect();
				return;
			}
			_RX.ValueChanged += Receive_ValueChanged;
			State = BlueState.Connected;
		}

		private void Receive_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
			BytesRead.Add(data);
			InputAvailable(this, EventArgs.Empty);
		}

		private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
		{
			// Protect against race condition if the task runs after the app stopped the deviceWatcher.
			if (sender == deviceWatcher)
			{
				if (DeviceInfo != null && DeviceInfo.Id == deviceInfoUpdate.Id)
					DeviceInfo.Update(deviceInfoUpdate);
			}
		}

		private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
		{
			// Protect against race condition if the task runs after the app stopped the deviceWatcher.
			if (sender == deviceWatcher)
			{
				if (DeviceInfo != null && DeviceInfo.Id == deviceInfoUpdate.Id)
					DeviceInfo = null;
			}
		}

		private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
		{
			// Protect against race condition if the task runs after the app stopped the deviceWatcher.
			if (sender == deviceWatcher)
			{
				if (DeviceInfo == null)
				{
					StopBleDeviceWatcher();
					State = BlueState.NotFound;
				}
				else
				{
					// should have already raised Found event
				}
			}
		}

		public void Connect(string name)
		{
			ErrorMessage = null;
			TargetDeviceName = name;
			StartBleDeviceWatcher();
		}

		public void Disconnect()
		{
			if (CanConnect)
				return;
			StopBleDeviceWatcher();
			State = BlueState.Disconnecting;
			Cleanup();
		}

		void Cleanup()
		{
			DeviceInfo = null;
			_RX = null;
			_TX = null;
			if (Service != null)
			{
				Service.Dispose();
				Service = null;
			}
			if (bluetoothLeDevice != null)
			{
				bluetoothLeDevice.Dispose();
				bluetoothLeDevice = null;
			}
			State = BlueState.Disconnected;
		}

		public bool ByteAvailable => BytesRead.Count != 0;

		public byte GetByte()
		{
			byte b = BytesRead[0][BytesIndex++];
			if (BytesIndex >= BytesRead[0].Length)
			{
				BytesRead.RemoveAt(0);
				BytesIndex = 0;
			}
			return b;
		}

		public bool Write(params byte[] data)
		{
			if (_TX == null)
			{
				Debug.WriteLine($"--> Write no TX characteristic");
				return false;
			}
			var writeBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(data);
			try
			{
				// BT_Code: Writes the value from the buffer to the characteristic.
				var result = _TX.WriteValueAsync(writeBuffer);
				result.AsTask().Wait();

				if (result.GetResults() == GattCommunicationStatus.Success)
				{
					//    Debug.WriteLine("Successfully wrote value to device");
				}
				else
				{
					Debug.WriteLine($"--> Write failed: {result.GetResults()}");
					return false;
				}
			}
			catch (Exception ex) when ((uint)ex.HResult == 0x80650003 || (uint)ex.HResult == 0x80070005)
			{
				// E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED or E_ACCESSDENIED
				// This usually happens when a device reports that it support writing, but it actually doesn't.
				Debug.WriteLine($"--> Write failure: {ex.Message}");
				return false;
			}
			return true;
		}

		public bool Write(string data)
		{
			return Write(Encoding.UTF8.GetBytes(data));
		}
	}
}
