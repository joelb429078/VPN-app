using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using VPNproject.Windows;

namespace VPNproject.ViewModel
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServerModel> Servers { get; set; }

        private ObservableCollection<ServerModel> _filteredServers;
        public ObservableCollection<ServerModel> FilteredServers
        {
            get { return _filteredServers; }
            set
            {
                _filteredServers = value;
                OnPropertyChanged(nameof(FilteredServers));
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                FilterServers();
            }
        }

        private ServerModel _selectedServer;
        public ServerModel SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                OnPropertyChanged(nameof(SelectedServer));
            }
        }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get { return _isDarkMode; }
            set
            {
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));
                ToggleTheme();
            }
        }

        public ICommand DeleteServerCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        // Firebase configuration
        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "UkpGDcuqjkmyJwyE3bniiN0G81xWAC8PFd2nRKGg",
            BasePath = "https://vpnproject-7ec25-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        private IFirebaseClient client;

        public SettingsViewModel()
        {
            InitialiseFirebaseClient();
            Servers = new ObservableCollection<ServerModel>();
            FilteredServers = new ObservableCollection<ServerModel>();
            FetchServersFromFirebase().ConfigureAwait(false);

            DeleteServerCommand = new RelayCommand<ServerModel>(DeleteServer);
            ToggleThemeCommand = new RelayCommand<object>(param => IsDarkMode = !IsDarkMode);
        }

        private void InitialiseFirebaseClient()
        {
            client = new FireSharp.FirebaseClient(config);
            if (client == null)
            {
                throw new Exception("Could not connect to Firebase");
            }
        }

        private async Task FetchServersFromFirebase()
        {
            try
            {
                FirebaseResponse response = await client.GetTaskAsync("servers");
                if (response.Body != "null")
                {
                    var data = response.ResultAs<Dictionary<string, ServerModel>>();
                    Servers.Clear();
                    foreach (var server in data.Values)
                    {
                        server.FlagImageUrl = GetFlagImageUrl(server.Country);
                        Servers.Add(server);
                    }
                    FilterServers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching servers from Firebase: {ex.Message}");
            }
        }

        private string GetFlagImageUrl(string country)
        {
            return country switch
            {
                "US" => "https://i.imgur.com/tX2FzGr.png",
                "Canada" => "https://i.imgur.com/VIxuFmK.png",
                "UK" => "https://i.imgur.com/QW2YV9c.png",
                "Poland" => "https://i.imgur.com/k6ie3Ra.png",
                "Germany" => "https://i.imgur.com/l66r6qD.png",
                "France" => "https://i.imgur.com/XohHXyD.png",
                _ => "https://i.imgur.com/tX2FzGr.png"
            };
        }

        private void FilterServers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredServers = new ObservableCollection<ServerModel>(Servers);
            }
            else
            {
                var lowerCaseQuery = SearchQuery.ToLower();
                var filteredList = Servers.Where(s => s.Country.ToLower().Contains(lowerCaseQuery) ||
                                                      s.Server.ToLower().Contains(lowerCaseQuery) ||
                                                      s.Username.ToLower().Contains(lowerCaseQuery) ||
                                                      s.Password.ToLower().Contains(lowerCaseQuery)).ToList();
                FilteredServers = new ObservableCollection<ServerModel>(filteredList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ToggleTheme()
        {
            var theme = IsDarkMode ? "DarkTheme.Nord.xaml" : "LightTheme.Nord.xaml";
            var uri = new Uri($"/Themes/{theme}", UriKind.Relative);
            var resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/ToggleButton.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/SettingsServerListTheme.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/MenuButtons.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/ConnectButton.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/TitlebarButton.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/ServerListTheme.Nord.xaml", UriKind.Relative) });
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        public void ShowPopupWindow(ServerModel selectedServer)
        {
            var popupWindow = new VpnFormWindow();
            var viewModel = new VpnFormViewModel
            {
                Username = selectedServer?.Username ?? string.Empty,
                Password = selectedServer?.Password ?? string.Empty,
                Server = selectedServer?.Server ?? string.Empty,
                Country = selectedServer?.Country ?? string.Empty,
                IsEditMode = selectedServer != null
            };

            viewModel.OnSubmitAction = () =>
            {
                var username = viewModel.Username;
                var password = viewModel.Password;
                var server = viewModel.Server;
                var country = viewModel.Country;

                if (selectedServer != null)
                {
                    UpdateServerInFirebase(selectedServer.ID.ToString(), username, password, server, country);
                }
                else
                {
                    AddServerToFirebase(username, password, server, country);
                }

                // Close the popup
                popupWindow.DialogResult = true;
                popupWindow.Close();
            };

            popupWindow.DataContext = viewModel;
            popupWindow.Title = selectedServer != null ? "Edit VPN" : "Add VPN";

            popupWindow.ShowDialog();
        }

        private async void UpdateServerInFirebase(string serverId, string username, string password, string server, string country)
        {
            var serverData = new ServerModel
            {
                ID = int.Parse(serverId),
                Username = username,
                Password = password,
                Server = server,
                Country = country,
                FlagImageUrl = GetFlagUrl(country)
            };

            await client.UpdateTaskAsync($"servers/{serverId}", serverData);
            await FetchServersFromFirebase();
        }

        private async void AddServerToFirebase(string username, string password, string server, string country)
        {
            FirebaseResponse response = await client.GetTaskAsync("servers");
            var data = response.ResultAs<Dictionary<string, ServerModel>>();
            int newId = data.Count + 1;

            var serverData = new ServerModel
            {
                ID = newId,
                Username = username,
                Password = password,
                Server = server,
                Country = country,
                FlagImageUrl = GetFlagUrl(country)
            };

            await client.UpdateTaskAsync($"servers/{newId}", serverData);
            await FetchServersFromFirebase();
        }

        private async void DeleteServer(ServerModel server)
        {
            if (server != null)
            {
                await client.DeleteTaskAsync($"servers/{server.ID}");
                Servers.Remove(server);
                FilterServers();
            }
        }

        private string GetFlagUrl(string country)
        {
            return country switch
            {
                "US" => "https://i.imgur.com/tX2FzGr.png",
                "Canada" => "https://i.imgur.com/VIxuFmK.png",
                "UK" => "https://i.imgur.com/QW2YV9c.png",
                "Poland" => "https://i.imgur.com/k6ie3Ra.png",
                "Germany" => "https://i.imgur.com/l66r6qD.png",
                "France" => "https://i.imgur.com/XohHXyD.png",
                _ => "https://i.imgur.com/tX2FzGr.png"
            };
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

