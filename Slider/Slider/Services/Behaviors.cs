using System.Linq;
using Xamarin.Forms;

namespace CamSlider.Services
{
	/// <summary>
	/// A behavior for Xamarin Entry controls to restrict entry to valid integer.
	/// </summary>
	public class NumericValidationBehavior : Behavior<Entry>
	{
		/// <summary>
		/// Get/set whether only unsigned value should be allowed.
		/// </summary>
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

		/// <summary>
		/// Process the string in the Entry control to make sure it represents a valid integer.
		/// </summary>
		/// <remarks>
		/// The interesting side effects of this behavior are:
		///		Leading zeroes are trimmed
		///		'+' sign is trimmed
		///		'-' sign can be inserted anywhere and will move to the front
		///		'+' sign can be inserted anywhere and will cancel a '-' sign (and then be trimmed)
		/// </remarks>
		private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
		{
			var s = args.NewTextValue;
			if (!string.IsNullOrWhiteSpace(s))
			{
				// strip out signs, remembering what we found
				bool neg = s.Contains("-");
				bool pos = s.Contains("+");
				if (neg || pos)
				{
					s = s.Replace("+", "");
					s = s.Replace("-", "");
				}
				// for unsigned, just forget the signs
				if (Unsigned)
					neg = pos = false;
				// try to parse as an integer
				bool isValid = int.TryParse(s, out int v);

				if (!isValid)
				{
					// invalid - remove anything not a digit
					s = new string(s.Where(c => char.IsDigit(c)).ToArray());
					// parse again
					int.TryParse(s, out v);
				}
				// recreate the numeric string
				s = v.ToString();
				// put back the sign, as required
				if (neg && !pos)
					s = "-" + s;
			}
			else
			{
				// no text, so make it 0
				s = "0";
			}
			// reset the text
			((Entry)sender).Text = s;
		}
	}
}
