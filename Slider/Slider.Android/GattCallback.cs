//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using System;
using System.Diagnostics;
using Android.Bluetooth;
using Android.Runtime;

namespace CamSlider.Droid
{
	public class ConnectionStateChangeEventArgs : EventArgs
	{
		public BluetoothGatt Gatt;
		public GattStatus Status;
		public ProfileState NewState;

		public ConnectionStateChangeEventArgs() : base()
		{ }
	}

	/// <summary>
	/// A simple class to catch Gatt Callback calls and provide as events for clients.
	/// </summary>
	public class GattCallback : BluetoothGattCallback
	{
		public event EventHandler<ConnectionStateChangeEventArgs> ConnectionStateChange = delegate { };
		public event EventHandler ServicesDiscovered = delegate { };
		public event EventHandler<CharacteristicReadWriteEventArgs> CharacteristicValueUpdated = delegate { };
		public event EventHandler<CharacteristicReadWriteEventArgs> CharacteristicWriteStatus = delegate { };

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
		{
			base.OnConnectionStateChange(gatt, status, newState);
		//	Debug.WriteLine("++> OnConnectionStateChange: ");
			ConnectionStateChange(this, new ConnectionStateChangeEventArgs() { Gatt = gatt, Status = status, NewState = newState });
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
		{
			base.OnServicesDiscovered(gatt, status);
		//	Debug.WriteLine($"++> OnServicesDiscovered: {status}");
			ServicesDiscovered(this, EventArgs.Empty);
		}

		public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
		{
			base.OnDescriptorRead(gatt, descriptor, status);
		//	Debug.WriteLine("++> OnDescriptorRead: " + descriptor.ToString());
		}

		public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
		{
			base.OnCharacteristicRead(gatt, characteristic, status);
		//	Debug.WriteLine("++> OnCharacteristicRead: " + characteristic.GetStringValue(0));
			this.CharacteristicValueUpdated(this, new CharacteristicReadWriteEventArgs() { Characteristic = characteristic, Status = status });
		}

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
		{
			base.OnCharacteristicChanged(gatt, characteristic);
		//	Debug.WriteLine("++> OnCharacteristicChanged: " + characteristic.GetStringValue(0));
			this.CharacteristicValueUpdated(this, new CharacteristicReadWriteEventArgs() { Characteristic = characteristic });
		}

		public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
		{
			base.OnCharacteristicWrite(gatt, characteristic, status);
		//	Debug.WriteLine("++> OnCharacteristicWrite: " + status.ToString());
			this.CharacteristicWriteStatus(this, new CharacteristicReadWriteEventArgs() { Characteristic = characteristic, Status = status });
		}
	}

	public class CharacteristicReadWriteEventArgs : EventArgs
	{
		public BluetoothGattCharacteristic Characteristic { get; set; }
		public GattStatus Status;

		public CharacteristicReadWriteEventArgs()
		{
		}
	}
}
