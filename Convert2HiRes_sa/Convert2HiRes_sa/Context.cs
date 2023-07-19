using System.Linq;
using VMS.TPS.Common.Model.API;

namespace Context
{
    public class User
    {
        public string Name { get; set; }
        public string Id => Name;
    }
    public class ScriptContext
    {
        public PlanSetup PlanSetup { get; set; }
        public Course Course { get; set; }
        public User CurrentUser { get; set; }
        public Image Image { get { return StructureSet.Image; } }
        public Patient Patient { get; set; }
        public StructureSet StructureSet { get { return PlanSetup != null ? PlanSetup.StructureSet : Patient.StructureSets.FirstOrDefault(); } }

        public static ScriptContext CreateContext(Patient patient, Course course, PlanSetup planSetup)
        {
            ScriptContext context = new ScriptContext
            {
                PlanSetup = planSetup,
                Course = course,
                Patient = patient
            };

            return context;
        }
    }
}
