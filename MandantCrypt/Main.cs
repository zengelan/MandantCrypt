using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;
using Newtonsoft.Json;

namespace MandantCrypt
{
    public partial class Main : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        const double MaxInputMBytes = 20;
        Settings settingsForm = new Settings();
        AppSettings appSettings;
        private ResourceManager rm;
        ApiClient server;
        private string outFile;


        public Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("Starting");
            InitializeComponent();
            log.Debug("Initialized");
            rm = MandantCrypt.Strings.ResourceManager;
            labelStatus.Text = rm.GetString("Status_Loading");
            appSettings = settingsForm.getAppSettings();
            ColumnHeader columnHeader1 = this.listView1.Columns[0];
            columnHeader1.Text = rm.GetString("FileList_Header");
            columnHeader1.Width = this.listView1.Width - 21;
            this.listView1.Items.Clear();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            server = new ApiClient(this.appSettings.getServerUrl());
            log.Debug("Created new api client:");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            settingsForm.ShowDialog();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.listView1.Items.Clear();
            string[] selectedFiles = openFileDialog1.FileNames;
            listView1.Items.AddRange(getListItemsFromFiles(selectedFiles));
            log.Debug("Files selected in the file dialog: " + JsonConvert.SerializeObject(selectedFiles));
        }


        private ListViewItem[] getListItemsFromFiles(string[] files)
        {
            log.Debug("Converting and de-duplicating array of file names to ListViewItems" + JsonConvert.SerializeObject(files));
            //TODO: deduplicate listview items
            List<ListViewItem> newItems = new List<ListViewItem>();
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                ListViewItem item = new ListViewItem(fileName);
                item.Tag = file;
                item.ToolTipText = file;
                newItems.Add(item);
            }
            log.Debug("Result of conversion is" + JsonConvert.SerializeObject(newItems.ToArray()));
            return newItems.ToArray();
        }


        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            listView1.Items.AddRange(getListItemsFromFiles(droppedFiles));
            log.Debug("Added the files from drag&drop: " + JsonConvert.SerializeObject(droppedFiles));
        }

        private void listView1_DragEnter_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            log.Info("Cleared list of files per user request");
        }

        private string getSelectedMandantName()
        {
            //TODO: Fix this and add logging
            log.Debug("Getting the sleected mandant name");
            if (comboBox1.SelectedItem != null)
            {
                string selectedMandant = comboBox1.SelectedItem.ToString();
                log.Debug($"Selected mandant name is {selectedMandant}");
                return selectedMandant;
            }
            log.Error("Could not get selected mandant name");
            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count < 1)
            {
                log.Warn("User clicked on 'Encrypt' but no files were selected");
                return;  // do nothing if no files are selected
            }
            button3.Enabled = false;
            if (!checkFileSizes())
            {
                log.Error($"User selected files which exceed combined file size limit of {MaxInputMBytes} MB, showing error message");
                MessageBox.Show(String.Format(rm.GetString("ErrorBox_InputTooLarge"),MaxInputMBytes),
                    rm.GetString("ErrorBox_Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                button3.Enabled = true;
                return;
            }
            DateTime localDate = DateTime.Now;
            string dateString = localDate.ToString("yyyyMMdd");
            saveFileDialog1.FileName = getSelectedMandantName() + "_" + dateString + ".zip";
            log.Debug("Suggested target filename " + saveFileDialog1.FileName);
            this.outFile = null;
            saveFileDialog1.ShowDialog();
            if (this.outFile == null)
            {
                log.Error($"User did not chose a target file, returning and doing nothing");
                button3.Enabled = true;
                return;
            }
            string password = getDecryptedPassword();
            // TODO: pack and encrypt
            log.Info("Finished with file encryption process");
        }

        private bool checkFileSizes()
        {
            double combinedSize = 0;
            log.Debug("Calculating combined filesize of selected files");
            foreach (ListViewItem f in listView1.Items)
            {
                combinedSize += new System.IO.FileInfo((String)f.Tag).Length;
            }
            log.Info($"Combined filezise of all selected files is {combinedSize} bytes. The maximimum allowed is {MaxInputMBytes} MB");
            if (combinedSize > (MaxInputMBytes * 1024 * 1024))
            {
                log.Error("Combined file size is more than allowed, returning false");
                return false;
            }
            return true;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.outFile = saveFileDialog1.FileName;
        }

        private string getDecryptedPassword()
        {
            string password = null;
            Mandant m = (Mandant) comboBox1.SelectedItem;
            log.Info("Retrieving decrypted password for encryption for mandant: " + JsonConvert.SerializeObject(m));
            password = this.server.getDecryptedPassword(m.id);
            log.Debug($"Retrieved plaintext password from server with length {password.Length}");
            return password;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            log.Debug("Main form loading");
            this.getMandantsDict();           
        }

        private bool getMandantsDict()
        {
            this.comboBox1.Text = null;
            this.comboBox1.Items.Clear();
            try
            {
                log.Info("Retrieving list of mandants from server");
                List<Mandant> mlist = this.server.getActiveMandantList();
                log.Debug($"Mandants retrieved from server: " + JsonConvert.SerializeObject(mlist));
                foreach (Mandant m in mlist)
                {
                    this.comboBox1.Items.Add(m);
                }
                log.Info($"Retrieved {mlist.Count} mandants and added them to the list");
            } catch (Exception e)
            {
                log.Error("Error while getting mandants from server");
                labelStatus.Text = rm.GetString("Status_ConnectionError") + " " + appSettings.getServerUrl();
                labelStatus.ForeColor = Color.Red;
                StringBuilder tt = new StringBuilder(e.Message).Append("\n");
                tt.Append(e.StackTrace);
                if (e.InnerException != null)
                {
                    tt.Append("\nInner Exception: ");
                    tt.Append(e.InnerException.Message).Append("\n");
                    tt.Append(e.InnerException.StackTrace);
                }
                tt.Append("\n").Append(rm.GetString("Status_Retry"));
                toolTip1.SetToolTip(labelStatus, tt.ToString());
                log.Error("Error details: " + tt.ToString());
                return false;
            }

            if (this.comboBox1.Items.Count > this.settingsForm.loadLastMandantIndex())
            {
                log.Debug($"Using selected mandant index from settings");
                this.comboBox1.SelectedIndex = this.settingsForm.loadLastMandantIndex();
            } else
            {
                log.Debug($"Setting selected mandant index to 0");
                this.comboBox1.SelectedIndex = 0;
            }

            log.Debug($"Selected mandant with index {this.comboBox1.SelectedIndex}");

            labelStatus.Text = rm.GetString("Status_Connected") + " " + this.appSettings.getServerUrl();
            labelStatus.ForeColor = Color.Green;
            toolTip1.SetToolTip(labelStatus, this.server.serviceUrl);
            log.Info("Fininshed getting the mandant list from the server and adding it to the drop down");
            return true;
        }

        private void labelStatus_Click(object sender, EventArgs e)
        {
            if(labelStatus.ForeColor == Color.Red)
            {
                log.Debug($"Re-trying server connection as user clicked on status label");
                getMandantsDict();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.settingsForm.saveLastMandantIndex(comboBox1.SelectedIndex);
            Mandant m = (Mandant) comboBox1.SelectedItem;
            log.Debug($"Mandant latest_password_created: [{m.latest_password_created}], now is [{DateTime.Now}]");
            int passwordAge = (DateTime.Now - m.latest_password_created).Days;
            string ptext = String.Format(rm.GetString("Password_Age"),passwordAge);
            label2.Text = ptext;
            log.Debug($"Updated password age field with {passwordAge} days");
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closing app");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
