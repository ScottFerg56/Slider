using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CamSlider.Models;

namespace CamSlider.Services
{
	public static class DataStore
	{
		public static T LoadDataStore<T>(string name)
		{
			if (App.Current.Properties.TryGetValue(name, out object js) && js is string json && !string.IsNullOrWhiteSpace(json))
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(json);
				}
				catch (Exception)
				{
					return default(T);
				}
			}
			return default(T);
		}

		public static void SaveDataStore<T>(string name, T data)
		{
			var json = JsonConvert.SerializeObject(data);
			App.Current.Properties[name] = json;
		}
	}
}