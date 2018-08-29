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
		public Command PanZeroCommand { get; set; }

		public ManualViewModel()
		{
			PanZeroCommand = new Command(() => { SliderComm.Instance.Pan.Zero(); });
			SliderComm.Instance.Slide.PropertyChanged += Slide_PropertyChanged;
			SliderComm.Instance.Pan.PropertyChanged += Pan_PropertyChanged; ;
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

		public int SlidePosition { get => (int)Math.Round(SliderComm.Instance.Slide.Position); }

		public double SlideVector { set => SliderComm.Instance.Slide.Vector = -value; }

		public int PanPosition { get => (int)Math.Round(SliderComm.Instance.Pan.Position); }

		public double PanVector { set => SliderComm.Instance.Pan.Vector = -value; }

		public bool Enabled { get => SliderComm.Instance.Slide.Homed; }

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
