/*
OOOOOO                  OO   OO    OO                   OO   OO            OOO            OOO
 OO  OO                 OO   OO    OO                   OOO OOO             OO             OO
 OO  OO                 OO   OO                         OOOOOOO             OO             OO
 OO  OO OO  OO  OO OOO  OO   OO   OOO    OOOOO  OO   OO OOOOOOO  OOOOO    OOOO   OOOOO     OO
 OOOOO  OO  OO   OOOOOO OO   OO    OO   OO   OO OO   OO OO O OO OO   OO  OO OO  OO   OO    OO
 OO OO  OO  OO   OO  OO OO   OO    OO   OOOOOOO OO O OO OO   OO OO   OO OO  OO  OOOOOOO    OO
 OO  OO OO  OO   OO  OO  OO OO     OO   OO      OO O OO OO   OO OO   OO OO  OO  OO         OO
 OO  OO OO  OO   OO  OO   OOO      OO   OO   OO  OOOOO  OO   OO OO   OO OO  OO  OO   OO    OO
OOO  OO  OOO OO  OO  OO    O      OOOO   OOOOO    O O   OO   OO  OOOOO   OOO OO  OOOOO    OOOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using Platforms.BluePortable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.ViewModels
{
	/// <summary>
	/// State of Run operations.
	/// </summary>
	public enum RunCommand
	{
		/// <summary>motion is stopped</summary>
		Stopped,
		/// <summary>moving to the In point</summary>
		MoveToIn,
		/// <summary>moving to the Out point</summary>
		MoveToOut,
		/// <summary>running the sequence from In to Out</summary>
		RunSequence,
		/// <summary>resuming a previous operation</summary>
		Resume,
	}

	/// <summary>
	/// ViewModel for the RunPage.
	/// </summary>
	public class RunViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// The operation in progress.
		/// </summary>
		public RunCommand Command;

		protected SliderComm Comm { get => SliderComm.Instance; }
		bool PanDiff;		// true if need to move Pan
		bool SlideDiff;		// true if need to move Slide
		bool PreMoveToIn;	// true if moving to In in preparation for RunSequence

		/// <summary>
		/// Fired when the current operation is complete.
		/// </summary>
		public event EventHandler Stopped;

		public RunViewModel()
		{
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.Pan.PropertyChanged += Pan_PropertyChanged;
			Comm.Intervalometer.PropertyChanged += Intervalometer_PropertyChanged;
			Comm.StateChange += Comm_StateChange;
		}

		/// <summary>
		/// Stop when disconnected.
		/// </summary>
		private void Comm_StateChange(object sender, EventArgs e)
		{
			if (Comm.State != BlueState.Connected)
				Stop();
		}

		/// <summary>
		/// Initialize operations with a desired command state.
		/// </summary>
		/// <param name="cmd"></param>
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
					// try to figure out what we were doing when the connection was lost
					// and get back to it
					switch (Comm.Global.Action)
					{
						case GlobalElement.Actions.MovingToIn:
							if (!SetupMoveToIn(true))
							{
								Stop();
							}
							break;
						case GlobalElement.Actions.MovingToOut:
							if (!SetupMoveToOut(true))
							{
								Stop();
							}
							break;
						case GlobalElement.Actions.Running:
							if (!SetupRun(true))
							{
								Stop();
							}
							break;
						default:
							Debug.WriteLine($"--> Invalid resume action: {Comm.Global.Action}");
							break;
					}
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Setup to run throught the sequence, having already moved to the In point.
		/// </summary>
		/// <param name="resume">True if resuming from some kind of disconnect.</param>
		/// <returns>True if a sequence was actually started.</returns>
		bool SetupRun(bool resume = false)
		{
			// send our current action to the device so we can recover it later after any kind of disconnect
			Comm.Global.Action = GlobalElement.Actions.Running;
			CanPlay = false;
			StatusMsg = (resume ? "Resume " : "") + "Running";
			// remember which parts need moving
			SlideDiff = (int)Math.Round(Comm.Slide.Position) != Comm.Sequence.SlideOut;
			PanDiff = (int)Math.Round(Comm.Pan.Position) != Comm.Sequence.PanOut;
			// see if something needs doing
			if (SlideDiff || PanDiff || Comm.Sequence.Intervalometer && Comm.Sequence.Frames != 0)
			{
				if (resume)			// if resuming, just run with it
					return true;
				// calculate the proper max speeds to achieve the desired durations for the moves
				var slideMaxSpeed = Comm.Slide.MaxSpeedForDistanceAndTime(Comm.Sequence.SlideOut - (int)Math.Round(Comm.Slide.Position), Comm.Sequence.Duration);
				var panMaxSpeed = Comm.Pan.MaxSpeedForDistanceAndTime(Comm.Sequence.PanOut - (int)Math.Round(Comm.Pan.Position), Comm.Sequence.Duration);
				// initiate Slide and Pan movement
				Comm.Slide.Move(Comm.Sequence.SlideOut, slideMaxSpeed, Comm.Settings.SlideAcceleration);
				Comm.Pan.Move(Comm.Sequence.PanOut, panMaxSpeed, Comm.Settings.PanAcceleration);
				if (Comm.Sequence.Intervalometer)
				{
					// start the intervalometer sequence
					Comm.Intervalometer.FocusDelay = Comm.Settings.FocusDelay;
					Comm.Intervalometer.ShutterHold = Comm.Settings.ShutterHold;
					Comm.Intervalometer.Interval = (uint)Math.Round(Comm.Sequence.Interval * 1000);
					// setting Frames will trigger the device to begin the intervalometer shutter sequence
					Comm.Intervalometer.Frames = Comm.Sequence.Frames;
				}
				else
				{
					// no intervalometer action, make sure the device knows
					Comm.Intervalometer.Frames = 0;
				}
				return true;
			}
			// nothing to do!
			return false;
		}

		/// <summary>
		/// Setup for a move to the In point
		/// </summary>
		/// <param name="resume">True if resuming from some kind of disconnect.</param>
		/// <returns>True if a sequence was actually started.</returns>
		bool SetupMoveToIn(bool resume = false)
		{
			// send our current action to the device so we can recover it later after any kind of disconnect
			Comm.Global.Action = GlobalElement.Actions.MovingToIn;
			StatusMsg = (resume ? "Resume " : "") + "Moving to IN";
			// remember which parts need moving
			SlideDiff = (int)Math.Round(Comm.Slide.Position) != Comm.Sequence.SlideIn;
			PanDiff = (int)Math.Round(Comm.Pan.Position) != Comm.Sequence.PanIn;
			// see if something needs doing (no intervalometer action here)
			if (SlideDiff || PanDiff)
			{
				if (resume)         // if resuming, just run with it
					return true;
				// initiate Slide and Pan movement
				Comm.Slide.Move(Comm.Sequence.SlideIn, Comm.Settings.SlideMoveSpeed, Comm.Settings.SlideMoveSpeed);
				Comm.Pan.Move(Comm.Sequence.PanIn, Comm.Settings.PanMoveSpeed, Comm.Settings.PanAcceleration);
				return true;
			}
			// nothing to do!
			return false;
		}

		/// <summary>
		/// Setup for a move to the Out point
		/// </summary>
		/// <param name="resume">True if resuming from some kind of disconnect.</param>
		/// <returns>True if a sequence was actually started.</returns>
		bool SetupMoveToOut(bool resume = false)
		{
			// send our current action to the device so we can recover it later after any kind of disconnect
			Comm.Global.Action = GlobalElement.Actions.MovingToOut;
			StatusMsg = (resume ? "Resume " : "") + "Moving to OUT";
			// remember which parts need moving
			SlideDiff = (int)Math.Round(Comm.Slide.Position) != Comm.Sequence.SlideOut;
			PanDiff = (int)Math.Round(Comm.Pan.Position) != Comm.Sequence.PanOut;
			// see if something needs doing (no intervalometer action here)
			if (SlideDiff || PanDiff)
			{
				if (resume)         // if resuming, just run with it
					return true;
				// initiate Slide and Pan movement
				Comm.Slide.Move(Comm.Sequence.SlideOut, Comm.Settings.SlideMoveSpeed, Comm.Settings.SlideMoveSpeed);
				Comm.Pan.Move(Comm.Sequence.PanOut, Comm.Settings.PanMoveSpeed, Comm.Settings.PanAcceleration);
				return true;
			}
			// nothing to do!
			return false;
		}

		/// <summary>
		/// Setup to Run the sequence, or exit.
		/// </summary>
		public void Play()
		{
			if (!SetupRun())
			{
				Stop();
			}
		}

		/// <summary>
		/// Cleanup after an operation or abort one in progress.
		/// </summary>
		public void Stop()
		{
			Comm.Global.Action = GlobalElement.Actions.None;
			Command = RunCommand.Stopped;
			StatusMsg = "Stopped";
			Comm.Slide.Velocity = 0;
			Comm.Pan.Velocity = 0;
			Comm.Intervalometer.Frames = 0;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				Stopped?.Invoke(this, EventArgs.Empty);
			});
		}

		public string _StatusMsg = "";
		/// <summary>
		/// Status message about current operation.
		/// </summary>
		public string StatusMsg
		{
			get => _StatusMsg;
			protected set => SetProperty(ref _StatusMsg, value);
		}

		public bool _CanPlay;
		/// <summary>
		/// True if a Run operation is ready and the MoveToIn has completed.
		/// </summary>
		public bool CanPlay
		{
			get => _CanPlay;
			protected set => SetProperty(ref _CanPlay, value);
		}

		/// <summary>
		/// Propagate changes for Pan-related properties.
		/// </summary>
		private void Pan_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
			{
				OnPropertyChanged("PanPosition");
				// update time remaining and notify
				PanTimeRemaining = Comm.Pan.TimeRemaining((int)Math.Round(Comm.Pan.TargetPosition) - PanPosition);
				OnPropertyChanged("TimeRemaining");
			}
			else if (e.PropertyName == "Speed")
			{
				OnPropertyChanged("PanSpeed");
				if (Comm.Pan.Speed == 0)
				{
					// Pan movement is done
					PanDiff = false;
					// update time remaining and notify
					PanTimeRemaining = 0;
					OnPropertyChanged("TimeRemaining");
				}
				CheckStopped();
			}
		}

		/// <summary>
		/// Propagate changes for Slide-related properties.
		/// </summary>
		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
			{
				OnPropertyChanged("SlidePosition");
				// update time remaining and notify
				SlideTimeRemaining = Comm.Slide.TimeRemaining((int)Math.Round(Comm.Slide.TargetPosition) - SlidePosition);
				OnPropertyChanged("TimeRemaining");
			}
			else if (e.PropertyName == "Speed")
			{
				OnPropertyChanged("SlideSpeed");
				if (Comm.Slide.Speed == 0)
				{
					// Slide movement is done
					SlideDiff = false;
					// update time remaining and notify
					SlideTimeRemaining = 0;
					OnPropertyChanged("TimeRemaining");
				}
				CheckStopped();
			}
		}

		/// <summary>
		/// Propagate changes for Intervalometer-related properties.
		/// </summary>
		private void Intervalometer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Frames")
			{
				OnPropertyChanged("FramesRemaining");
				OnPropertyChanged("TimeRemaining");
				CheckStopped();
			}
		}

		/// <summary>
		/// Check to see if all sequence actions have completed.
		/// </summary>
		void CheckStopped()
		{
			if (Command != RunCommand.Stopped && !SlideDiff && !PanDiff && (!Comm.Sequence.Intervalometer || Comm.Intervalometer.Frames == 0))
			{
				if (!PreMoveToIn)
				{
					// this wasn't a prelude move to In point for a sequence run
					// so we're done here
					Stop();
				}
				else
				{
					// prelude move complete
					PreMoveToIn = false;
					Comm.Global.Action = GlobalElement.Actions.None;
					// waiting for user to ready the camera and the scene and then press the 'Play' button
					CanPlay = true;
					StatusMsg = "Ready to run";
				}
			}
		}

		/// <summary>
		/// Get the Slide Position.
		/// </summary>
		public int SlidePosition { get => (int)Math.Round(Comm.Slide.Position); }

		/// <summary>
		/// Get the Slide Speed.
		/// </summary>
		public int SlideSpeed { get => (int)Math.Round(Comm.Slide.Speed); }

		/// <summary>
		/// Get the Pan Position.
		/// </summary>
		public int PanPosition { get => (int)Math.Round(Comm.Pan.Position); }

		/// <summary>
		/// Get the Pan Speed.
		/// </summary>
		public int PanSpeed { get => (int)Math.Round(Comm.Pan.Speed); }

		/// <summary>
		/// Get the Frames count.
		/// </summary>
		public uint FramesRemaining { get => Comm.Intervalometer.Frames; }

		double PanTimeRemaining;		// time remaining for Pan movement
		double SlideTimeRemaining;      // time remaining for Slide movement

		/// <summary>
		/// Gets the time remaining for the movement in progress, in seconds.
		/// </summary>
		public double TimeRemaining
		{
			get
			{
				// all component activities were initially calculated to end at the same time
				// though some may not be active at all, those that are should have nearly the same time remaining
				// just take the max of the three components
				double time = Comm.Sequence.Intervalometer ? FramesRemaining * Comm.Intervalometer.Interval / 1000.0 : 0;
				return Math.Max(Math.Max(time, SlideTimeRemaining), PanTimeRemaining);
			}
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
