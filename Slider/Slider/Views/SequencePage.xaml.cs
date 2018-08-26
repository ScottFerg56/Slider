using CamSlider.Models;
using CamSlider.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SequencePage : ContentPage
	{
		SequenceViewModel SeqViewModel;
		Timer timer;

		public SequencePage ()
		{
			InitializeComponent ();

			BindingContext = SeqViewModel = new SequenceViewModel();
			ButtonMinsUp.Pressed += ButtonMinsUp_Pressed;
			ButtonMinsUp.Released += Button_Released;
			ButtonMinsDn.Pressed += ButtonMinsDn_Pressed;
			ButtonMinsDn.Released += Button_Released;
			ButtonSecsUp.Pressed += ButtonSecsUp_Pressed;
			ButtonSecsUp.Released += Button_Released;
			ButtonSecsDn.Pressed += ButtonSecsDn_Pressed;
			ButtonSecsDn.Released += Button_Released;

			ButtonPMinsUp.Pressed += ButtonPMinsUp_Pressed;
			ButtonPMinsUp.Released += Button_Released;
			ButtonPMinsDn.Pressed += ButtonPMinsDn_Pressed;
			ButtonPMinsDn.Released += Button_Released;
			ButtonPSecsUp.Pressed += ButtonPSecsUp_Pressed;
			ButtonPSecsUp.Released += Button_Released;
			ButtonPSecsDn.Pressed += ButtonPSecsDn_Pressed;
			ButtonPSecsDn.Released += Button_Released;

			timer = new Timer
			{
				Enabled = false,
				Interval = 500
			};
			timer.Elapsed += Timer_Elapsed;
		}

		enum TimerAction { None, MinsUp, MinsDn, SecsUp, SecsDn, PMinsUp, PMinsDn, PSecsUp, PSecsDn }

		TimerAction Action = TimerAction.None;

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Interval = 100;
			switch (Action)
			{
				case TimerAction.None:
					timer.Enabled = false;
					timer.Interval = 500;
					break;
				case TimerAction.MinsUp:
					SeqViewModel.DurationMins++;
					break;
				case TimerAction.MinsDn:
					SeqViewModel.DurationMins--;
					break;
				case TimerAction.SecsUp:
					SeqViewModel.DurationSecs++;
					break;
				case TimerAction.SecsDn:
					SeqViewModel.DurationSecs--;
					break;
				case TimerAction.PMinsUp:
					SeqViewModel.PlaybackMins++;
					break;
				case TimerAction.PMinsDn:
					SeqViewModel.PlaybackMins--;
					break;
				case TimerAction.PSecsUp:
					SeqViewModel.PlaybackSecs++;
					break;
				case TimerAction.PSecsDn:
					SeqViewModel.PlaybackSecs--;
					break;
			}
		}

		private void Button_Released(object sender, EventArgs e)
		{
			Action = TimerAction.None;
		}

		private void ButtonMinsUp_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.DurationMins++;
			Action = TimerAction.MinsUp;
			timer.Enabled = true;
		}

		private void ButtonMinsDn_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.DurationMins--;
			Action = TimerAction.MinsDn;
			timer.Enabled = true;
		}

		private void ButtonSecsUp_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.DurationSecs++;
			Action = TimerAction.SecsUp;
			timer.Enabled = true;
		}

		private void ButtonSecsDn_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.DurationSecs--;
			Action = TimerAction.SecsDn;
			timer.Enabled = true;
		}

		private void ButtonPMinsUp_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.PlaybackMins++;
			Action = TimerAction.PMinsUp;
			timer.Enabled = true;
		}

		private void ButtonPMinsDn_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.PlaybackMins--;
			Action = TimerAction.PMinsDn;
			timer.Enabled = true;
		}

		private void ButtonPSecsUp_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.PlaybackSecs++;
			Action = TimerAction.PSecsUp;
			timer.Enabled = true;
		}

		private void ButtonPSecsDn_Pressed(object sender, EventArgs e)
		{
			SeqViewModel.PlaybackSecs--;
			Action = TimerAction.PSecsDn;
			timer.Enabled = true;
		}
	}

	public class NumericValidationBehavior : Behavior<Entry>
	{

		protected override void OnAttachedTo(Entry entry)
		{
			entry.TextChanged += OnEntryTextChanged;
			base.OnAttachedTo(entry);
		}

		protected override void OnDetachingFrom(Entry entry)
		{
			entry.TextChanged -= OnEntryTextChanged;
			base.OnDetachingFrom(entry);
		}

		private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
		{
			var s = args.NewTextValue;
			if (!string.IsNullOrWhiteSpace(s))
			{
				bool neg = s.Contains("-");
				bool pos = s.Contains("+");
				if (neg || pos)
				{
					s = s.Replace("+", "");
					s = s.Replace("-", "");
				}
				bool isValid = int.TryParse(s, out int v);

				if (!isValid)
				{
					s = new string(s.Where(c => char.IsDigit(c)).ToArray());
					int.TryParse(s, out v);
				//	System.Diagnostics.Debug.WriteLine("fix numeric: " + s);
				}
				s = v.ToString();
				if (neg && !pos)
					s = "-" + s;
			}
			else
			{
				s = "0";
			}
			((Entry)sender).Text = s;
		}


	}
}