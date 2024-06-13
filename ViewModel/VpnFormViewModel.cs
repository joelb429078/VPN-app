using System;
using System.ComponentModel;
using System.Windows.Input;

namespace VPNproject.ViewModel
{
    public class VpnFormViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _server;
        private string _country;
        private bool _isEditMode;
        private string _submitButtonText;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged(nameof(Server));
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertyChanged(nameof(Country));
            }
        }

        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
                SubmitButtonText = value ? "Edit" : "Add";
            }
        }

        public string SubmitButtonText
        {
            get { return _submitButtonText; }
            set
            {
                _submitButtonText = value;
                OnPropertyChanged(nameof(SubmitButtonText));
            }
        }

        public ICommand SubmitCommand { get; }
        public Action OnSubmitAction { get; set; }

        public VpnFormViewModel()
        {
            SubmitCommand = new RelayCommand(OnSubmit);
            SubmitButtonText = "Add";
        }

        private void OnSubmit(object parameter)
        {
            OnSubmitAction?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
