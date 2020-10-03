using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace AutoCompleteTextBox.Demo
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<string> _ltrItems;
        private ObservableCollection<string> _rtlItems;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            AddDummyData();
            RegisterCommands();

            DataContext = this;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public ObservableCollection<string> LtrItems
        {
            get => _ltrItems;
            set
            {
                _ltrItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LtrItems)));
            }
        }

        public ObservableCollection<string> RtlItems
        {
            get => _rtlItems;
            set
            {
                _rtlItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RtlItems)));
            }
        }


        #endregion

        #region Commands

        public ICommand LtrDeleteCommand { get; set; }
        public ICommand RtlDeleteCommand { get; set; }

        #endregion

        #region Command handlers

        private void HandleLtrDeleteCommand(object parameter)
        {
            var item = parameter.ToString();
            var filteredItems = LtrItems.Where(i => i.ToLower() != item.ToLower());

            LtrItems = new ObservableCollection<string>(filteredItems);
        }

        private void HandleRtlDeleteCommand(object parameter)
        {
            var item = parameter.ToString();
            var filteredItems = RtlItems.Where(i => i.ToLower() != item.ToLower());

            RtlItems = new ObservableCollection<string>(filteredItems);
        }

        #endregion

        #region Helper methods

        private void AddDummyData()
        {
            LtrItems = new ObservableCollection<string>
            {
                "window",
                "How to connect to sql in c#",
                "wood",
                "Good for one",
                "first",
                "first time search",
                "WPF custom text field",
                "How to create custom window chrome in wpf",
                "Create radio button with custom functionality in wpf",
                "Good for one, good for all",
                "Submit your email address to stay informed",
                "Open file in c++",
                "C# display system information",
                "How to connect to sql server from remote computer",
                "the best practice for checking internet connection",
            };

            RtlItems = new ObservableCollection<string>
            {
                "سلام",
                "سلام چطوری",
                "خوبم تو چطوری",
                "سورس سرا",
                "سورس کدهای آماده",
                "انجمن سورس سرا",
                "سورس بازی مار",
                "سورس پیام رسان به زبان سی شارپ",
                "سورس برنامه حساب داری",
                "سورس بازی شطرنج به زبان سی پلاس پلاس",
                "دانلود نرم افزار ویژوال استودیو",
                "دانلود نرم Visual Studio 2019",
            };
        }

        private void RegisterCommands()
        {
            LtrDeleteCommand = new RelayCommand(HandleLtrDeleteCommand);
            RtlDeleteCommand = new RelayCommand(HandleRtlDeleteCommand);
        }

        #endregion
    }
}
