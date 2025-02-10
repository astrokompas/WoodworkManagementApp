using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private readonly Dictionary<string, string> _errors = new();

        public ObservableCollection<OrderProduct> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

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
                ValidateCompletionDate();
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

        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle old items
            if (e.OldItems != null)
            {
                foreach (OrderProduct item in e.OldItems)
                {
                    item.PropertyChanged -= Product_PropertyChanged;
                }
            }

            // Handle new items
            if (e.NewItems != null)
            {
                foreach (OrderProduct item in e.NewItems)
                {
                    item.PropertyChanged += Product_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(TotalAmount));
        }

        private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderProduct.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public decimal TotalAmount => Products?.Sum(p => p.TotalPrice) ?? 0;

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
                ValidateCompletionDate();
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
            _products = new ObservableCollection<OrderProduct>();
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
                CreatorName = CreatorName?.Clone() as string,
                ReceiverName = ReceiverName?.Clone() as string,
                CompletionDate = CompletionDate?.Clone() as string,
                Comments = Comments?.Clone() as string,
                IsLocked = IsLocked,
                LockedBy = LockedBy?.Clone() as string,
                ThumbnailPath = ThumbnailPath?.Clone() as string
            };

            if (Products != null)
            {
                clone.Products = new ObservableCollection<OrderProduct>(
                    Products.Select(p => p?.Clone())
                           .Where(p => p != null));
            }

            return clone;
        }

        private void ValidateCompletionDate()
        {
            if (string.IsNullOrWhiteSpace(_completionDate))
            {
                _errors[nameof(CompletionDate)] = "Completion date is required";
                return;
            }

            if (DateTime.TryParse(_completionDate, out DateTime completionDate))
            {
                if (completionDate.Date < CreationDate.Date)
                {
                    _errors[nameof(CompletionDate)] =
                        "Completion date cannot be earlier than creation date";
                }
                else
                {
                    _errors.Remove(nameof(CompletionDate));
                }
            }
            else
            {
                _errors[nameof(CompletionDate)] = "Invalid date format";
            }
        }
    }
}