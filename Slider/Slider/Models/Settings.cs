using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CamSlider.Models
{
    public class Settings : INotifyPropertyChanged
	{
		private bool _MotorLocation = true;	// false means motor/stepper is on the left
		public bool MotorLocation
		{
			get => _MotorLocation;
			set => SetProperty(ref _MotorLocation, value);
		}

		private uint _SlideAcceleration = 25;
		public uint SlideAcceleration
		{
			get => _SlideAcceleration;
			set => SetProperty(ref _SlideAcceleration, value, (v) => Math.Min(50, v));
		}

		public uint _SlideMoveSpeed = 30;
		public uint SlideMoveSpeed
		{
			get => _SlideMoveSpeed;
			set => SetProperty(ref _SlideMoveSpeed, value, (v) => Math.Min(SliderComm.Instance.Slide.SpeedLimit, v));
		}

		private uint _PanAcceleration = 45;
		public uint PanAcceleration
		{
			get => _PanAcceleration;
			set => SetProperty(ref _PanAcceleration, value, (v) => Math.Min(90, v));
		}

		public uint _PanMoveSpeed = 55;
		public uint PanMoveSpeed
		{
			get => _PanMoveSpeed;
			set => SetProperty(ref _PanMoveSpeed, value, (v) => Math.Min(SliderComm.Instance.Pan.SpeedLimit, v));
		}

		public uint _FocusDelay = 150;
		public uint FocusDelay
		{
			get => _FocusDelay;
			set => SetProperty(ref _FocusDelay, value, (v) => Math.Max(0, v));
		}

		public uint _ShutterHold = 50;
		public uint ShutterHold
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
