using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.Reflection;
using Microsoft.Win32;

namespace MandantCrypt
{
    public partial class Settings : Form
    {
        const string userRoot = "HKEY_CURRENT_USER";
        const string subkey = "Software\\zCloud\\MandantCrypt";
        const string keyName = userRoot + "\\" + subkey;
        const string SERVER_URL = "ServerUrl";
        const string DEV_MODE = "DeveloperMode";
        const string LAST_MANDANT_INDEX = "LastMandantIndex";
        const string DEFAULT_URL = "https://mandantcrypto";


        private ResourceManager rm;
        private AppSettings appSettings = new AppSettings();

        public Settings()
        {
            InitializeComponent();
            rm = MandantCrypt.Strings.ResourceManager;
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
            this.Close();
        }

        private bool saveSettings()
        {
            bool success = false;
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
            bool success = false;
            Uri uriResult;
            bool devMode = false;
            string url = DEFAULT_URL;
            try
            {
                url = (String) Registry.GetValue(keyName, SERVER_URL, "");
                if (url.EndsWith("/"))
                {
                    url = url.Remove(url.Length - 1);
                }
                    
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
                {
                    url = DEFAULT_URL;
                }
                int devModeInt = (int) Registry.GetValue(keyName, DEV_MODE, 0);
                devMode = devModeInt > 0 ? true : false;
                success = true;
            }
            catch (Exception e)
            {
                
            }

            this.appSettings.setServerUrl(url);
            this.appSettings.setDevMode(devMode);
            this.checkBox1.Checked = devMode;
            this.textBox1.Text = url;

            return success;
        }

        private bool validateInput()
        {
            Uri uriResult;
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

            return true;
        }

        private void showError(String text)
        {
            String prefix = rm.GetString("Error_Prefix");
            errorLabel.Text = prefix + text;
            errorLabel.Visible = true;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            readSettings();
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
