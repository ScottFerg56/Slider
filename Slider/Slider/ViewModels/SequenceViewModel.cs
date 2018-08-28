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
		public Sequence Seq;
		public Command MoveToInCommand { get; set; }
		public Command MoveToOutCommand { get; set; }
		public Command SetInFromCurrentCommand { get; set; }
		public Command SetOutFromCurrentCommand { get; set; }

		public SequenceViewModel()
		{
			MoveToInCommand = new Command(() => ExecuteMoveToInCommand());
			MoveToOutCommand = new Command(() => ExecuteMoveToOutCommand());
			SetInFromCurrentCommand = new Command(() => ExecuteSetInFromCurrentCommand());
			SetOutFromCurrentCommand = new Command(() => ExecuteSetOutFromCurrentCommand());

			Seq = Services.DataStore.LoadDataStore<Sequence>("sequence") ?? new Sequence();
			Seq.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, e); };
		}

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

		public int Duration
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		protected TimeSpan DurationTimeSpan
		{
			get => new TimeSpan(0, 0, Duration);
			set => Duration = (int)value.TotalSeconds;
		}

		public int DurationMins
		{
			get => DurationTimeSpan.Minutes;
			set
			{
				var ts = DurationTimeSpan;
				DurationTimeSpan = new TimeSpan(0, Math.Max(0, value), ts.Seconds);
				OnPropertyChanged();
			}
		}

		public int DurationSecs
		{
			get => DurationTimeSpan.Seconds;
			set
			{
				var ts = DurationTimeSpan;
				var v = (value + 60) % 60;
				DurationTimeSpan = new TimeSpan(0, ts.Minutes, v);
				OnPropertyChanged();
			}
		}

		public int Playback
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		protected TimeSpan PlaybackTimeSpan
		{
			get => new TimeSpan(0, 0, Playback);
			set => Playback = (int)value.TotalSeconds;
		}

		public int PlaybackMins
		{
			get => PlaybackTimeSpan.Minutes;
			set
			{
				var ts = PlaybackTimeSpan;
				PlaybackTimeSpan = new TimeSpan(0, Math.Max(0, value), ts.Seconds);
				OnPropertyChanged();
			}
		}

		public int PlaybackSecs
		{
			get => PlaybackTimeSpan.Seconds;
			set
			{
				var ts = PlaybackTimeSpan;
				var v = (value + 60) % 60;
				PlaybackTimeSpan = new TimeSpan(0, ts.Minutes, Math.Max(0, Math.Min(59, v)));
				OnPropertyChanged();
			}
		}

		public int FramesPerSecond
		{
			get => GetProperty<int>();
			set => SetProperty(value);
		}

		public int Frames
		{
			get => GetProperty<int>();
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

		void ExecuteMoveToInCommand()
		{
			Debug.WriteLine("Move To In -- not implemented");
		}

		void ExecuteMoveToOutCommand()
		{
			Debug.WriteLine("Move To Out -- not implemented");
		}

		void ExecuteSetInFromCurrentCommand()
		{
			Debug.WriteLine("Set In From Current -- not implemented");
		}

		void ExecuteSetOutFromCurrentCommand()
		{
			Debug.WriteLine("Set Out From Current -- not implemented");
		}

		protected T GetProperty<T>([CallerMemberName]string propertyName = "")
		{
			return (T)Seq.GetType().GetProperty(propertyName).GetValue(Seq);
		}

		protected bool SetProperty<T>(T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			var pi = Seq.GetType().GetProperty(propertyName);
			if (EqualityComparer<T>.Default.Equals((T)pi.GetValue(Seq), value))
				return false;

			pi.SetValue(Seq, value);
			onChanged?.Invoke();
			// the Seq object will propagate changes back up thru us
		//	OnPropertyChanged(propertyName);
			SaveSequence();
			return true;
		}

		protected void SaveSequence()
		{
			Services.DataStore.SaveDataStore("sequence", Seq);
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
