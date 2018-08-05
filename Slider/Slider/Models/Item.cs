using System;
using System.Diagnostics;

namespace CamSlider.Models
{
//	[DebuggerDisplay("Time {Time}, Slide {Slide}, Pan {Pan}")]
	public class Item
    {
		public double Time { get; set; }
		public double Slide { get; set; }
		public double Pan { get; set; }
		//	public double Acceleration { get; set; }

		public static Item DefaultItem()
		{
			return new Item
			{
				Time = 600.0,
				Slide = 600.0,
				Pan = 0.0,
			};

		}

		public override string ToString()
		{
			return $"Time: {Time}, Slide: {Slide}, Pan: {Pan}";
		}
	}
}