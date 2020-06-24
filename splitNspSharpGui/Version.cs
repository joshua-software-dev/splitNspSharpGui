namespace splitNspSharpGui
{
    public static class Version
    {
        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "";
        }
    }
}