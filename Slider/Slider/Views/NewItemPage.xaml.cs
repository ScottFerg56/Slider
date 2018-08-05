using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CamSlider.Models;

namespace CamSlider.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage(Item newItem = null)
        {
            InitializeComponent();

			Item = newItem ?? Item.DefaultItem();

            BindingContext = this;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }

		async void Remove_Clicked(object sender, EventArgs e)
		{
			MessagingCenter.Send(this, "RemoveItem", Item);
			await Navigation.PopModalAsync();
		}
	}
}