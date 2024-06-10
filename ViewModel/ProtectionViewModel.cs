using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;


namespace VPNProject.ViewModel
{
    internal class ProtectionViewModel : ObservableObject
    {
        public ObservableCollection<ServerModel> Servers { get; set; }


        //Firebase connection stuff
        private IFirebaseConfig config = new FirebaseConfig 
        {
            AuthSecret = "UkpGDcuqjkmyJwyE3bniiN0G81xWAC8PFd2nRKGg", //firebase authentication secret also removed - add your own one if downloading and testing from github!
            BasePath = "https://vpnproject-7ec25-default-rtdb.europe-west1.firebasedatabase.app/" //firebase link removed for privacy reasons!
        };

        private IFirebaseClient client;

        public ProtectionViewModel()
        {
            InitialiseFirebaseClient();
            Servers = new ObservableCollection<ServerModel>();
            FetchServersFromFirebase();
            ConnectCommand = new RelayCommand(ConnectToServer);
        }

        private void InitialiseFirebaseClient()
        {
            client = new FireSharp.FirebaseClient(config);
            if (client == null)
            {
                throw new Exception("Could not connect to Firebase");
            }
        }

        private async void FetchServersFromFirebase()
        {
            try
            {
                // Reinitialize Firebase client in case of network changes
                InitialiseFirebaseClient();
                FirebaseResponse response = await client.GetTaskAsync("servers");
                if (response.Body != "null")
                {
                    var data = response.ResultAs<Dictionary<string, ServerModel>>();
                    Servers.Clear();
                    foreach (var server in data.Values)
                    {
                        // Country flags logic
                        server.FlagImageUrl = server.Country switch
                        {
                            "US" => "https://i.imgur.com/tX2FzGr.png",
                            "Canada" => "https://i.imgur.com/VIxuFmK.png",
                            "UK" => "https://i.imgur.com/QW2YV9c.png",
                            "Poland" => "https://i.imgur.com/k6ie3Ra.png",
                            "Germany" => "https://i.imgur.com/l66r6qD.png",
                            "France" => "https://i.imgur.com/XohHXyD.png",
                            _ => "https://i.imgur.com/VIxuFmK.png"
                        };
                        Console.WriteLine($"The country is {server.Country} --> with link {server.FlagImageUrl}");
                        Servers.Add(server);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching servers from Firebase: {ex.Message}");
            }
        }



        private string _connectionStatus;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged();
            }
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

        public RelayCommand ConnectCommand { get; set; }

        private void ConnectToServer(object obj)
        {
            if (SelectedServer == null)
            {
                ConnectionStatus = "No server selected.";
                return;
            }

            ServerBuilder();

            Task.Run(() =>
                {
                    ConnectionStatus = $"Connecting to {SelectedServer.Country} Server...";
                    var process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

                    string entryName = SelectedServer.Server;
                    string username = SelectedServer.Username;
                    string password = SelectedServer.Password;
                    string phoneBookPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "VPN", $"{SelectedServer.Server}.pbk"));
                    string arguments = $@"/c rasdial MyServer {username} {password} /phonebook:{phoneBookPath}";
                    process.StartInfo.Arguments = arguments;


                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    //paths & arguments for rasdial & pbk
                    Debug.WriteLine($"Phonebook Path: {phoneBookPath}");
                    Debug.WriteLine($"Arguments: {arguments}");



                    //Debug - error & outputs for debugg
                    Debug.WriteLine("Output: " + output);
                    Debug.WriteLine("Error: " + error);

                    //connection checking based on output and exit code
                    if (process.ExitCode == 0 && output.Contains("Command completed successfully."))
                    {
                        Debug.WriteLine("Success!");
                        ConnectionStatus = "Connected!";
                    }
                    else
                    {
                        switch (process.ExitCode)
                        {
                            case 691:
                                Debug.WriteLine("Wrong credentials!");
                                ConnectionStatus = "Wrong credentials!";
                                break;
                            default:
                                Debug.WriteLine($"Error: {process.ExitCode}, {error}");
                                ConnectionStatus = $"Error: {process.ExitCode}";
                                break;
                        }
                    }
                });
        }

        private void ServerBuilder()
        {
            if (SelectedServer == null)
            {
                MessageBox.Show("No server selected.");
                return;
            }

            var address = SelectedServer.Server;
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "VPN");
            var pbkPath = Path.Combine(folderPath, $"{address}.pbk");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (File.Exists(pbkPath))
            {
                Debug.WriteLine("Connection already exists!");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[MyServer]");
            //sb.AppendLine($"[{address}]");
            sb.AppendLine("MEDIA=rastapi");
            sb.AppendLine("Port=VPN2-0");
            sb.AppendLine("Device=WAN Miniport (IKEv2)");
            sb.AppendLine("DEVICE=vpn");
            sb.AppendLine($"PhoneNumber={address}");
            File.WriteAllText(pbkPath, sb.ToString());

            Debug.WriteLine($"Phonebook file created: {pbkPath}");
        }
    }
}