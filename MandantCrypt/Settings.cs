using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace MandantCrypt
{
    public partial class Settings : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        const string userRoot = "HKEY_CURRENT_USER";
        const string machineRoot = "HKEY_LOCAL_MACHINE";
        const string subkey = "Software\\zCloud\\MandantCrypt";
        const string keyName = userRoot + "\\" + subkey;
        const string SERVER_URL = "ServerUrl";
        const string DEV_MODE = "DeveloperMode";
        const string LAST_MANDANT_INDEX = "LastMandantIndex";
        const string DEFAULT_PACKAGE_MODE = "DefaultPackageMode";
        const string DEFAULT_URL = "https://mandantcryptoserver";
        internal static Packer.PackerType[] ALLOWED_TYPES = { Packer.PackerType.ZIP, Packer.PackerType.SEVEN_ZIP, Packer.PackerType.ITEXT7 };


        private ResourceManager rm;
        private AppSettings appSettings = new AppSettings();

        public Settings()
        {
            log.Debug("Loading settings window");
            InitializeComponent();
            rm = MandantCrypt.Strings.ResourceManager;
            log.Debug("Loaded ressource manager");
            log.Info("Allowed types in this version are: " + JsonConvert.SerializeObject(ALLOWED_TYPES));
            errorLabel.Visible = false;
            readSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            errorLabel.Visible = false;
            if (validateInput())
            {
                bool res = saveSettings();
                if (!res)
                {
                    showError(rm.GetString("Error_CannotSaveSettings"));
                    return;
                }
            }
        }

        public AppSettings getAppSettings()
        {
            readSettings();
            return this.appSettings;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            log.Debug($"Closing Settings window");
            this.Close();
        }

        private bool saveSettings()
        {
            bool success = false;
            log.Debug($"Saving current settings");
            try
            {
                string url = textBox1.Text;
                if (url.EndsWith("/"))
                {
                    url = url.Remove(url.Length - 1);
                }
                Registry.SetValue(keyName, SERVER_URL, url,RegistryValueKind.ExpandString);
                int devMode = checkBox1.Checked ? 1 : 0;
                Registry.SetValue(keyName, DEV_MODE, devMode, RegistryValueKind.DWord);
                success = true;
            } catch (Exception e)
            {
                success = false;
                showError(rm.GetString("Error_CouldNotSaveSettings") + ": "+ e.Message);
            }

            saveDefaultPackageMode((Packer.PackerType)comboBox1.SelectedItem);

            log.Debug($"Saved current settings");

            return success;
        }

        public void saveLastMandantIndex(int index)
        {
            Registry.SetValue(keyName, LAST_MANDANT_INDEX, index, RegistryValueKind.DWord);
        }

        public int loadLastMandantIndex()
        {
            return (int)Registry.GetValue(keyName, LAST_MANDANT_INDEX, 1);
        }

        private bool readSettings()
        {
            Uri uriResult;
            bool devMode = false;
            string url = DEFAULT_URL;
            log.Debug("Loading settings from registry");
            try
            {
                url = (String) Registry.GetValue(keyName, SERVER_URL, "");
                log.Debug($"Received URL [{url}] from regsitry");
                    
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
                {
                    log.Warn($"Could not use URL from registry, using default URL");
                    url = DEFAULT_URL;
                }

                if (url.EndsWith("/"))
                {
                    log.Debug($"URL ends in /, so removing it");
                    url = url.Remove(url.Length - 1);
                }
            }
            catch (Exception e)
            {
                log.Error("Error while parsing server URL, falling back to default URL");
                url = DEFAULT_URL;
            }

            log.Debug($"URL is now [{url}]");

            int devModeInt = (int)Registry.GetValue(keyName, DEV_MODE, 0);
            devMode = devModeInt > 0 ? true : false;
            log.Debug($"DevMode is set to [{devMode}]");

            this.appSettings.setServerUrl(url);
            this.appSettings.setDevMode(devMode);
            this.checkBox1.Checked = devMode;
            this.textBox1.Text = url;

            populatePackageTypeDropDown();

            return true; ;
        }

        public static bool isAllowedType(Packer.PackerType type)
        {
            foreach (Packer.PackerType t in ALLOWED_TYPES)
            {
                if (type == t) return true;
            }
            return false;
        }

        private void populatePackageTypeDropDown()
        {
            log.Debug("Populating Default Package Mode dropdown");
            this.comboBox1.Items.Clear();
            foreach (Packer.PackerType tp in Enum.GetValues(typeof(Packer.PackerType)))
            {
                if (isAllowedType(tp))
                {
                    this.comboBox1.Items.Add(tp);
                    log.Debug($"Added [{tp}] to Default Package Mode dropdown");
                }
                else
                {
                    log.Debug($"Did not add [{tp}] to Default Package Mode dropdown as its not in ALLOWED_TYPES");
                }
            }
            this.comboBox1.SelectedItem = loadDefaultPackageMode();
            log.Debug($"Selected item in Default Package Mode dropdown is set to  [{loadDefaultPackageMode()}]");
        }

        private bool validateInput()
        {
            Uri uriResult;
            log.Debug($"Validating user entered URL: [{textBox1.Text}]");
            bool result = Uri.TryCreate(textBox1.Text, UriKind.Absolute, out uriResult);
            if (!result)
            {
                showError(rm.GetString("Error_NoUrl"));
                return false;
            }

            if ((uriResult.Scheme != Uri.UriSchemeHttps) && !checkBox1.Checked)
            {
                showError(rm.GetString("Error_NotHttps"));
                return false;
            }
            log.Debug($"User entered URL was validated: [{textBox1.Text}]");
            return true;
        }

        private void showError(String text)
        {
            String prefix = rm.GetString("Error_Prefix");
            errorLabel.Text = prefix + text;
            errorLabel.Visible = true;
            log.Warn($"Showing an error in settings window: [{errorLabel.Text}]");
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            readSettings();
        }

        private static void saveDefaultPackageMode(Packer.PackerType type)
        {
            Registry.SetValue(keyName, DEFAULT_PACKAGE_MODE, type, RegistryValueKind.DWord);
        }

        public static Packer.PackerType loadDefaultPackageMode()
        {
            return (Packer.PackerType)Registry.GetValue(keyName, DEFAULT_PACKAGE_MODE, 0);
        }

    }

    public class AppSettings
    {
        string serverUrl;
        bool devMode = false;

        public string getServerUrl() { return this.serverUrl; }
        public bool getDevMode() { return this.devMode; }
        public void setServerUrl(string url) { this.serverUrl = url; }
        public void setDevMode(bool devmode) { this.devMode = devmode; }
        public string asJson() { return JsonConvert.SerializeObject(this); }
        public int lastSelectedMandantIndex { get; set; }
        // public void fromJson(string json)
        // {
        //     AppSettings decoded = JsonConvert.DeserializeObject<AppSettings>(json);
        //     this.setDevMode(decoded.getDevMode());
        //     this.setServerUrl(decoded.getServerUrl());
        // }
    }
}
