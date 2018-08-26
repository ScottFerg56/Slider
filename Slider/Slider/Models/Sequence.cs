using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CamSlider.Models
{
    public class Sequence
    {
		protected int _SlideIn = 0;
		public int SlideIn
		{
			get { return _SlideIn; }
			set
			{
				if (value == _SlideIn)
					return;
				_SlideIn = Stepper.LimitSlideValue(value);
			}
		}

		protected int _SlideOut = 0;
		public int SlideOut
		{
			get { return _SlideOut; }
			set
			{
				if (value == _SlideOut)
					return;
				_SlideOut = Stepper.LimitSlideValue(value);
			}
		}

		protected int _PanIn = 0;
		public int PanIn
		{
			get { return _PanIn; }
			set
			{
				if (value == _PanIn)
					return;
				_PanIn = Stepper.LimitPanValue(value);
			}
		}

		protected int _PanOut = 0;
		public int PanOut
		{
			get { return _PanOut; }
			set
			{
				if (value == _PanOut)
					return;
				_PanOut = Stepper.LimitPanValue(value);
			}
		}

		protected int _Duration = 0;
		public int Duration
		{
			get { return _Duration; }
			set
			{
				if (value == _Duration)
					return;
				_Duration = Math.Max(0, value);
			}
		}

		protected int _Playback = 0;
		public int Playback
		{
			get { return _Playback; }
			set
			{
				if (value == _Playback)
					return;
				_Playback = Math.Max(0, value);
			}
		}
	}
}
