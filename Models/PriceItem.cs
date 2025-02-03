using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WoodworkManagementApp.Models
{
    public class PriceItem : INotifyPropertyChanged
    {
        private Product _product;
        private decimal? _volume;
        private int? _pieces;
        private decimal? _discount;
        private decimal _totalPrice;
        private decimal? _pricePerPiece;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                CalculatePrices();
                OnPropertyChanged();
            }
        }

        public decimal? Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                CalculatePrices();
                OnPropertyChanged();
            }
        }

        public int? Pieces
        {
            get => _pieces;
            set
            {
                _pieces = value;
                CalculatePrices();
                OnPropertyChanged();
            }
        }

        public decimal? Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                CalculatePrices();
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

        public decimal? PricePerPiece
        {
            get => _pricePerPiece;
            private set
            {
                _pricePerPiece = value;
                OnPropertyChanged();
            }
        }

        private void CalculatePrices()
        {
            if (Product == null || !Volume.HasValue) return;

            decimal basePrice = Volume.Value * Product.PricePerM3;

            if (Volume >= Product.Discount && Discount.HasValue)
            {
                decimal discountMultiplier = 1 - (Discount.Value / 100);
                TotalPrice = basePrice * discountMultiplier;
            }
            else
            {
                TotalPrice = basePrice;
            }

            PricePerPiece = Pieces.HasValue && Pieces.Value > 0
                ? TotalPrice / Pieces.Value
                : null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}