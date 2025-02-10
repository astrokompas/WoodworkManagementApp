using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Models
{
    public class OrderProduct : INotifyPropertyChanged
    {
        private Product _product;
        private decimal _volume;
        private int _pieces;
        private decimal _discount;
        private decimal _totalPrice;
        private readonly Dictionary<string, string> _errors = new();

        private void ValidateVolume(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Volume cannot be negative");
        }

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                CalculateTotalPrice();
                OnPropertyChanged();
            }
        }

        public decimal Volume
        {
            get => _volume;
            set
            {
                ValidateVolume(value);
                _volume = value;
                CalculateTotalPrice();
                OnPropertyChanged();
            }
        }

        public int Pieces
        {
            get => _pieces;
            set
            {
                _pieces = value;
                OnPropertyChanged();
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                CalculateTotalPrice();
                OnPropertyChanged();
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            private set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        private void CalculateTotalPrice()
        {
            if (Product == null)
            {
                TotalPrice = 0;
                return;
            }

            decimal basePrice = Volume * Product.PricePerM3;
            TotalPrice = Volume >= Product.Discount && Discount > 0
                ? basePrice * (1 - (Discount / 100))
                : basePrice;
        }

        public OrderProduct Clone()
        {
            return new OrderProduct
            {
                Product = Product,
                Volume = Volume,
                Pieces = Pieces,
                Discount = Discount
            };
        }

        public string Error => string.Join(Environment.NewLine, _errors.Values);

        public string this[string propertyName]
        {
            get
            {
                _errors.TryGetValue(propertyName, out var error);
                return error ?? string.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
