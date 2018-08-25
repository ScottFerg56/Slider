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
		protected Sequence Seq;
		public Command MoveToInCommand { get; set; }
		public Command MoveToOutCommand { get; set; }
		public Command SetInFromCurrentCommand { get; set; }
		public Command SetOutFromCurrentCommand { get; set; }

		public int SlideIn
		{
			get { return GetProperty<int>(); }
			set { SetProperty(value); }
		}

		public int PanIn
		{
			get { return GetProperty<int>(); }
			set { SetProperty(value); }
		}

		public int SlideOut
		{
			get { return GetProperty<int>(); }
			set { SetProperty(value); }
		}

		public int PanOut
		{
			get { return GetProperty<int>(); }
			set { SetProperty(value); }
		}

		public SequenceViewModel()
		{
			MoveToInCommand = new Command(() => ExecuteMoveToInCommand());
			MoveToOutCommand = new Command(() => ExecuteMoveToOutCommand());
			SetInFromCurrentCommand = new Command(() => ExecuteSetInFromCurrentCommand());
			SetOutFromCurrentCommand = new Command(() => ExecuteSetOutFromCurrentCommand());
			Seq = Services.DataStore.LoadDataStore<Sequence>("sequence");
			if (Seq == null)
			{
				Seq = new Sequence();
			}
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
			OnPropertyChanged(propertyName);
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
