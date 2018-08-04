using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Slider.Models;

namespace Slider.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage(Item newItem = null)
        {
            InitializeComponent();

			Item = newItem;
			if (Item == null)
			{
				Item = new Item
				{
					Time = 600.0,
					Slide = 600.0,
					Pan = 0.0,
				};
			}

            BindingContext = this;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }
    }
}