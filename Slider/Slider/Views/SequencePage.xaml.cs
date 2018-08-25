using CamSlider.Models;
using CamSlider.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SequencePage : ContentPage
	{
		SequenceViewModel SeqViewModel;

		public SequencePage ()
		{
			InitializeComponent ();

			BindingContext = SeqViewModel = new SequenceViewModel();
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