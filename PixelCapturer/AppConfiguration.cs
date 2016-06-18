using System.Configuration;

namespace PixelCapturer
{
    public static class AppConfiguration
    {
        static AppConfiguration()
        {
            AutoRegisterInGac = bool.Parse(ConfigurationManager.AppSettings["autoRegisterInGAC"]);
        }
        public static bool AutoRegisterInGac { get; private set; }
    }
}