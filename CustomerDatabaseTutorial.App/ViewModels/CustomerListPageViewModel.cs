﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using CustomerDatabaseTutorial.App.UserControls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.Smo;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Net.Http.Headers;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using System.IO;

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

        
        public bool AddingNewLogin { get; private set; }

        public async Task CreateNewCustomerAsync()
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

        public async Task UpdateCustomersAsync()
        {
            foreach (var modifiedCustomer in Customers
                .Where(x => x.IsModified).Select(x => x.Model))
            {
                await App.Repository.Customers.UpsertAsync(modifiedCustomer);
            }
            await GetCustomerListAsync();
        }

        public async void LoginClick()
        {
            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                await LoginAsync();
            }
            else
            {
                Windows.UI.ApplicationSettings.AccountsSettingsPane.Show();
            }
        }

        

        private string _name;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private void Set(ref string name, string value)
        {
            throw new NotImplementedException();
        }

        private string _email;

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        private string _title;

        /// <summary>
        /// Gets or sets the user's standard title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _domain;

        /// <summary>
        /// Gets or sets the user's AAD domain.
        /// </summary>
        public string Domain
        {
            get => _domain;
            set => Set(ref _domain, value);
        }

        private BitmapImage _photo;

        /// <summary>
        /// Gets or sets the user's photo.
        /// </summary>
        public BitmapImage Photo
        {
            get => _photo;
            set => Set(ref _photo, value);
        }

        private void Set(ref BitmapImage photo, BitmapImage value)
        {
            throw new NotImplementedException();
        }

        private string _errorText;

        /// <summary>
        /// Gets or sets error text to show if the login operation fails.
        /// </summary>
        public string ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private bool _showWelcome;

        /// <summary>
        /// Gets or sets whether to show the starting welcome UI. 
        /// </summary>
        public bool ShowWelcome
        {
            get => _showWelcome;
            set => Set(ref _showWelcome, value);
        }

        private void Set(ref bool showWelcome, bool value)
        {
            throw new NotImplementedException();
        }

        private bool _showLoading;

        /// <summary>
        /// Gets or sets whether to show the logging in progress UI.
        /// </summary>
        public bool ShowLoading
        {
            get => _showLoading;
            set => Set(ref _showLoading, value);
        }

        private bool _showData;

        /// <summary>
        /// Gets or sets whether to show user data UI.
        /// </summary>
        public bool ShowData
        {
            get => _showData;
            set => Set(ref _showData, value);
        }

        private bool _showError;

        /// <summary>
        /// Gets or sets whether to show the error UI.
        /// </summary>
        public bool ShowError
        {
            get => _showError;
            set => Set(ref _showError, value);
        }


        public async Task PrepareAsync()
        {
            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                await SetVisibleAsync(vm => vm.ShowLoading);
                await LoginAsync();
            }
            else
            {
                await SetVisibleAsync(vm => vm.ShowWelcome);
            }
        }

        /// <summary>
        /// Logs the user in by requesting a token and using it to query the 
        /// Microsoft Graph API.
        /// </summary>
        public async Task LoginAsync()
        {
            try
            {
                await SetVisibleAsync(vm => vm.ShowLoading);
                string token = await GetTokenAsync();
                if (token != null)
                {
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"] = true;
                    await SetUserInfoAsync(token);
                    await SetUserPhoto(token);
                    await SetVisibleAsync(vm => vm.ShowData);
                }
                else
                {
                    await SetVisibleAsync(vm => vm.ShowError);
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                await SetVisibleAsync(vm => vm.ShowError);
            }
        }

        /// <summary>
        /// Gets an auth token for the user, which can be used to call the Microsoft Graph API.
        /// </summary>
        private async Task<string> GetTokenAsync()
        {
            var provider = await GetAadProviderAsync();
            var request = new WebTokenRequest(provider, "User.Read",
                Repository.Constants.AccountClientId);
            request.Properties.Add("resource", "https://graph.microsoft.com");
            var result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request);
            if (result.ResponseStatus != WebTokenRequestStatus.Success)
            {
                result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            }
            return result.ResponseStatus == Windows.Security.Authentication.Web.Core.WebTokenRequestStatus.Success ?
                result.ResponseData[0].Token : null;
        }

        /// <summary>
        /// Gets and processes the user's info from the Microsoft Graph API.
        /// </summary>
        private async Task SetUserInfoAsync(string token)
        {
            var users = await Windows.System.User.FindAllAsync();
            var graph = new Microsoft.Graph.GraphServiceClient(new Microsoft.Graph.DelegateAuthenticationProvider(message =>
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return Task.CompletedTask;
            }));

            var me = await graph.Me.Request().GetAsync();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    Name = me.DisplayName;
                    Email = me.Mail;
                    Title = me.JobTitle;
                    Domain = (string)await users[0].GetPropertyAsync(Windows.System.KnownUserProperties.DomainName);
                });
        }

        private async Task SetUserPhoto(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = "https://graph.microsoft.com/beta/me/photo/$value";
                var result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    return;
                }
                using (Stream stream = await result.Content.ReadAsStreamAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                Photo = new BitmapImage();
                                await Photo.SetSourceAsync(memoryStream.AsRandomAccessStream());
                            });
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the AccountsSettingsPane with AAD login.
        /// </summary>
        private async void BuildAccountsPaneAsync(Windows.UI.ApplicationSettings.AccountsSettingsPane sender,
            Windows.UI.ApplicationSettings.AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var command = new Windows.UI.ApplicationSettings.WebAccountProviderCommand(await GetAadProviderAsync(), async (x) =>
                await LoginAsync());
            args.WebAccountProviderCommands.Add(command);
            deferral.Complete();
        }

        /// <summary>
        /// Gets the Microsoft ADD login provider.
        /// </summary>
        public async Task<Windows.Security.Credentials.WebAccountProvider> GetAadProviderAsync() =>
            await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "organizations");


        
        

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async void LogoutClick()
        {
            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("IsLoggedIn") &&
                (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["IsLoggedIn"])
            {
                ContentDialog SignoutDialog = new ContentDialog()
                {
                    Title = "Sign out",
                    Content = "Sign out?",
                    PrimaryButtonText = "Sign out",
                    SecondaryButtonText = "Cancel"

                };
                await SignoutDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Shows one part of the login UI sequence and hides all the others.
        /// </summary>
        private async Task SetVisibleAsync(System.Linq.Expressions.Expression<Func<AuthenticationViewModel, bool>> selector)
        {
            var prop = (System.Reflection.PropertyInfo)((System.Linq.Expressions.MemberExpression)selector.Body).Member;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ShowWelcome = false;
                ShowLoading = false;
                ShowData = false;
                ShowError = false;
                prop.SetValue(this, true);
            });
        }
    }


    

        
    }
    
