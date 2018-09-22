using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace CamSlider.Services
{
	/// <summary>
	/// Uses the Xamarin.Plugins.Settings to store persist settings on all platforms as Json strings.
	/// </summary>
	/// <remarks>
	/// ANDROID ISSUE:
	/// On Android certain changes to the program, i.e. during development, are major enough to cause some kind of more thorough
	/// install on the Android device. This causes the settings to be lost and it seems nothing can be done about it.
	/// This is likely only a nuisance during development.
	/// https://www.nuget.org/packages/Xam.Plugins.Settings
	/// </remarks>
	public static class DataStore
	{
		private static Plugin.Settings.Abstractions.ISettings AppSettings =>
			Plugin.Settings.CrossSettings.Current;

		/// <summary>
		/// Load an object from the data store.
		/// </summary>
		/// <typeparam name="T">The type of the object to load.</typeparam>
		/// <param name="name">The name given to the object in the data store.</param>
		/// <returns>
		/// An object of the specified type constructed with the values loaded from the named data store
		/// or the default value for the type if the store does not exist or fails to load.
		/// </returns>
		public static T LoadDataStore<T>(string name)
		{
			// get the Json string
			var json = AppSettings.GetValueOrDefault(name, string.Empty);
			if (!string.IsNullOrWhiteSpace(json))
			{
				try
				{
					// deserialize and return the constructed object
					return JsonConvert.DeserializeObject<T>(json);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					// deserialization failed, return the default value
					return default(T);
				}
			}
			// nothing in the data store, return the default value
			return default(T);
		}

		/// <summary>
		/// Save an object into the data store.
		/// </summary>
		/// <typeparam name="T">The type of the object to be stored.</typeparam>
		/// <param name="name">The name given to the object in the data store.</param>
		/// <param name="data">The object to be stored.</param>
		public static void SaveDataStore<T>(string name, T data)
		{
			// serialize to Json and put in storage.
			var json = JsonConvert.SerializeObject(data);
			AppSettings.AddOrUpdateValue(name, json);
		}
	}
}