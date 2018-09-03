using Newtonsoft.Json;
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
		protected int _SlideIn = 100;
		public int SlideIn
		{
			get => _SlideIn;
			set => SetProperty(ref _SlideIn, value, SliderComm.Instance.Slide.LimitValue);
		}

		protected int _SlideOut = 400;
		public int SlideOut
		{
			get => _SlideOut;
			set => SetProperty(ref _SlideOut, value, SliderComm.Instance.Slide.LimitValue);
		}

		protected int _PanIn = 30;
		public int PanIn
		{
			get => _PanIn;
			set => SetProperty(ref _PanIn, value, SliderComm.Instance.Pan.LimitValue);
		}

		protected int _PanOut = -30;
		public int PanOut
		{
			get => _PanOut;
			set => SetProperty(ref _PanOut, value, SliderComm.Instance.Pan.LimitValue);
		}

		protected int _Duration = 60;
		public int Duration
		{
			get => _Duration;
			set => SetProperty(ref _Duration, value, (v) => Math.Max(0, v), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; });
		}

		protected int _Playback = 10;
		public int Playback
		{
			get => _Playback;
			set => SetProperty(ref _Playback, value, (v) => Math.Max(0, v), onChanged: () => { Frames = Playback * FramesPerSecond; });
		}

		protected int _FramesPerSecond = 30;
		public int FramesPerSecond
		{
			get => _FramesPerSecond;
			set => SetProperty(ref _FramesPerSecond, value, (v) => Math.Max(0, v), onChanged: () => { Frames = Playback * FramesPerSecond; });
		}

		// Playback x FPS -> #Frames
		protected int _Frames = 0;
		[JsonIgnore]
		public int Frames
		{
			get => _Frames;
			protected set => SetProperty(ref _Frames, value, (v) => Math.Max(0, v), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; });
		}

		// Duration / #Frames -> Interval
		protected double _Interval = 0;
		[JsonIgnore]
		public double Interval
		{
			get => _Interval;
			protected set => SetProperty(ref _Interval, value, (v) => Math.Max(0, v));
		}

		protected bool _Intervalometer = false;
		public bool Intervalometer
		{
			get => _Intervalometer;
			set => SetProperty(ref _Intervalometer, value);
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
