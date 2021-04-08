using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AutoCompleteTextBox.Demo
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<string> _ltrItems;
        private ObservableCollection<string> _rtlItems;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            AddDummyData();
            RegisterCommands();

            DataContext = this;
        }

        #endregion Constructors

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

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

        #endregion Properties

        #region Commands

        public ICommand LtrDeleteCommand { get; set; }
        public ICommand RtlDeleteCommand { get; set; }

        #endregion Commands

        #region Command handlers

        /// <summary>
        /// 删除选项项命令
        /// </summary>
        /// <param name="parameter"></param>
        private void HandleLtrDeleteCommand(object parameter)
        {
            //点击删除的项
            var item = parameter.ToString();
            //更新列表集合
            var filteredItems = LtrItems.Where(i => i.ToLower() != item.ToLower());

            //更新数据源
            LtrItems = new ObservableCollection<string>(filteredItems);
        }

        private void HandleRtlDeleteCommand(object parameter)
        {
            var item = parameter.ToString();
            var filteredItems = RtlItems.Where(i => i.ToLower() != item.ToLower());

            RtlItems = new ObservableCollection<string>(filteredItems);
        }

        #endregion Command handlers

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

        #endregion Helper methods

        private void AutoCompleteTextField_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = this.autoCompleteText.Text;
            if (LtrItems.All(i => i.ToLower() != text.ToLower()))
            {
                LtrItems.Add(text);
            }
        }
    }
}