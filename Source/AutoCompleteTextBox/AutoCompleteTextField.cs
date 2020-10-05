using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoCompleteTextBox
{
    public class AutoCompleteTextField : Control
    {
        #region Fields

        private const string TextBoxPartName = "PART_TextBox";
        private const string PopupTextBoxPartName = "PART_PopupTextBox";
        private const string SuggestionPopupPartName = "PART_SuggestionPopup";
        private const string SuggestionListBoxPartName = "PART_SuggestionListBox";

        private TextBox _textBox;
        private TextBox _popupTextBox;
        private Popup _suggestionPopup;
        private ListBox _suggestionListBox;

        #endregion

        #region Constructors

        static AutoCompleteTextField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextField),
                new FrameworkPropertyMetadata(typeof(AutoCompleteTextField)));
        }

        public AutoCompleteTextField()
        {
            Loaded += (sender, args) => RegisterEventHandlers();
            Unloaded += (sender, args) => UnregisterEventHandlers();
        }

        #endregion

        #region Base methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LoadControls();

            var selectItemCommandBinding = new CommandBinding(SelectItemCommand, HandleSelectItemCommand);
            CommandBindings.Add(selectItemCommandBinding);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            _textBox.Focus();
        }

        #endregion

        #region Static commands

        public static RoutedCommand SelectItemCommand = new RoutedCommand();

        #endregion

        #region Command handlers

        private void HandleSelectItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            _suggestionListBox.SelectedItem = e.Parameter;
            OnItemSelected();
        }

        #endregion

        #region Helper methods

        private void LoadControls()
        {
            _textBox = GetTemplateChild(TextBoxPartName) as TextBox;
            _suggestionPopup = GetTemplateChild(SuggestionPopupPartName) as Popup;
            _popupTextBox = GetTemplateChild(PopupTextBoxPartName) as TextBox;
            _suggestionListBox = GetTemplateChild(SuggestionListBoxPartName) as ListBox;
        }

        private void RegisterEventHandlers()
        {
            PreviewKeyDown += OnPreviewKeyDown;
            KeyDown += OnKeyDown;
            _textBox.GotMouseCapture += OnTextBoxGotMouseCapture;
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            _popupTextBox.PreviewKeyDown += OnPopupTextBoxPreviewKeyDown;
            _suggestionPopup.Opened += OnSuggestionPopupOpened;
            _suggestionPopup.Closed += OnSuggestionPopupClosed;
        }

        private void UnregisterEventHandlers()
        {
            PreviewKeyDown -= OnPreviewKeyDown;
            KeyDown -= OnKeyDown;
            _textBox.GotMouseCapture -= OnTextBoxGotMouseCapture;
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            _popupTextBox.PreviewKeyDown -= OnPopupTextBoxPreviewKeyDown;
            _suggestionPopup.Opened -= OnSuggestionPopupOpened;
            _suggestionPopup.Closed -= OnSuggestionPopupClosed;
        }

        private string GetSelectedItem()
            => (_suggestionListBox.SelectedItem as AutoCompleteTextFieldMatchResult).Item;

        private void OnItemSelected()
        {
            _textBox.Text = GetSelectedItem();
            _textBox.CaretIndex = _textBox.Text.Length;
            _textBox.Focus();

            _suggestionPopup.IsOpen = false;

            if (ItemSelectionCommand is null)
                return;

            if (ItemSelectionCommand.CanExecute(Text))
            {
                ItemSelectionCommand.Execute(Text);
                ClosePopup();
            }
        }

        private void ClosePopup()
        {
            _suggestionListBox.ItemsSource = null;
            _suggestionPopup.IsOpen = false;
        }

        private void CreateSuggestion()
        {
            if (!IsLoaded)
                return;

            if (ItemsSource is null || ItemsSource.Count() == 0)
            {
                ClosePopup();
                return;
            }

            var value = _textBox.Text.ToLower();
            var filteredItems = new List<AutoCompleteTextFieldMatchResult>();

            ItemsSource.ToList().ForEach(i =>
            {
                var matchInfo = IsMatch(value, i.ToLower());

                if (matchInfo.Percentage >= MinimumMatchPercentage)
                    filteredItems.Add(matchInfo);

            });

            filteredItems = filteredItems.OrderByDescending(i => i.Percentage).Take(MaxSuggestionItems).ToList();

            if (filteredItems.Count() == 1 && filteredItems.First().Item.Equals(_textBox.Text, StringComparison.OrdinalIgnoreCase))
            {
                ClosePopup();
                return;
            }

            if (filteredItems.Count() > 0)
            {
                _suggestionListBox.ItemsSource = filteredItems;
                _suggestionListBox.SelectedIndex = 0;
                _suggestionPopup.IsOpen = true;
                return;
            }

            ClosePopup();
        }

        private AutoCompleteTextFieldMatchResult IsMatch(string input, string suggestedItem)
        {
            var inputWords = input.Split(' ');
            var totalMatchPercentage = inputWords.Length * 100;
            var matchedWords = new List<AutoCompleteTextFieldMatchResult>();

            foreach (var inputWord in inputWords)
            {
                var matchInfo = GetWordMatchInfo(inputWord, suggestedItem);
                matchedWords.Add(matchInfo);
            }

            var matchedWordsPercentage = matchedWords.Sum(i => i.Percentage);
            var percentage = (matchedWordsPercentage * 100) / totalMatchPercentage;

            return new AutoCompleteTextFieldMatchResult { Percentage = percentage, Item = suggestedItem };
        }

        private AutoCompleteTextFieldMatchResult GetWordMatchInfo(string input, string suggestedItem)
        {
            var inputChars = input.ToCharArray();
            var suggestedItemWords = suggestedItem.Split(' ');
            var matchInfo = new AutoCompleteTextFieldMatchResult();

            foreach (var word in suggestedItemWords)
            {
                if (input == word)
                {
                    matchInfo.Percentage = 100;
                    return matchInfo;
                }

                if (word.Length < input.Length)
                    continue;

                var wordChars = word.ToCharArray();
                var matchCount = GetMatchCount(inputChars, wordChars);

                var matchPercentage = matchCount * 100 / wordChars.Length;

                if (matchCount > 1)
                {
                    matchInfo.Percentage = matchPercentage;
                    return matchInfo;
                }
            }

            return matchInfo;
        }

        private static int GetMatchCount(char[] input, char[] word)
        {
            if (input.Length == 0 || word.Length == 0)
                return 0;

            int counter;

            for (var i = 0; i < word.Length; i++)
            {
                counter = 0;

                if (word[i] != input[0])
                    continue;

                for (int j = 0, k = i; j < input.Length; j++, k++)
                {
                    if (k < word.Length && word[k] == input[j])
                        counter++;
                }

                if (counter == input.Length)
                    return counter;
            }

            return 0;
        }

        #endregion

        #region Event handlers

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _suggestionPopup.IsOpen)
            {
                OnItemSelected();
                return;
            }

            if (e.Key == Key.Escape)
            {
                ClosePopup();
                return;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (SubmitByEnterCommand is null || e.Key != Key.Return)
                return;

            if (SubmitByEnterCommand.CanExecute(Text))
            {
                SubmitByEnterCommand.Execute(Text);
                ClosePopup();
            }
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                CreateSuggestion();
                return;
            }
        }

        private void OnPopupTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && _suggestionListBox.Items.Count > 0)
            {
                _suggestionPopup.IsOpen = true;

                if (_suggestionListBox.SelectedIndex < _suggestionListBox.Items.Count)
                    _suggestionListBox.SelectedIndex++;

                return;
            }

            if (e.Key == Key.Up && _suggestionListBox.Items.Count > 0)
            {
                _suggestionPopup.IsOpen = true;

                if (_suggestionListBox.SelectedIndex > 0)
                    _suggestionListBox.SelectedIndex--;

                return;
            }
        }

        private void OnTextBoxGotMouseCapture(object sender, MouseEventArgs e)
        {
            if (_suggestionPopup.IsOpen)
                return;

            if (_suggestionListBox.Items.Count > 0)
            {
                _suggestionPopup.IsOpen = true;
                _popupTextBox.CaptureMouse();
            }
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            CreateSuggestion();
        }

        private void OnSuggestionPopupOpened(object sender, EventArgs e)
        {
            _popupTextBox.CaretIndex = _popupTextBox.Text.Length;
            _popupTextBox.Focus();
        }

        private void OnSuggestionPopupClosed(object sender, EventArgs e)
        {
            _textBox.CaretIndex = _popupTextBox.Text.Length;
            _textBox.Focus();
        }

        #endregion

        #region DependencyProperty : ItemsSource

        public IEnumerable<string> ItemsSource
        {
            get { return (IEnumerable<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource),
                typeof(IEnumerable<string>), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextField autoCompleteTextField)
                autoCompleteTextField.CreateSuggestion();
        }

        #endregion

        #region DependencyProperty : MinimumMatchPercentage

        public int MinimumMatchPercentage
        {
            get { return (int)GetValue(MinimumMatchPercentageProperty); }
            set { SetValue(MinimumMatchPercentageProperty, value); }
        }

        public static readonly DependencyProperty MinimumMatchPercentageProperty =
            DependencyProperty.Register(nameof(MinimumMatchPercentage),
                typeof(int), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : MaxSuggestionItems

        public int MaxSuggestionItems
        {
            get { return (int)GetValue(MaxSuggestionItemsProperty); }
            set { SetValue(MaxSuggestionItemsProperty, value); }
        }

        public static readonly DependencyProperty MaxSuggestionItemsProperty =
            DependencyProperty.Register(nameof(MaxSuggestionItems),
                typeof(int), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                typeof(string), typeof(AutoCompleteTextField),
                new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region DependencyProperty : Hint

        public object Hint
        {
            get { return GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint),
                typeof(object), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : CornerRadius

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius),
                typeof(CornerRadius), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : RemoveItemCommand

        public ICommand RemoveItemCommand
        {
            get { return (ICommand)GetValue(RemoveItemCommandProperty); }
            set { SetValue(RemoveItemCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveItemCommandProperty =
            DependencyProperty.Register(nameof(RemoveItemCommand),
                typeof(ICommand), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : SubmitByEnterCommand

        public ICommand SubmitByEnterCommand
        {
            get { return (ICommand)GetValue(SubmitByEnterCommandProperty); }
            set { SetValue(SubmitByEnterCommandProperty, value); }
        }

        public static readonly DependencyProperty SubmitByEnterCommandProperty =
            DependencyProperty.Register(nameof(SubmitByEnterCommand),
                typeof(ICommand), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : ItemSelectionCommand

        public ICommand ItemSelectionCommand
        {
            get { return (ICommand)GetValue(ItemSelectionCommandProperty); }
            set { SetValue(ItemSelectionCommandProperty, value); }
        }

        public static readonly DependencyProperty ItemSelectionCommandProperty =
            DependencyProperty.Register(nameof(ItemSelectionCommand),
                typeof(ICommand), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : ListBoxSelectedItemBackground

        public Brush ListBoxSelectedItemBackground
        {
            get { return (Brush)GetValue(ListBoxSelectedItemBackgroundProperty); }
            set { SetValue(ListBoxSelectedItemBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ListBoxSelectedItemBackgroundProperty =
            DependencyProperty.Register(nameof(ListBoxSelectedItemBackground),
                typeof(Brush), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : ListBoxItemMouseOverBackground

        public Brush ListBoxItemMouseOverBackground
        {
            get { return (Brush)GetValue(ListBoxItemMouseOverBackgroundProperty); }
            set { SetValue(ListBoxItemMouseOverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ListBoxItemMouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(ListBoxItemMouseOverBackground),
                typeof(Brush), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion

        #region DependencyProperty : SuggestionListMaxHeight

        public double SuggestionListMaxHeight
        {
            get { return (double)GetValue(SuggestionListMaxHeightProperty); }
            set { SetValue(SuggestionListMaxHeightProperty, value); }
        }

        public static readonly DependencyProperty SuggestionListMaxHeightProperty =
            DependencyProperty.Register(nameof(SuggestionListMaxHeight),
                typeof(double), typeof(AutoCompleteTextField),
                new UIPropertyMetadata(default));

        #endregion
    }
}
