namespace SFramework.Configs.Runtime
{
    public static class SFConfigsExtensions
    {
        public static string GetSFId(params string[] names)
        {
            return string.Join('/', names);
        }
    }
}