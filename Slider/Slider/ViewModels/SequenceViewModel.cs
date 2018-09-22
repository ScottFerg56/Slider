using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CamSlider.ViewModels
{
	/// <summary>
	/// ViewModel for the SequencePage.
	/// </summary>
	public class SequenceViewModel : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		public Sequence Sequence { get => Comm.Sequence; }

		public SequenceViewModel()
		{
			// propagate Sequence changes on through
			Sequence.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, e); };
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.StateChange += Comm_StateChange;
		}

		/// <summary>
		/// Propagate the communications state change as an Enabled change.
		/// </summary>
		private void Comm_StateChange(object sender, EventArgs e)
		{
			OnPropertyChanged("Enabled");
		}

		/// <summary>
		/// Propagate the Slide Calibrated change as an Enabled change.
		/// </summary>
		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Calibrated")
				OnPropertyChanged("Enabled");
		}

		/// <summary>
		/// Enable some UI elements only if Connected and Calibrated
		/// </summary>
		public bool Enabled { get => Comm.Slide.Calibrated && Comm.State == BlueState.Connected; }

		/// <summary>
		/// Get/set the Slide In point, in millimeters, 0 at the motor location.
		/// </summary>
		public int SlideIn
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Pan In point, in degrees, positive is CCW.
		/// </summary>
		public int PanIn
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Slide Out point, in millimeters, 0 at the motor location.
		/// </summary>
		public int SlideOut
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Pan Out point, in degrees, positive is CCW.
		/// </summary>
		public int PanOut
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Duration of the move, in seconds.
		/// </summary>
		public uint Duration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Duration as a TimeSpan.
		/// </summary>
		protected TimeSpan DurationTimeSpan
		{
			get => new TimeSpan(0, 0, (int)Duration);
			set => Duration = (uint)value.TotalSeconds;
		}

		/// <summary>
		/// Get/set the minutes component of the Duration.
		/// </summary>
		public uint DurationMins
		{
			get => (uint)DurationTimeSpan.Minutes;
			set
			{
				var ts = DurationTimeSpan;
				DurationTimeSpan = new TimeSpan(0, (int)Math.Max(0, value), ts.Seconds);
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Get/set the seconds component of the Duration.
		/// </summary>
		public uint DurationSecs
		{
			get => (uint)DurationTimeSpan.Seconds;
			set
			{
				var ts = DurationTimeSpan;
				var v = (value + 60) % 60;
				DurationTimeSpan = new TimeSpan(0, ts.Minutes, (int)v);
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Get/set the timelapse Playback time, in seconds.
		/// </summary>
		public uint Playback
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get/set the Playback time as a TimeSpan.
		/// </summary>
		protected TimeSpan PlaybackTimeSpan
		{
			get => new TimeSpan(0, 0, (int)Playback);
			set => Playback = (uint)value.TotalSeconds;
		}

		/// <summary>
		/// Get/set the minutes component of the Playback time.
		/// </summary>
		public uint PlaybackMins
		{
			get => (uint)PlaybackTimeSpan.Minutes;
			set
			{
				var ts = PlaybackTimeSpan;
				PlaybackTimeSpan = new TimeSpan(0, (int)Math.Max(0, value), ts.Seconds);
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Get/set the seconds component of the Playback time.
		/// </summary>
		public uint PlaybackSecs
		{
			get => (uint)PlaybackTimeSpan.Seconds;
			set
			{
				var ts = PlaybackTimeSpan;
				var v = (value + 60) % 60;
				PlaybackTimeSpan = new TimeSpan(0, ts.Minutes, (int)Math.Max(0, Math.Min(59, v)));
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Get/set the timelapse FramesPerSecond.
		/// </summary>
		public uint FramesPerSecond
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Gets the calculated number of frames required for the timelapse parameters.
		/// </summary>
		public uint Frames
		{
			get => GetProperty<uint>();
		}

		/// <summary>
		/// Gets the calculated interval between frames for the timelapse parameters.
		/// </summary>
		public double Interval
		{
			get => GetProperty<double>();
		}

		/// <summary>
		/// Get/set whether intervalometer shutter control should be enabled for the sequence.
		/// </summary>
		public bool Intervalometer
		{
			get => GetProperty<bool>();
			set => SetProperty(value);
		}

		/// <summary>
		/// Get a property value from the Sequence object.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="propertyName">The name of the property to access.</param>
		/// <returns>The value of the property.</returns>
		/// <remarks>Defers property access to the Sequence.</remarks>
		protected T GetProperty<T>([CallerMemberName]string propertyName = "")
		{
			return (T)Sequence.GetType().GetProperty(propertyName).GetValue(Sequence);
		}

		/// <summary>
		/// Set a property value on the Sequence object.
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
			var pi = Sequence.GetType().GetProperty(propertyName);
			if (EqualityComparer<T>.Default.Equals((T)pi.GetValue(Sequence), value))
				return false;

			pi.SetValue(Sequence, value);
			onChanged?.Invoke();
			// we've setup to propagate changes from the Sequence object back up thru us
			// so no need to do that here
		//	OnPropertyChanged(propertyName);
			// save the Sequence with every change
			SaveSequence();
			return true;
		}

		/// <summary>
		/// Save the Sequence to the data store.
		/// </summary>
		protected void SaveSequence()
		{
			Services.DataStore.SaveDataStore("sequence", Sequence);
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
