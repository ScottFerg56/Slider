using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Bluetooth;
using System.Threading.Tasks;
using Android.Bluetooth.LE;
using System.Diagnostics;

[assembly: Xamarin.Forms.Dependency(typeof(CamSlider.Droid.BlueAndroid))]
namespace CamSlider.Droid
{
	/*
		Need to add the following permissions to the .Android\Properties\AndroidManifest.xml:
			<uses-permission android:name="android.permission.BLUETOOTH" />
			<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
			<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />

		And add a permission check in the OnCreate method in .Android\MainActivity.cs:
			if (this.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
			{
				RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation }, 0);
			}
	 */

	public class BlueAndroid : Android.Bluetooth.LE.ScanCallback, CamSlider.IBlueDevice
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
			//	Debug.WriteLine($"++> State change: {State}");
				StateChange(this, EventArgs.Empty);
			}
		}

		private string TargetDeviceName;
		public event EventHandler StateChange = delegate { };
		public event EventHandler InputAvailable = delegate { };

		protected BluetoothManager _manager;
		protected BluetoothAdapter _adapter;
		protected GattCallback _gattCallback;
		protected BluetoothDevice _device;
		protected BluetoothGatt _gatt;
		protected BluetoothGattService _service;
		protected BluetoothGattCharacteristic _TX;
		protected BluetoothGattCharacteristic _RX;

		protected Java.Util.UUID uuidService = Java.Util.UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
		protected Java.Util.UUID uuidTX = Java.Util.UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
		protected Java.Util.UUID uuidRX = Java.Util.UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		protected Java.Util.UUID uuidCharacteristicConfig = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		protected List<byte[]> BytesRead = new List<byte[]>();
		protected int BytesIndex = 0;

		public BlueAndroid()
		{
			var appContext = Android.App.Application.Context;
			if (appContext == null)
				return;
			// get a reference to the bluetooth system service
			_manager = (BluetoothManager)appContext.GetSystemService("bluetooth");
			if (_manager == null)
				return;
			_adapter = _manager.Adapter;
			_gattCallback = new GattCallback();
			if (_gattCallback == null)
				return;
			_gattCallback.ConnectionStateChange += GattCallback_ConnectionStateChange;
			_gattCallback.ServicesDiscovered += GattCallback_ServicesDiscovered;
			_gattCallback.CharacteristicValueUpdated += GattCallback_CharacteristicValueUpdated;
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
					default:
						return false;
				}
			}
		}

		public string ErrorMessage { get; protected set; }

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
				return false;
			bool res = _TX.SetValue(data);
			if (!res)
			{
			//	Debug.WriteLine("--> TX SetValue failed");
				return false;
			}
			else
			{
				res = _gatt.WriteCharacteristic(_TX);
				if (!res)
				{
				//	Debug.WriteLine("--> TX WriteCharacteristic failed");
					return false;
				}
			}
			return true;
		}

		public bool Write(string data)
		{
			return Write(Encoding.UTF8.GetBytes(data));
		}

		private void GattCallback_CharacteristicValueUpdated(object sender, CharacteristicReadWriteEventArgs e)
		{
			if (e.Characteristic == _RX)
			{
				var bytes = e.Characteristic.GetValue();
				BytesRead.Add(bytes);
				InputAvailable(this, EventArgs.Empty);
			}
		}

		private void GattCallback_ServicesDiscovered(object sender, EventArgs e)
		{
		//	Debug.WriteLine("++> GattCallback_ServicesDiscovered");
			_service = _gatt.GetService(uuidService);
			_TX = _service.GetCharacteristic(uuidTX);
			_RX = _service.GetCharacteristic(uuidRX);
			BluetoothGattDescriptor config = _RX.GetDescriptor(uuidCharacteristicConfig);
			if (config == null)
			{
				Debug.WriteLine("--> _RX.GetDescriptor failed");
				return;
			}
			bool b = _gatt.SetCharacteristicNotification(_RX, true);
		//	b = _gatt.SetCharacteristicNotification(_RX, true);
			// enableNotification/disable remotely
			b = config.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
			b = _gatt.WriteDescriptor(config);
		}

		private void GattCallback_ConnectionStateChange(object sender, ConnectionStateChangeEventArgs e)
		{
		//	Debug.WriteLine($"++> GattCallback_ConnectionStateChange: {e.NewState}");
			switch (e.NewState)
			{
				case ProfileState.Connected:
					_gatt = e.Gatt;
					_service = _gatt.GetService(uuidService);
					_gatt.DiscoverServices();
					break;
				case ProfileState.Connecting:
					break;
				case ProfileState.Disconnected:
					break;
				case ProfileState.Disconnecting:
					break;
				default:
					break;
			}
			State = (BlueState)e.NewState;
		}

		public async void Connect(string name)
		{
			if (_adapter == null)   // no Bluetooth service (or emulator!)
				return;

			ErrorMessage = null;
			TargetDeviceName = name;

			// start scanning
			_device = null;
			_adapter.BluetoothLeScanner.StartScan(this);
			State = BlueState.Searching;

			// in 10 seconds, stop the scan
			await Task.Delay(20000);

			if (State == BlueState.Searching)
			{
				Disconnect();
				State = BlueState.NotFound;
			}
		}

		public void Disconnect()
		{
			if (State == BlueState.Searching)
			{
				_adapter.BluetoothLeScanner.StopScan(this);
			}
			if (_device != null)
			{
				_device = null;
				if (_gatt != null)
				{
					_gatt.Disconnect();
					_gatt = null;
					_service = null;
					_TX = null;
				}
			}
			State = BlueState.Disconnected;
		}

		public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
		{
			switch (callbackType)
			{
				case ScanCallbackType.AllMatches:
					break;
				case ScanCallbackType.FirstMatch:
					break;
				case ScanCallbackType.MatchLost:
					break;
				default:
					break;
			}
			DeviceResult(result);
		}

		public override void OnBatchScanResults(IList<ScanResult> results)
		{
		//	Debug.WriteLine("++> OnBatchScanResults");
			foreach (var result in results)
			{
				DeviceResult(result);
			}
		}

		public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
		{
			//	base.OnScanFailed(errorCode);
			Debug.WriteLine($"--> OnScanFailed: {errorCode}");
		}

		public void DeviceResult(ScanResult result)
		{
			if (result == null)
				return;
		//	Debug.WriteLine($"++> DeviceResult: {result.Device.Name}");

			if (_device == null && result.Device.Name == TargetDeviceName)
			{
				_device = result.Device;
				_adapter.BluetoothLeScanner.StopScan(this);
				State = BlueState.Found;
				_device.ConnectGatt(Android.App.Application.Context, true, _gattCallback);
			}
		}
	}
}