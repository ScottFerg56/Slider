using CamSlider.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.ViewModels
{
    public class SequenceViewModel : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		public Sequence Sequence { get => Comm.Sequence; }

		public SequenceViewModel()
		{
			Sequence.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, e); };
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.StateChange += Comm_StateChange;
		}

		private void Comm_StateChange(object sender, EventArgs e)
		{
			OnPropertyChanged("Enabled");
		}

		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Homed")
				OnPropertyChanged("Enabled");
		}

		public bool Enabled { get => Comm.Slide.Homed && Comm.State == BlueState.Connected; }

		public int SlideIn
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int PanIn
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int SlideOut
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int PanOut
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public uint Duration
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		protected TimeSpan DurationTimeSpan
		{
			get => new TimeSpan(0, 0, (int)Duration);
			set => Duration = (uint)value.TotalSeconds;
		}

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

		public uint Playback
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		protected TimeSpan PlaybackTimeSpan
		{
			get => new TimeSpan(0, 0, (int)Playback);
			set => Playback = (uint)value.TotalSeconds;
		}

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

		public uint FramesPerSecond
		{
			get => GetProperty<uint>();
			set => SetProperty(value);
		}

		public uint Frames
		{
			get => GetProperty<uint>();
		}

		public double Interval
		{
			get => GetProperty<double>();
		}

		public bool Intervalometer
		{
			get => GetProperty<bool>();
			set => SetProperty(value);
		}

		protected T GetProperty<T>([CallerMemberName]string propertyName = "")
		{
			return (T)Sequence.GetType().GetProperty(propertyName).GetValue(Sequence);
		}

		protected bool SetProperty<T>(T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			var pi = Sequence.GetType().GetProperty(propertyName);
			if (EqualityComparer<T>.Default.Equals((T)pi.GetValue(Sequence), value))
				return false;

			pi.SetValue(Sequence, value);
			onChanged?.Invoke();
			// the Seq object will propagate changes back up thru us
		//	OnPropertyChanged(propertyName);
			SaveSequence();
			return true;
		}

		protected void SaveSequence()
		{
			Services.DataStore.SaveDataStore("sequence", Sequence);
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
