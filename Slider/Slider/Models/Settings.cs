using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CamSlider.Models
{
    public class Settings : INotifyPropertyChanged
	{
		private bool _MotorLocation = false;	// false means motor/stepper is on the left
		public bool MotorLocation
		{
			get => _MotorLocation;
			set => SetProperty(ref _MotorLocation, value);
		}

		private int _SlideAcceleration = 25;
		public int SlideAcceleration
		{
			get => _SlideAcceleration;
			set => SetProperty(ref _SlideAcceleration, value, (v) => Math.Max(0, Math.Min(50, v)));
		}

		public int _SlideMoveSpeed = 30;
		public int SlideMoveSpeed
		{
			get => _SlideMoveSpeed;
			set => SetProperty(ref _SlideMoveSpeed, value, (v) => Math.Max(0, Math.Min(SlideSpeedLimit, v)));
		}

		public int _SlideSpeedLimit = 50;
		public int SlideSpeedLimit
		{
			get => _SlideSpeedLimit;
			set => SetProperty(ref _SlideSpeedLimit, value, (v) => Math.Max(0, Math.Min(50, v)));
		}

		private int _PanAcceleration = 45;
		public int PanAcceleration
		{
			get => _PanAcceleration;
			set => SetProperty(ref _PanAcceleration, value, (v) => Math.Max(0, Math.Min(90, v)));
		}

		public int _PanMoveSpeed = 55;
		public int PanMoveSpeed
		{
			get => _PanMoveSpeed;
			set => SetProperty(ref _PanMoveSpeed, value, (v) => Math.Max(0, Math.Min(PanSpeedLimit, v)));
		}

		public int _PanSpeedLimit = 90;
		public int PanSpeedLimit
		{
			get => _PanSpeedLimit;
			set => SetProperty(ref _PanSpeedLimit, value, (v) => Math.Max(0, Math.Min(90, v)));
		}

		public int _FocusDelay = 150;
		public int FocusDelay
		{
			get => _FocusDelay;
			set => SetProperty(ref _FocusDelay, value, (v) => Math.Max(0, v));
		}

		public int _ShutterHold = 50;
		public int ShutterHold
		{
			get => _ShutterHold;
			set => SetProperty(ref _ShutterHold, value, (v) => Math.Max(0, v));
		}

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
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
