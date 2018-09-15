using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CamSlider.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
	{
		public SettingsViewModel()
		{
			SliderComm.Instance.Settings.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, e); };
		}

		protected T GetProperty<T>([CallerMemberName]string propertyName = "")
		{
			return (T)SliderComm.Instance.Settings.GetType().GetProperty(propertyName).GetValue(SliderComm.Instance.Settings);
		}

		public bool MotorLocation
		{
			get => GetProperty<bool>();
			set => SetProperty(value);
		}

		public uint SlideMoveSpeed
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint SlideAcceleration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint PanMoveSpeed
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint PanAcceleration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint FocusDelay
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint ShutterHold
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		protected bool SetProperty<T>(T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			var pi = SliderComm.Instance.Settings.GetType().GetProperty(propertyName);
			if (EqualityComparer<T>.Default.Equals((T)pi.GetValue(SliderComm.Instance.Settings), value))
				return false;

			pi.SetValue(SliderComm.Instance.Settings, value);
			onChanged?.Invoke();
			// the Settings object will propagate changes back up thru us
			//	OnPropertyChanged(propertyName);
			SaveSequence();
			return true;
		}

		protected void SaveSequence()
		{
			Services.DataStore.SaveDataStore("settings", SliderComm.Instance.Settings);
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
