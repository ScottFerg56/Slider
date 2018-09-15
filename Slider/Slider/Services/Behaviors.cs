using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.Services
{
	public class NumericValidationBehavior : Behavior<Entry>
	{
		public bool Unsigned { get; set; } = true;

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
			var beh = ((Entry)sender).Behaviors.OfType<NumericValidationBehavior>().First();
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
				if (beh.Unsigned)
					neg = pos = false;
				bool isValid = int.TryParse(s, out int v);

				if (!isValid)
				{
					s = new string(s.Where(c => char.IsDigit(c)).ToArray());
					int.TryParse(s, out v);
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
