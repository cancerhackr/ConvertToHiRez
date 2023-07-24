using LRCPSoftware.WPFControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Convert2HiRes_sa
{
    /// <summary>
    /// Interaction logic for ConvertForm.xaml
    /// </summary>
    public partial class ConvertForm : UserControl
    {
        #region Disable 'Close' Button on the title bar

        //  This statements use Runtime.Interop services to imprt functions from the user32.dll library (a win32 old-school API library)
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        //  These are constants used by win32 to identify and manipulate UI elements.
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_ENABLED = 0x0;
        private const uint MF_DISABLED = 0x00000002;
        private const uint SC_CLOSE = 0xF060;
        private IntPtr menuHandle;

        /// <summary>
        /// This function enables and disables the 'Close' button on the right side of the title bar.
        /// </summary>
        /// <param name="window">The window whose button is to be enabled/disabled</param>
        /// <param name="flag">The flag that indicates whether the button is to enabled or disabled</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void EnableCloseButton(Window window, uint flag)
        {
            if (window == null)
                throw new InvalidOperationException("The window has not yet been created");

            //  Get the window handle (a 32-bit unsigned integer) that represents the parent window
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            if (windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            //  Get the handle for the menu bar (this includes the close button).
            menuHandle = GetSystemMenu(windowHandle, false);
            if (menuHandle != IntPtr.Zero)
            {
                //  Enable/disable the title bar close button.
                EnableMenuItem(menuHandle, SC_CLOSE, MF_BYCOMMAND | flag);
            }
        }

        #endregion

        private readonly StructureSet _structureSet;

        public ConvertForm()
        {
            InitializeComponent();
            Height = double.NaN; Width = double.NaN;
            MinHeight = 300;
            MinWidth = 400;
            Threshold = 100;

            #region Banner

            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resources = assembly.GetManifestResourceNames();

            string imgName = resources.FirstOrDefault(x => x.Contains("banner"));
            PngBitmapDecoder png = new PngBitmapDecoder(
                assembly.GetManifestResourceStream(imgName), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default
                );
            banner.Source = png.Frames[0];

            #endregion

        }
        public ConvertForm(ScriptContext context) : this()
        {
            _structureSet = context.StructureSet;
            txtPatient.Text = $"{context.Patient.Id} - {_structureSet.Id}";
            List<string> strIDs = new List<string>();// = _structureSet.Structures.Select(item => item.Id).ToList();
            foreach (Structure s in _structureSet.Structures)
            {
                List<StructureApprovalHistoryEntry> approvals = new List<StructureApprovalHistoryEntry>(s.ApprovalHistory);
                approvals.Sort(new ApprovalHistoryComparer());
                if (approvals.Last().ApprovalStatus == StructureApprovalStatus.Approved)
                {
                    continue;
                }
                strIDs.Add(s.Id);
            }
            strIDs.Sort();
            lbStructures.ItemsSource = strIDs;
            txtThreshold.LostFocus += TxtThreshold_LostFocus;

            ScanStructures();
        }

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            lbStructures.SelectAll();
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }
        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            DisableGUI();
            //_ = Task.Run(() =>
            //{
            //    WorkClass.Items = (IEnumerable<object>)lbStructures.SelectedItems;
            //    WorkClass.StructureSet = _structureSet;
            //    WorkClass.DoWork();   //  crashes as soon as I try to access a structure.
            //});
            WorkClass.Items = (IEnumerable<object>)lbStructures.SelectedItems;
            WorkClass.StructureSet = _structureSet;
            WorkClass.DoWork();
            for (int i = 0; i < lbStructures.Items.Count; ++i)
            {
                var item = lbStructures.Items[i];
                Structure s = _structureSet.Structures.FirstOrDefault(str => str.Id == item.ToString());
                if (s.IsHighResolution)
                {
                    lbStructures.SelectedItems.Remove(item);
                }
            }
            //btnClose.IsEnabled = true;
            EnableGUI();
        }
        private void BtnNone_Click(object sender, RoutedEventArgs e)
        {
            lbStructures.SelectedItems.Clear();
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ScanStructures();
        }
        private void TxtThreshold_LostFocus(object sender, System.EventArgs e)
        {
            LabeledTextBox txt = sender as LabeledTextBox;
            txt.Text = double.Parse(txt.Text).ToString("0.0##");
        }
        private void TxtThreshold_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            bool parsed = false;
            string newVal = $"{txtThreshold.Text}{e.Text}";
            parsed = !double.TryParse(newVal, out double _);
            e.Handled = parsed;
        }

        private void DisableGUI()
        {
            Window window = Window.GetWindow(this);
            EnableCloseButton(window, MF_DISABLED);
            btnConvert.IsEnabled = false;
            btnClose.IsEnabled = false;
        }
        private void EnableGUI()
        {
            Window window = Window.GetWindow(this);
            EnableCloseButton(window, MF_ENABLED);
            //btnConvert.IsEnabled = true;
            btnClose.IsEnabled = true;
        }
        private void ScanStructures()
        {
            lbStructures.SelectedItems.Clear();
            foreach (var item in lbStructures.Items)
            {
                Structure s = _structureSet.Structures.FirstOrDefault(str => str.Id == item.ToString());
                if (s.IsHighResolution)
                {
                    continue;
                }
                if (s != null && s.Volume < Threshold)
                {
                    lbStructures.SelectedItems.Add(item);
                }
            }
        }

        private double Threshold
        {
            get
            {
                if (double.TryParse(txtThreshold.Text, out double threshold))
                {
                    return threshold;
                }
                return 0;
            }
            set => txtThreshold.Text = value.ToString("0.0##");
        }

    }
}
