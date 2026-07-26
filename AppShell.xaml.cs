using StudySync.Views;

namespace StudySync;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register detail page routes for modal/push navigation
        Routing.RegisterRoute("CourseDetailPage", typeof(CourseDetailPage));
        Routing.RegisterRoute("TimetableEntryPage", typeof(TimetableEntryPage));
        Routing.RegisterRoute("AssignmentDetailPage", typeof(AssignmentDetailPage));
        Routing.RegisterRoute("ExamDetailPage", typeof(ExamDetailPage));
    }
}
