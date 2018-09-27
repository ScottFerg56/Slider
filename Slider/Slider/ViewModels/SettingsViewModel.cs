//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CamSlider.ViewModels
{
	public class SettingsViewModel : INotifyPropertyChanged
	{
		public SettingsViewModel()
		{
			// propagate Settings changes on through
			SliderComm.Instance.Settings.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, e); };
		}

		/// <summary>
		/// Get a property value from the Settings object.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="propertyName">The name of the property to access.</param>
		/// <returns>The value of the property.</returns>
		/// <remarks>Defers property access to the Settings.</remarks>
		protected T GetProperty<T>([CallerMemberName]string propertyName = "")
		{
			return (T)SliderComm.Instance.Settings.GetType().GetProperty(propertyName).GetValue(SliderComm.Instance.Settings);
		}

		/// <summary>
		/// Get/set the motor location relative to the slider track.
		/// False means motor/stepper is on the left.
		/// </summary>
		public bool MotorLocation
		{
			get => GetProperty<bool>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the speed value to be used for Slide movements.
		/// </summary>
		public uint SlideMoveSpeed
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the acceleration value to be used for Slide movements.
		/// </summary>
		public uint SlideAcceleration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the speed value to be used for Pan movements.
		/// </summary>
		public uint PanMoveSpeed
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the acceleration value to be used for Pan movements.
		/// </summary>
		public uint PanAcceleration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the time, in milliseconds, for the focus operation to be active
		/// before the shutter is triggered.
		/// </summary>
		public uint FocusDelay
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the time, in milliseconds, for the shutter trigger to be held.
		/// </summary>
		public uint ShutterHold
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Set a property value on the Settings object.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="value">The value to be set.</param>
		/// <param name="propertyName">The name of the property to access.</param>
		/// <param name="onChanged">An Action to be invoked when a change is detected.</param>
		/// <returns>True if the value changed.</returns>
		protected bool SetProperty<T>(T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			var pi = SliderComm.Instance.Settings.GetType().GetProperty(propertyName);
			if (EqualityComparer<T>.Default.Equals((T)pi.GetValue(SliderComm.Instance.Settings), value))
				return false;

			pi.SetValue(SliderComm.Instance.Settings, value);
			onChanged?.Invoke();
			// we've setup to propagate changes from the Settings object back up thru us
			// so no need to do that here
		//	OnPropertyChanged(propertyName);
			// save the Settings with every change
			SaveSettings();
			return true;
		}

		/// <summary>
		/// Save the Settings to the data store.
		/// </summary>
		protected void SaveSettings()
		{
			Services.DataStore.SaveDataStore("settings", SliderComm.Instance.Settings);
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
