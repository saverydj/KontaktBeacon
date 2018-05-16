using System;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Principal;

namespace KontaktBeacon.StaticTools
{
    public static class Config
    {
        public static string WebAddr;
        public static string AcceptHeader;
        public static string PrivateAPIKey;

        private static void UpdateFields()
        {
            WebAddr = FormatPath(AppConfig("WebAddr"));
            AcceptHeader = FormatPath(AppConfig("AcceptHeader"));
            PrivateAPIKey = FormatPath(AppConfig("PrivateAPIKey"));
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static Config()
        {
            string path = typeof(Config).Assembly.Location + ".config";
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dir;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = file;
            watcher.Changed += new FileSystemEventHandler(OnAppConfigChanged);
            watcher.EnableRaisingEvents = true;

            UpdateFields();
        }

        private static void OnAppConfigChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                UpdateFields();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("being used by another process"))
                {
                    throw ex;
                }
            }
        }

        private static string FormatPartialPath(string path)
        {
            path = FormatPath(path);
            if (!path.EndsWith(@"\"))
            {
                return path + @"\";
            }
            return path;
        }

        private static string FormatPath(string path)
        {
            return path.Replace("*here", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        private static string AppConfig(string key)
        {
            Configuration config = null;
            string exeConfigPath = typeof(Config).Assembly.Location;
            config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            if (config == null || config.AppSettings.Settings.Count == 0)
            {
                throw new Exception(String.Format("Config file {0}.config is missing or could not be loaded.", exeConfigPath));
            }

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }

        private static void SetField(string key, string value)
        {
            Configuration config = null;
            string exeConfigPath = typeof(Config).Assembly.Location;
            config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            if (config == null || config.AppSettings.Settings.Count == 0)
            {
                throw new Exception(String.Format("Config file {0}.config is missing or could not be loaded.", exeConfigPath));
            }

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element == null) return;
            element.Value = value;
            config.Save();

            UpdateFields();
        }

    }
}
