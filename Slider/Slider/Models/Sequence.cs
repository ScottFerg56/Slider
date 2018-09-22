using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CamSlider.Models
{
	/// <summary>
	/// Represents a sequence moving the Slide and Pan from an In point to an Out point
	/// with accompanying intervalometer action.
	/// </summary>
	public class Sequence : INotifyPropertyChanged
    {
		protected int _SlideIn = 100;
		/// <summary>
		/// Get/set the Slide In point, in millimeters, 0 at the motor location.
		/// </summary>
		public int SlideIn
		{
			get => _SlideIn;
			set => SetProperty(ref _SlideIn, value, SliderComm.Instance.Slide.LimitValue);
		}

		protected int _SlideOut = 400;
		/// <summary>
		/// Get/set the Slide Out point, in millimeters, 0 at the motor location.
		/// </summary>
		public int SlideOut
		{
			get => _SlideOut;
			set => SetProperty(ref _SlideOut, value, SliderComm.Instance.Slide.LimitValue);
		}

		protected int _PanIn = 30;
		/// <summary>
		/// Get/set the Pan In point, in degrees, positive is CCW.
		/// </summary>
		public int PanIn
		{
			get => _PanIn;
			set => SetProperty(ref _PanIn, value, SliderComm.Instance.Pan.LimitValue);
		}

		protected int _PanOut = -30;
		/// <summary>
		/// Get/set the Pan Out point, in degrees, positive is CCW.
		/// </summary>
		public int PanOut
		{
			get => _PanOut;
			set => SetProperty(ref _PanOut, value, SliderComm.Instance.Pan.LimitValue);
		}

		protected uint _Duration = 60;
		/// <summary>
		/// Get/set the Duration of the move, in seconds.
		/// </summary>
		public uint Duration
		{
			get => _Duration;
			set => SetProperty(ref _Duration, value, (v) => Math.Max(0, v), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; });
		}

		protected uint _Playback = 10;
		/// <summary>
		/// Get/set the timelapse Playback time, in seconds.
		/// </summary>
		public uint Playback
		{
			get => _Playback;
			set => SetProperty(ref _Playback, value, (v) => Math.Max(0, v), onChanged: () => { Frames = Playback * FramesPerSecond; });
		}

		protected uint _FramesPerSecond = 30;
		/// <summary>
		/// Get/set the timelapse FramesPerSecond.
		/// </summary>
		public uint FramesPerSecond
		{
			get => _FramesPerSecond;
			set => SetProperty(ref _FramesPerSecond, value, (v) => Math.Max(0, v), onChanged: () => { Frames = Playback * FramesPerSecond; });
		}

		// Playback x FPS -> #Frames
		protected uint _Frames = 0;
		/// <summary>
		/// Gets the calculated number of frames required for the timelapse parameters.
		/// </summary>
		[JsonIgnore]
		public uint Frames
		{
			get => _Frames;
			protected set => SetProperty(ref _Frames, value, (v) => Math.Max(0, v), onChanged: () => { Interval = (Frames == 0) ? 0 : (double)Duration / Frames; });
		}

		// Duration / #Frames -> Interval
		protected double _Interval = 0;
		/// <summary>
		/// Gets the calculated interval between frames for the timelapse parameters.
		/// </summary>
		[JsonIgnore]
		public double Interval
		{
			get => _Interval;
			protected set => SetProperty(ref _Interval, value, (v) => Math.Max(0, v));
		}

		protected bool _Intervalometer = false;
		/// <summary>
		/// Get/set whether intervalometer shutter control should be enabled for the sequence.
		/// </summary>
		public bool Intervalometer
		{
			get => _Intervalometer;
			set => SetProperty(ref _Intervalometer, value);
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
