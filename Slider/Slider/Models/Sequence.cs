using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.Models
{
    public class Sequence : INotifyPropertyChanged
    {
		protected int _SlideIn = 0;
		public int SlideIn
		{
			get { return _SlideIn; }
			set { SetProperty(ref _SlideIn, Stepper.LimitSlideValue(value)); }
		}

		protected int _SlideOut = 640;
		public int SlideOut
		{
			get { return _SlideOut; }
			set { SetProperty(ref _SlideOut, Stepper.LimitSlideValue(value)); }
		}

		protected int _PanIn = 0;
		public int PanIn
		{
			get { return _PanIn; }
			set { SetProperty(ref _PanIn, Stepper.LimitPanValue(value)); }
		}

		protected int _PanOut = 0;
		public int PanOut
		{
			get { return _PanOut; }
			set { SetProperty(ref _PanOut, Stepper.LimitPanValue(value)); }
		}

		protected int _Duration = 1800;
		public int Duration
		{
			get { return _Duration; }
			set { SetProperty(ref _Duration, Math.Max(0, value), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; }); }
		}

		protected int _Playback = 120;
		public int Playback
		{
			get { return _Playback; }
			set { SetProperty(ref _Playback, Math.Max(0, value), onChanged: () => { Frames = Playback * FramesPerSecond; }); }
		}

		protected int _FramesPerSecond = 30;
		public int FramesPerSecond
		{
			get { return _FramesPerSecond; }
			set { SetProperty(ref _FramesPerSecond, Math.Max(0, value), onChanged: () => { Frames = Playback * FramesPerSecond; }); }
		}

		protected int _Frames = 0;
		public int Frames
		{
			get { return _Frames; }
			protected set { SetProperty(ref _Frames, Math.Max(0, value), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; }); }
		}

		protected double _Interval = 0;
		public double Interval
		{
			get { return _Interval; }
			protected set { SetProperty(ref _Interval, Math.Max(0, value)); }
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
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
