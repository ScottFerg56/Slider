using System;

using Slider.Models;

namespace Slider.ViewModels
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
