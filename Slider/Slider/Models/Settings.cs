//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CamSlider.Models
{
	/// <summary>
	/// Settings related to the camera slider hardware and its performance characteristics.
	/// </summary>
	public class Settings : INotifyPropertyChanged
	{
		private bool _MotorLocation = true;
		/// <summary>
		/// Get/set the motor location relative to the slider track.
		/// False means motor/stepper is on the left.
		/// </summary>
		public bool MotorLocation
		{
			get => _MotorLocation;
			set => SetProperty(ref _MotorLocation, value);
		}

		private uint _SlideAcceleration = 25;
		/// <summary>
		/// Get/set the acceleration value to be used for Slide movements.
		/// </summary>
		public uint SlideAcceleration
		{
			get => _SlideAcceleration;
			set => SetProperty(ref _SlideAcceleration, value, (v) => Math.Min(50, v));
		}

		private uint _SlideMoveSpeed = 30;
		/// <summary>
		/// Get/set the speed value to be used for Slide movements.
		/// </summary>
		public uint SlideMoveSpeed
		{
			get => _SlideMoveSpeed;
			set => SetProperty(ref _SlideMoveSpeed, value, (v) => (uint)Math.Round(Math.Min(SliderComm.Instance.Slide.SpeedLimit, (double)v)));
		}

		private uint _PanAcceleration = 45;
		/// <summary>
		/// Get/set the acceleration value to be used for Pan movements.
		/// </summary>
		public uint PanAcceleration
		{
			get => _PanAcceleration;
			set => SetProperty(ref _PanAcceleration, value, (v) => Math.Min(90, v));
		}

		private uint _PanMoveSpeed = 55;
		/// <summary>
		/// Get/set the speed value to be used for Pan movements.
		/// </summary>
		public uint PanMoveSpeed
		{
			get => _PanMoveSpeed;
			set => SetProperty(ref _PanMoveSpeed, value, (v) => (uint)Math.Round(Math.Min(SliderComm.Instance.Pan.SpeedLimit, v)));
		}

		private uint _FocusDelay = 150;
		/// <summary>
		/// Get/set the time, in milliseconds, for the focus operation to be active
		/// before the shutter is triggered.
		/// </summary>
		public uint FocusDelay
		{
			get => _FocusDelay;
			set => SetProperty(ref _FocusDelay, value, (v) => Math.Max(0, v));
		}

		private uint _ShutterHold = 50;
		/// <summary>
		/// Get/set the time, in milliseconds, for the shutter trigger to be held.
		/// </summary>
		public uint ShutterHold
		{
			get => _ShutterHold;
			set => SetProperty(ref _ShutterHold, value, (v) => Math.Max(0, v));
		}

		/// <summary>
		/// Process the setting of a property including setting the backing store,
		/// filtering values, notifying INotifyPropertyChanged clients and performing
		/// other arbitrary actions when a change is detected.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="backingStore">A reference to the property's backing store.</param>
		/// <param name="value">The value to be assigned.</param>
		/// <param name="filter">An optional filter to process the value before assignment.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="onChanged">An Action to be invoked when a change is detected.</param>
		/// <returns>True if the value changed.</returns>
		protected bool SetProperty<T>(ref T backingStore, T value, Func<T, T> filter = null,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			// NOTE: For the binding interactions with XAML controls
			// we need to test for equality before filtering
			// so that property change notifications are sent
			// even if not ultimately changed
			// because it's still different from what the UI has

			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = (filter != null) ? filter(value) : value;
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
