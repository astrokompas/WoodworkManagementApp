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
            if (Product == null) return;

            decimal basePrice = Volume * Product.PricePerM3;
            if (Volume >= Product.Discount && Discount > 0)
            {
                decimal discountMultiplier = 1 - (Discount / 100);
                TotalPrice = basePrice * discountMultiplier;
            }
            else
            {
                TotalPrice = basePrice;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
