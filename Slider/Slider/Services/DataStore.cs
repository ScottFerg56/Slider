using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CamSlider.Services
{
	public static class DataStore
	{
		private static Plugin.Settings.Abstractions.ISettings AppSettings =>
			Plugin.Settings.CrossSettings.Current;

		public static T LoadDataStore<T>(string name)
		{
			var json = AppSettings.GetValueOrDefault(name, string.Empty);
			if (!string.IsNullOrWhiteSpace(json))
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(json);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return default(T);
				}
			}
			return default(T);
		}

		public static void SaveDataStore<T>(string name, T data)
		{
			var json = JsonConvert.SerializeObject(data);
			AppSettings.AddOrUpdateValue(name, json);
		}
	}
}