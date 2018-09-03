using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.ViewModels
{
    public class ManualViewModel : INotifyPropertyChanged
	{
		protected SliderComm Comm { get => SliderComm.Instance; }
		public Command PanZeroCommand { get; set; }

		public ManualViewModel()
		{
			PanZeroCommand = new Command(() => { Comm.Pan.Zero(); });
			Comm.Slide.PropertyChanged += Slide_PropertyChanged;
			Comm.Pan.PropertyChanged += Pan_PropertyChanged; ;
			Comm.StateChange += Comm_StateChange;
		}

		private void Comm_StateChange(object sender, EventArgs e)
		{
			OnPropertyChanged("Enabled");
		}

		private void Pan_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("PanPosition");
		}

		private void Slide_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				OnPropertyChanged("SlidePosition");
			else if (e.PropertyName == "Homed")
				OnPropertyChanged("Enabled");
		}

		public int SlidePosition { get => Comm.Slide.Position; }

		public double SlideVector { set => Comm.Slide.Vector = -value; }

		public int PanPosition { get => Comm.Pan.Position; }

		public double PanVector { set => Comm.Pan.Vector = -value; }

		public bool Enabled { get => Comm.Slide.Homed && Comm.State == BlueState.Connected; }

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
