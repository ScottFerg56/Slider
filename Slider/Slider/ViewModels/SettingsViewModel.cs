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

		public int SlideMoveSpeed
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int SlideAcceleration
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int PanMoveSpeed
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int PanAcceleration
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int FocusDelay
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int ShutterHold
		{
			get => GetProperty<int>();
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
