using System;

namespace WebApp.UI.Helpers
{
    public static class UIUrlHelper
    {
        private static readonly Uri s_dummyBaseUri = new Uri("xx:");

        public static string GetRelativePath(string basePath, string path) =>
            new Uri(new Uri(s_dummyBaseUri, basePath), path).AbsolutePath;
    }
}
