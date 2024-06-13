using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPNproject.View;
using VPNproject.ViewModel;


namespace VPNproject
{
    internal class MainViewModel : ObservableObject
    {
        //Commands for the windows
        public RelayCommand MoveWindowCommand { get; set; }

        public RelayCommand ShutdownWindowCommand { get; set; }

        public RelayCommand MaximiseWindowCommand { get; set; }

        public RelayCommand MinimiseWindowCommand { get; set; }

        public RelayCommand ShowProtectionView { get; set; }
        public RelayCommand ShowSettingsView { get; set; }


        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ProtectionViewModel ProtectionVm { get; set; }

        public SettingsViewModel SettingsVm { get; set; }

        public MainViewModel()
        {
            //shOWCING the protection view 
            ProtectionVm = new ProtectionViewModel();
            SettingsVm = new SettingsViewModel();
            CurrentView = ProtectionVm;

            Application.Current.MainWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            MoveWindowCommand = new RelayCommand(o => { Application.Current.MainWindow.DragMove(); });
            ShutdownWindowCommand = new RelayCommand(o => { Application.Current.Shutdown(); });
            MaximiseWindowCommand = new RelayCommand(o => {
                if (Application.Current.MainWindow.WindowState == WindowState.Maximized) //check if normal or if maximised already!
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                }
            });
            MinimiseWindowCommand = new RelayCommand(o => { Application.Current.MainWindow.WindowState = WindowState.Minimized; });

            ShowProtectionView = new RelayCommand(o => { CurrentView = ProtectionVm; });
            ShowSettingsView = new RelayCommand(o => { CurrentView = SettingsVm; });
        }
    }
}
