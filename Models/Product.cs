using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace WoodworkManagementApp.Models
{
    public class Product : INotifyPropertyChanged
    {
        private string _name;
        private decimal _pricePerM3;
        private int _discount;
        private string _category;
        private int _order;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public decimal PricePerM3
        {
            get => _pricePerM3;
            set
            {
                if (value < 0) throw new ArgumentException("Price cannot be negative");
                _pricePerM3 = value;
                OnPropertyChanged();
            }
        }

        public int Discount
        {
            get => _discount;
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentException("Discount must be between 0 and 100");
                _discount = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}