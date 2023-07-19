using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using VMS.TPS.Common.Model.API;

using ScriptContext = Context.ScriptContext;

namespace Convert2HiRes_sa
{
    /// <summary>
    /// Interaction logic for ConvertForm.xaml
    /// </summary>
    public partial class ConvertForm : UserControl
    {
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
            List<string> strIDs = _structureSet.Structures.Select(item => item.Id).ToList();
            strIDs.Sort();
            lbStructures.ItemsSource = strIDs;

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
            btnConvert.IsEnabled = false;
            btnClose.IsEnabled = false;
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
            btnClose.IsEnabled = true;
        }
        private void BtnNone_Click(object sender, RoutedEventArgs e)
        {
            lbStructures.SelectedItems.Clear();
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ScanStructures();
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
