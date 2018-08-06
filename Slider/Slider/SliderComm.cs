using System;
using System.Collections.Generic;
using System.Text;

namespace CamSlider
{
	public class SliderComm
	{
		BlueApp Blue;

		public event EventHandler StateChange;

		protected SliderComm()
		{
			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;
		}

		private static SliderComm _Instance;
		public static SliderComm Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new SliderComm();
				return _Instance;
			}
		}

		public void Connect(string name) => Blue.Connect(name);
		public void Disconnect() => Blue.Disconnect();
		public BlueState State { get => Blue.State; }
		public bool CanConnect { get => Blue.CanConnect; }
		public string ErrorMessage { get => Blue.ErrorMessage; }

		public string StateText
		{
			get
			{
				string state = Blue.State.ToString();
				if (!string.IsNullOrWhiteSpace(ErrorMessage))
					state += " - " + ErrorMessage;
				return state;
			}
		}

		public void SetSlideVector(double speed)
		{
			if (SliderComm.Instance.State != BlueState.Connected)
				return;
			speed = Math.Round(speed, 1);
			Blue.Write($"sv{speed:0.#};", Math.Abs(speed) < 0.01);
		}

		public void SetPanVector(double speed)
		{
			if (SliderComm.Instance.State != BlueState.Connected)
				return;
			speed = Math.Round(speed, 1);
			Blue.Write($"pv{speed:0.#};", Math.Abs(speed) < 0.01);
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			StateChange?.Invoke(this, e);
		}
	}
}
