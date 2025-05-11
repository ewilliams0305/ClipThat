namespace ClipThat.Setup;

public class TheorySkipsOnActionsAttribute : TheoryAttribute
{
    public TheorySkipsOnActionsAttribute()
    {
        if (IsRunningOnCI())
        {
            Skip = "Test skipped on CI environment";
        }
    }

    private bool IsRunningOnCI()
    {
        return Environment.GetEnvironmentVariable("CI") == "true" ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_BUILDID")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
    }
}