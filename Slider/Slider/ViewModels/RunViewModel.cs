using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.ViewModels
{
	public enum RunCommand
	{
		Stopped,
		MoveToIn,
		MoveToOut,
		RunSequence,
	}

	public class RunViewModel : INotifyPropertyChanged
	{
		public RunCommand Command;
		protected SliderComm Comm { get => SliderComm.Instance; }
		bool PanDiff;
		bool SlideDiff;
		bool PreMoveToIn;

		public event EventHandler Stopped;

		public RunViewModel()
		{
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.Pan.PropertyChanged += Pan_PropertyChanged; ;
		}

		public void Init(RunCommand cmd)
		{
			PanDiff = SlideDiff = PreMoveToIn = CanPlay = false;
			PanTimeRemaining = SlideTimeRemaining = 0;

			Command = cmd;

			switch (Command)
			{
				case RunCommand.MoveToIn:
					if (!SetupMoveToIn())
					{
						Stop();
					}
					break;
				case RunCommand.MoveToOut:
					if (!SetupMoveToOut())
					{
						Stop();
					}
					break;
				case RunCommand.RunSequence:
					PreMoveToIn = true;
					if (!SetupMoveToIn())
					{
						PreMoveToIn = false;
						CanPlay = true;
						StatusMsg = "Ready to run";
					}
					break;
				default:
					break;
			}
		}

		bool SetupRun()
		{
			CanPlay = false;
			StatusMsg = "Running";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideOut;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanOut;
			if (SlideDiff || PanDiff)
			{
				var slideMaxSpeed = Comm.Slide.MaxSpeedForDistanceAndTime(Comm.Sequence.SlideOut - Comm.Slide.Position, Comm.Sequence.Duration);
				var panMaxSpeed = Comm.Pan.MaxSpeedForDistanceAndTime(Comm.Sequence.PanOut - Comm.Pan.Position, Comm.Sequence.Duration);
				Debug.WriteLine($"Run slide: {slideMaxSpeed} pan: {panMaxSpeed}");
				Comm.Slide.Move(Comm.Sequence.SlideOut, slideMaxSpeed);
				Comm.Pan.Move(Comm.Sequence.PanOut, panMaxSpeed);
				return true;
			}
			return false;
		}

		bool SetupMoveToIn()
		{
			StatusMsg = "Moving to IN";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideIn;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanIn;
			if (SlideDiff || PanDiff)
			{
				Comm.Slide.Move(Comm.Sequence.SlideIn);
				Comm.Pan.Move(Comm.Sequence.PanIn);
				return true;
			}
			return false;
		}

		bool SetupMoveToOut()
		{
			StatusMsg = "Moving to OUT";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideOut;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanOut;
			if (SlideDiff || PanDiff)
			{
				Comm.Slide.Move(Comm.Sequence.SlideOut);
				Comm.Pan.Move(Comm.Sequence.PanOut);
				return true;
			}
			return false;
		}

		public void Play()
		{
			if (!SetupRun())
			{
				Stop();
			}
		}

		public void Stop()
		{
			Command = RunCommand.Stopped;
			StatusMsg = "Stopped";
			Comm.Slide.Vector = 0;
			Comm.Pan.Vector = 0;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				Stopped?.Invoke(this, EventArgs.Empty);
			});
		}

		public string _StatusMsg = "";
		public string StatusMsg
		{
			get => _StatusMsg;
			set => SetProperty(ref _StatusMsg, value);
		}

		public bool _CanPlay;
		public bool CanPlay
		{
			get => _CanPlay;
			set => SetProperty(ref _CanPlay, value);
		}

		private void Pan_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
			{
				OnPropertyChanged("PanPosition");
				PanTimeRemaining = Comm.Pan.TimeRemaining(Comm.Pan.GoalPosition - PanPosition);
				OnPropertyChanged("TimeRemaining");
			}
			else if (e.PropertyName == "Speed")
			{
				OnPropertyChanged("PanSpeed");
				if (Comm.Pan.Speed == 0)
				{
					PanDiff = false;
					PanTimeRemaining = 0;
					OnPropertyChanged("TimeRemaining");
				}
				CheckStopped();
			}
		}

		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
			{
				OnPropertyChanged("SlidePosition");
				SlideTimeRemaining = Comm.Slide.TimeRemaining(Comm.Slide.GoalPosition - SlidePosition);
				OnPropertyChanged("TimeRemaining");
			}
			else if (e.PropertyName == "Speed")
			{
				OnPropertyChanged("SlideSpeed");
				if (Comm.Slide.Speed == 0)
				{
					SlideDiff = false;
					SlideTimeRemaining = 0;
					OnPropertyChanged("TimeRemaining");
				}
				CheckStopped();
			}
		}

		void CheckStopped()
		{
			if (Command != RunCommand.Stopped && !SlideDiff && !PanDiff)
			{
				if (!PreMoveToIn)
				{
					Stop();
				}
				else
				{
					PreMoveToIn = false;
					CanPlay = true;
					StatusMsg = "Ready to run";
				}
			}
		}

		public int SlidePosition { get => Comm.Slide.Position; }

		public double SlideSpeed { get => Comm.Slide.Speed; }

		public int PanPosition { get => Comm.Pan.Position; }

		public double PanSpeed { get => Comm.Pan.Speed; }

		double PanTimeRemaining;
		double SlideTimeRemaining;
		public double TimeRemaining
		{
			get
			{
			//	Debug.WriteLine($"++> Slide Time: {SlideTimeRemaining}  Pan Time: {PanTimeRemaining}");
				return SlideTimeRemaining > 0 ? SlideTimeRemaining : PanTimeRemaining;
			}
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
