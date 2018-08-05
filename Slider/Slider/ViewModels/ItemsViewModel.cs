using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CamSlider.Models;
using CamSlider.Views;
using System.Linq;
using System.Collections.Generic;

namespace CamSlider.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command( () => ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", (obj, item) =>
            {
				//
				// !!ANDROID BUG HACK!!
				// On UWP we can just rebuild the ObservableCollection from the stored list.
				// On Android setting the Items property bound to the ListView doesn't rebuild the view list properly.
				// So here we mirror the changes to the internal list with simple Remove/Add/Move to keep
				// the list sorted. Then update the stored list.
				// Also tried Remove and Insert for an edit, but Insert apparently doesn't trigger properly either.
				//

				// find the item if this is an EDIT; or not if it's an ADD
				// find current index
				// remove, if an edit and it already is in the list
				Items.Remove(item);
				// always add back in because a no-op move won't trigger a change
				Items.Add(item);
				// sort to a companion list
				var list = Items.OrderBy(_ => _.Time).ToList();
				// find the new index
				var newIndex = list.IndexOf(item);
				// move the item to keep the list sorted
				Items.Move(Items.Count - 1, newIndex);
				// update the data store
				Services.DataStore.SaveDataStore("items", Items);
            });

			MessagingCenter.Subscribe<NewItemPage, Item>(this, "RemoveItem", (obj, item) =>
			{
				Items.Remove(item);
				Services.DataStore.SaveDataStore("items", Items);
			});
		}

		void ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
				var list = Services.DataStore.LoadDataStore<List<Item>>("items");
				if (list == null || list.Count == 0)
				{
					// fabricate an initial list of items on first run
					list = new List<Item>
					{
						new Item { Time = 0.0, Slide = 0.0, Pan = 0.0 },
						new Item { Time = 10.0, Slide = 200.0, Pan = -90.0 },
					};
				}
				Items = new ObservableCollection<Item>(Services.DataStore.LoadDataStore<List<Item>>("items"));
			}
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
				OnPropertyChanged("Items");
            }
        }
    }
}