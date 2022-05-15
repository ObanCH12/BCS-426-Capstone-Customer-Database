using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CustomerDatabaseTutorial.App.ViewModels
{
    public class CustomerListPageViewModel : INotifyPropertyChanged
    {
        public CustomerListPageViewModel()
        {
            Task.Run(GetCustomerListAsync);
        }

        private ObservableCollection<CustomerViewModel> _customers = new ObservableCollection<CustomerViewModel>();

        public ObservableCollection<CustomerViewModel> Customers { get => _customers; }


        private CustomerViewModel _selectedCustomer;

        private CustomerViewModel _newCustomer;

        private bool _addingNewCustomer = false;
        public bool EnableCommandBar => !AddingNewCustomer;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CustomerViewModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged();
                }
            }
        }

        public CustomerViewModel NewCustomer
        {
            get => _newCustomer;
            set
            {
                if (_newCustomer != value)
                {
                    _newCustomer = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool AddingNewCustomer
        {
            get => _addingNewCustomer;
            set
            {
                if (_addingNewCustomer != value)
                {
                    _addingNewCustomer = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged(nameof(EnableCommandBar));
            }
        }

        public async Task CreateNewCustomerAsync()
        {
            CustomerViewModel newCustomer = new CustomerViewModel(new Models.Customer());
            NewCustomer = newCustomer;
            await App.Repository.Customers.UpsertAsync(NewCustomer.Model);
            AddingNewCustomer = true;
        }

        public async Task ReadFile()
        {
            CustomerViewModel newCustomer = new CustomerViewModel(new Models.Customer());
            NewCustomer = newCustomer;
            await App.Repository.Customers.UpsertAsync(NewCustomer.Model);
            AddingNewCustomer = true;
        }


        public async Task DeleteNewCustomerAsync()
        {
            if (NewCustomer != null)
            {
                await App.Repository.Customers.DeleteAsync(_newCustomer.Model.Id);
                AddingNewCustomer = false;
            }
        }

        public async void DeleteAndUpdateAsync()
        {
            if (SelectedCustomer != null)
            {
                await App.Repository.Customers.DeleteAsync(_selectedCustomer.Model.Id);
            }
            await UpdateCustomersAsync();
        }

        public async Task GetCustomerListAsync()
        {
            var customers = await App.Repository.Customers.GetAsync();
            if (customers == null)
            {
                return;
            }
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Customers.Clear();
                foreach (var c in customers)
                {
                    Customers.Add(new CustomerViewModel(c));
                }
            });
        }

        public async Task SaveInitialChangesAsync()
        {
            await App.Repository.Customers.UpsertAsync(NewCustomer.Model);
            await UpdateCustomersAsync();
            AddingNewCustomer = false;
        }
        public async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            //OutputTextBlock.Text = "";

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // The StorageFile has read/write access to the picked file.
                // See the FileAccess sample for code that uses a StorageFile to read and write.
               // OutputTextBlock.Text = "Picked photo: " + file.Name;
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }
        }

        public async Task UpdateCustomersAsync()
        {
            foreach (var modifiedCustomer in Customers
                .Where(x => x.IsModified).Select(x => x.Model))
            {
                await App.Repository.Customers.UpsertAsync(modifiedCustomer);
            }
            await GetCustomerListAsync();
        }
    }
}
