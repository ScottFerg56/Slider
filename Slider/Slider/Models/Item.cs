using System;
using System.Diagnostics;

namespace Slider.Models
{
//	[DebuggerDisplay("Time {Time}, Slide {Slide}, Pan {Pan}")]
	public class Item
    {
		public double Time { get; set; }
		public double Slide { get; set; }
		public double Pan { get; set; }
		//	public double Acceleration { get; set; }

		public override string ToString()
		{
			return $"Time: {Time}, Slide: {Slide}, Pan: {Pan}";
		}
	}
}