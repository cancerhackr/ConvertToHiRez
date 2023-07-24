using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using CustomAttributes;
using Telemetry;
using Convert2HiRes_sa;
using System.Net.Sockets;
using System.Net;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0")]

[assembly: AssemblyType(ASSEMBLYTYPES.PLUGIN)]
[assembly: AssemblyTitle("Convert to High Res v1.0")]
[assembly: AssemblyDescription("Convert small structures (volume < 100 cc) to high resolution.")]
[assembly: AssemblyAuthorship("Jeff Kempe", "")]
[assembly: AssemblyQA("Jonatan Snir")]
[assembly: AssemblyDeveloperLead("Jeff Kempe", "2023-07-21")]
[assembly: AssemblyManager("Stewart Gaede", "")]
[assembly: AssemblyKeywords("Structures;convert;high res;high resolution")]

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        private string ScriptName = "Convert To High Res";
        private ScriptTelemetry _telemetry = null;

        private void SetUpTelemetry()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Version version;
            AssemblyName assemblyName;

            assemblyName = assembly.GetName();
            version = assemblyName.Version;

            ScriptName = $"{ScriptName} v{version.Major}.{version.Minor}";

            string location = assembly.Location;
            DateTime now = DateTime.Now;
            IPAddress host = Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray()[0];

            _telemetry = new ScriptTelemetry()
            {
                EntryDate = now,
                FileName = location,
                HostName = Environment.GetEnvironmentVariable("COMPUTERNAME"),
                HostIP = host.ToString(),
                ScriptName = ScriptName,
                StartTime = now,
                Status = "TBOX",
                Version = version,
            };
            if (location.ToUpper().Contains(@"\PROD\"))
            {
                _telemetry.Status = "PROD";
            }
            else if (location.ToUpper().Contains(@"\TBOX\"))
            {
                _telemetry.Status = "TBOX";
            }
            else if (location.ToUpper().Contains(@"\ESAPI\"))
            {
                _telemetry.Status = "ESAPI";
            }
            else
            {
                _telemetry.Status = "N/A";
            }
        }

        public Script()
        {
            SetUpTelemetry();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context, Window window/*, ScriptEnvironment environment*/)
        {
            window.Closing += Window_Closing;
            window.Title = ScriptName;

            _telemetry.CourseID = context.Course != null ? context.Course.Id : "NoCrs";
            _telemetry.PatientID = context.Patient != null ? context.Patient.Id : "NoPt";
            _telemetry.PlanID = context.PlanSetup != null ? context.PlanSetup.Id : "NoPln";
            _telemetry.UserID = context.CurrentUser.Id;

            if (context.Patient == null || context.StructureSet == null)
            {
                _ = MessageBox.Show(
                        "Please load a patient and structure set before running this script.",
                        ScriptName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation
                    );
                window.Loaded += Window_Loaded;
                _telemetry.Error = new Exception("No patient/structure-set loaded.");
                return;
            }
            _telemetry.Notes = $"StructureSet ID: {context.StructureSet.Id}";

            context.Patient.BeginModifications();

            window.Content = new ConvertForm(context);
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.MaxHeight = 600;
            window.MinHeight = 450;
            window.MaxWidth = 750;
            window.MinWidth = 600;
        }

        private void LogTelemetry()
        {
            if (_telemetry != null)
            {
                if (_telemetry.StopTime == new DateTime())
                {
                    _telemetry.StopTime = DateTime.Now;
                }
#if DEBUG
                _ = MessageBox.Show($"Logging telemetry:\n{_telemetry}", "Telemetry", MessageBoxButton.OK, MessageBoxImage.Information);
#else
                _telemetry?.LogEntry();
#endif
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogTelemetry();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;
            window.Close();
        }
    }
}
