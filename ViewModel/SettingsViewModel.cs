using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

using VPNProject.Windows;


namespace VPNProject.ViewModel
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
                OnPropertyChanged();
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                FilterServers();
            }
        }

        // Firebase configuration
        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "UkpGDcuqjkmyJwyE3bniiN0G81xWAC8PFd2nRKGg", // firebase authentication secret
            BasePath = "https://vpnproject-7ec25-default-rtdb.europe-west1.firebasedatabase.app/" // firebase URL
        };

        private IFirebaseClient client;

        public SettingsViewModel()
        {
            InitialiseFirebaseClient();
            Servers = new ObservableCollection<ServerModel>();
            FilteredServers = new ObservableCollection<ServerModel>();
            FetchServersFromFirebase().Wait();
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
                //reinitialising Firebase client in case of network changes --> i.e. VPN connection established
                InitialiseFirebaseClient();

                FirebaseResponse response = await client.GetTaskAsync("servers");
                if (response.Body != "null")
                {
                    var data = response.ResultAs<Dictionary<string, ServerModel>>();
                    Servers.Clear();
                    foreach (var server in data.Values)
                    {
                        //Setting the flag URL based on the country
                        server.FlagImageUrl = server.Country switch
                        {
                            "US" => "https://i.imgur.com/tX2FzGr.png",
                            "Canada" => "https://i.imgur.com/VIxuFmK.png",
                            "UK" => "https://i.imgur.com/QW2YV9c.png",
                            "Poland" => "https://i.imgur.com/k6ie3Ra.png",
                            "Germany" => "https://i.imgur.com/l66r6qD.png",
                            "France" => "https://i.imgur.com/XohHXyD.png",
                            _ => "https://i.imgur.com/tX2FzGr.png"
                        };
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ServerModel _selectedServer;
        public ServerModel SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                OnPropertyChanged();
            }
        }

        public void ShowPopupWindow(ServerModel selectedServer)
        {
            var popupWindow = new VpnFormWindow();
            var viewModel = new VpnFormViewModel
            {
                Username = selectedServer?.Username ?? string.Empty,
                Password = selectedServer?.Password ?? string.Empty,
                Server = selectedServer?.Server ?? string.Empty,
                Country = selectedServer?.Country ?? string.Empty
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
            // Fetch existing servers to determine the new ID
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
                //FlagImageUrl = GetFlagUrl(country)
            };

            await client.UpdateTaskAsync($"servers/{newId}", serverData);
            await FetchServersFromFirebase();
        }

        //getting the right flag!
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
}

