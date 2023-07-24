using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LRCPSoftware.WPFControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LabeledTextBox : UserControl
    {
        public LabeledTextBox()
        {
            InitializeComponent();
            txt.PreviewTextInput += Txt_PreviewTextInput;
            txt.LostFocus += Txt_LostFocus;
        }

        #region Event Handlers

        private void Txt_LostFocus(object sender, RoutedEventArgs e) => LostFocus?.Invoke(this, e);
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e) => PreviewTextInput?.Invoke(this, e);

        #endregion

        #region Events

        public new event TextCompositionEventHandler PreviewTextInput;
        public new event EventHandler LostFocus;

        #endregion

        #region Properties

        public FontFamily LabelFontFamily
        {
            get => lbl.FontFamily;
            set => lbl.FontFamily = value;
        }
        public double LabelFontSize
        {
            get => lbl.FontSize;
            set => lbl.FontSize = value;
        }
        public FontStyle LabelFontStyle
        {
            get => lbl.FontStyle;
            set => lbl.FontStyle = value;
        }
        public FontWeight LabelFontWeight
        {
            get => lbl.FontWeight;
            set => lbl.FontWeight = value;
        }
        public Thickness LabelMargin
        {
            get => lbl.Margin;
            set => lbl.Margin = value;
        }
        public Thickness LabelPadding
        {
            get => lbl.Padding;
            set => lbl.Padding = value;
        }
        public string LabelText
        {
            get => lbl.Content.ToString();
            set => lbl.Content = value;
        }

        public string Text
        {
            get => txt.Text;
            set => txt.Text = value;
        }
        public FontFamily TextFontFamily
        {
            get => txt.FontFamily;
            set => txt.FontFamily = value;
        }
        public double TextFontSize
        {
            get => txt.FontSize;
            set => txt.FontSize = value;
        }
        public FontStyle TextFontStyle
        {
            get => txt.FontStyle;
            set => txt.FontStyle = value;
        }
        public FontWeight TextFontWeight
        {
            get => txt.FontWeight;
            set => txt.FontWeight = value;
        }
        public Thickness TextMargin
        {
            get => txt.Margin;
            set => txt.Margin = value;
        }
        public Thickness TextPadding
        {
            get => txt.Padding;
            set => txt.Padding = value;
        }

        #endregion

    }
}