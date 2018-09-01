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

		public event EventHandler Stopped;

		public RunViewModel()
		{
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.Pan.PropertyChanged += Pan_PropertyChanged; ;
		}

		public void Init(RunCommand cmd)
		{
			PanDiff = SlideDiff = false;

			Command = cmd;

			switch (Command)
			{
				case RunCommand.MoveToIn:
					StatusMsg = "Moving to IN";
					SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideIn;
					PanDiff = Comm.Pan.Position != Comm.Sequence.PanIn;
					if (SlideDiff || PanDiff)
					{
						Comm.Slide.Move(Comm.Sequence.SlideIn);
						Comm.Pan.Move(Comm.Sequence.PanIn);
					}
					else
					{
						Stop();
					}
					break;
				case RunCommand.MoveToOut:
					StatusMsg = "Moving to OUT";
					SlideDiff = Comm.Slide.Position != Comm.Sequence.SlideOut;
					PanDiff = Comm.Pan.Position != Comm.Sequence.PanOut;
					if (SlideDiff || PanDiff)
					{
						Comm.Slide.Move(Comm.Sequence.SlideOut);
						Comm.Pan.Move(Comm.Sequence.PanOut);
					}
					else
					{
						Stop();
					}
					break;
				case RunCommand.RunSequence:
					break;
				default:
					break;
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

		private void Pan_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("PanPosition");
			else if (e.PropertyName == "Speed")
			{ 
				OnPropertyChanged("PanSpeed");
				if (Comm.Pan.Speed == 0)
					PanDiff = false;
				CheckStopped();
			}
		}

		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("SlidePosition");
			else if (e.PropertyName == "Speed")
			{
				OnPropertyChanged("SlideSpeed");
				if (Comm.Slide.Speed == 0)
					SlideDiff = false;
				CheckStopped();
			}
		}

		void CheckStopped()
		{
			if (Command != RunCommand.Stopped && !SlideDiff && !PanDiff)
			{
				Stop();
			}
		}

		public int SlidePosition { get => SliderComm.Instance.Slide.Position; }

		public double SlideSpeed { get => SliderComm.Instance.Slide.Speed; }

		public int PanPosition { get => SliderComm.Instance.Pan.Position; }

		public double PanSpeed { get => SliderComm.Instance.Pan.Speed; }

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
