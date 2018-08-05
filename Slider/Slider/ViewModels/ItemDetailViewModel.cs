using System;

using CamSlider.Models;

namespace CamSlider.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.ToString();
            Item = item;
        }
    }
}
