using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CamSlider.ViewModels
{
	/// <summary>
	/// ViewModel for the ManualPage.
	/// </summary>
	public class ManualViewModel : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		public ManualViewModel()
		{
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.Pan.PropertyChanged += Pan_PropertyChanged;
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
		/// Propagate the Pan Position change.
		/// </summary>
		private void Pan_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("PanPosition");
		}

		/// <summary>
		/// Propagate the Slide Position change and the Slide Calibrated change as an Enabled change.
		/// </summary>
		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("SlidePosition");
			else if (e.PropertyName == "Calibrated")
				OnPropertyChanged("Enabled");
		}

		/// <summary>
		/// Get the Slide Position.
		/// </summary>
		public int SlidePosition { get => Comm.Slide.Position; }

		/// <summary>
		/// Pass Velocity through to Slide, changing direction based on Motor Location setting.
		/// </summary>
		public double SlideVelocity { set => Comm.Slide.Velocity = Comm.Settings.MotorLocation ? -value : value; }

		/// <summary>
		/// Get the Pan Position.
		/// </summary>
		public int PanPosition { get => Comm.Pan.Position; }

		/// <summary>
		/// Pass Velocity through to Pan, changing direction so CCW is positive.
		/// </summary>
		public double PanVelocity { set => Comm.Pan.Velocity = -value; }

		/// <summary>
		/// Enable some UI elements only if Connected and Calibrated
		/// </summary>
		public bool Enabled { get => Comm.Slide.Calibrated && Comm.State == BlueState.Connected; }

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
