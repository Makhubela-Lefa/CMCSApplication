namespace CMCSApplication.Models.ViewModels
{
    public class ModuleAssignmentVM
    {
        public List<Lecturer> Lecturers { get; set; } = new();
        public List<Module> Modules { get; set; } = new();
        public List<ModuleAssignment> Assignments { get; set; } = new();
    }
}
