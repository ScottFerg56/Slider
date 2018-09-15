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
		Resume,
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
			Comm.Pan.PropertyChanged += Pan_PropertyChanged;
			Comm.Camera.PropertyChanged += Camera_PropertyChanged;
			Comm.StateChange += Comm_StateChange;
		}

		private void Comm_StateChange(object sender, EventArgs e)
		{
			if (Comm.State != BlueState.Connected)
				Stop();
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
				case RunCommand.Resume:
					switch (Comm.Action)
					{
						case SliderComm.Actions.MovingToIn:
							if (!SetupMoveToIn(true))
							{
								Stop();
							}
							break;
						case SliderComm.Actions.MovingToOut:
							if (!SetupMoveToOut(true))
							{
								Stop();
							}
							break;
						case SliderComm.Actions.Running:
							if (!SetupRun(true))
							{
								Stop();
							}
							break;
						default:
							Debug.WriteLine($"Invalid resume action: {Comm.Action}");
							break;
					}
					break;
				default:
					break;
			}
		}

		bool SetupRun(bool resume = false)
		{
			Comm.Action = SliderComm.Actions.Running;
			CanPlay = false;
			StatusMsg = (resume ? "Resume " : "") + "Running";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideOut;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanOut;
			if (SlideDiff || PanDiff || Comm.Sequence.Intervalometer && Comm.Sequence.Frames != 0)
			{
				if (resume)
					return true;
				var slideMaxSpeed = Comm.Slide.MaxSpeedForDistanceAndTime(Comm.Sequence.SlideOut - Comm.Slide.Position, Comm.Sequence.Duration);
				var panMaxSpeed = Comm.Pan.MaxSpeedForDistanceAndTime(Comm.Sequence.PanOut - Comm.Pan.Position, Comm.Sequence.Duration);
				Debug.WriteLine($"Run slide: {slideMaxSpeed} pan: {panMaxSpeed}");
				Comm.Slide.Move(Comm.Sequence.SlideOut, slideMaxSpeed);
				Comm.Pan.Move(Comm.Sequence.PanOut, panMaxSpeed);
				if (Comm.Sequence.Intervalometer)
				{
					Comm.Camera.FocusDelay = Comm.Settings.FocusDelay;
					Comm.Camera.ShutterHold = Comm.Settings.ShutterHold;
					Comm.Camera.Interval = (uint)Math.Round(Comm.Sequence.Interval * 1000);
					// setting Frames will trigger the device to begin the camera shutter sequence
					Comm.Camera.Frames = Comm.Sequence.Frames;
				}
				else
				{
					Comm.Camera.Frames = 0;
				}
				return true;
			}
			return false;
		}

		bool SetupMoveToIn(bool resume = false)
		{
			Comm.Action = SliderComm.Actions.MovingToIn;
			StatusMsg = (resume ? "Resume " : "") + "Moving to IN";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideIn;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanIn;
			if (SlideDiff || PanDiff)
			{
				if (resume)
					return true;
				Comm.Slide.Move(Comm.Sequence.SlideIn);
				Comm.Pan.Move(Comm.Sequence.PanIn);
				return true;
			}
			return false;
		}

		bool SetupMoveToOut(bool resume = false)
		{
			Comm.Action = SliderComm.Actions.MovingToOut;
			StatusMsg = (resume ? "Resume " : "") + "Moving to OUT";
			SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideOut;
			PanDiff = Comm.Pan.Position != Comm.Sequence.PanOut;
			if (SlideDiff || PanDiff)
			{
				if (resume)
					return true;
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
			Comm.Action = SliderComm.Actions.None;
			Command = RunCommand.Stopped;
			StatusMsg = "Stopped";
			Comm.Slide.Vector = 0;
			Comm.Pan.Vector = 0;
			Comm.Camera.Frames = 0;
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
				PanTimeRemaining = Comm.Pan.TimeRemaining(Comm.Pan.TargetPosition - PanPosition);
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
				SlideTimeRemaining = Comm.Slide.TimeRemaining(Comm.Slide.TargetPosition - SlidePosition);
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

		private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Frames")
			{
				OnPropertyChanged("FramesRemaining");
				CheckStopped();
			}
		}

		void CheckStopped()
		{
			if (Command != RunCommand.Stopped && !SlideDiff && !PanDiff && (!Comm.Sequence.Intervalometer || Comm.Camera.Frames == 0))
			{
				if (!PreMoveToIn)
				{
					Stop();
				}
				else
				{
					PreMoveToIn = false;
					Comm.Action = SliderComm.Actions.None;
					CanPlay = true;
					StatusMsg = "Ready to run";
				}
			}
		}

		public int SlidePosition { get => Comm.Slide.Position; }

		public double SlideSpeed { get => Comm.Slide.Speed; }

		public int PanPosition { get => Comm.Pan.Position; }

		public double PanSpeed { get => Comm.Pan.Speed; }

		public uint FramesRemaining { get => Comm.Camera.Frames; }

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
