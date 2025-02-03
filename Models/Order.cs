using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WoodworkManagementApp.Models
{
    public class Order : INotifyPropertyChanged
    {
        private string _orderNumber;
        private DateTime _creationDate;
        private string _creatorName;
        private ObservableCollection<OrderProduct> _products;
        private string _receiverName;
        private string _completionDate;
        private string _comments;
        private bool _isLocked;
        private string _lockedBy;
        private string _thumbnailPath;

        public string OrderNumber
        {
            get => _orderNumber;
            set
            {
                _orderNumber = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged();
            }
        }

        public string CreatorName
        {
            get => _creatorName;
            set
            {
                _creatorName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OrderProduct> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public string ReceiverName
        {
            get => _receiverName;
            set
            {
                _receiverName = value;
                OnPropertyChanged();
            }
        }

        public string CompletionDate
        {
            get => _completionDate;
            set
            {
                _completionDate = value;
                OnPropertyChanged();
            }
        }

        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged();
            }
        }

        public string LockedBy
        {
            get => _lockedBy;
            set
            {
                _lockedBy = value;
                OnPropertyChanged();
            }
        }

        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set
            {
                _thumbnailPath = value;
                OnPropertyChanged();
            }
        }

        public Order()
        {
            Products = new ObservableCollection<OrderProduct>();
            CreationDate = DateTime.Now;
            IsLocked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Order Clone()
        {
            var clone = new Order
            {
                OrderNumber = OrderNumber,
                CreationDate = CreationDate,
                CreatorName = CreatorName,
                ReceiverName = ReceiverName,
                CompletionDate = CompletionDate,
                Comments = Comments,
                IsLocked = IsLocked,
                LockedBy = LockedBy,
                ThumbnailPath = ThumbnailPath
            };

            foreach (var product in Products)
            {
                clone.Products.Add(product.Clone());
            }

            return clone;
        }
    }
}