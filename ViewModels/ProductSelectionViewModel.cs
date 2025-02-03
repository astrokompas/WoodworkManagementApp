using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WoodworkManagementApp.Models;

namespace WoodworkManagementApp.ViewModels
{
    public class ProductSelectionViewModel : INotifyPropertyChanged
    {
        private readonly Product _product;
        private bool _isSelected;

        public ProductSelectionViewModel(Product product)
        {
            _product = product;
        }

        public Product Product => _product;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Name => _product.Name;
        public decimal PricePerM3 => _product.PricePerM3;
        public int Discount => _product.Discount;
        public string Category => _product.Category;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
