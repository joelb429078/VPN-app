using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace VPNProject.ViewModel
{
    public class VpnFormViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _server;
        private string _country;

        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        public string Server
        {
            get { return _server; }
            set { _server = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get { return _country; }
            set { _country = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }
        public Action OnSubmitAction { get; set; }

        public VpnFormViewModel()
        {
            SubmitCommand = new RelayCommand(OnSubmit);
        }

        private void OnSubmit(object parameter)
        {
            OnSubmitAction?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
