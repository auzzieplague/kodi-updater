using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace kodiupdates
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static void UnZip(string zipFile, string folderPath)
        {
            if (!File.Exists(zipFile))
                throw new FileNotFoundException();

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Shell32.Shell objShell = new Shell32.Shell();
            Shell32.Folder destinationFolder = objShell.NameSpace(folderPath);
            Shell32.Folder sourceFile = objShell.NameSpace(zipFile);

            try
            {
                foreach (var file in sourceFile.Items())
                {
                    destinationFolder.CopyHere(file, 4 | 16);
                }
            }
            catch
            {
                throw;
            }
        }

        private string GetLatestVersion(string inputFileUrl)
        {
            try
            {
                WebClient client = new WebClient();
                Uri uri = new Uri(inputFileUrl);
                client.DownloadFile(uri, "scanfile.txt");
                string[] lines = System.IO.File.ReadAllLines(@"scanfile.txt");
                string filename = ""; // lines[lines.Length - 3].Split('"')[1];
                int versions = 0;
                foreach (string line in lines)
                {
                    if (line.Contains(".zip"))
                    {
                        filename = line.Split('"')[1];
                        versions++;
                    }
                }
                //MessageBox.Show(filename);
                uri = new Uri(inputFileUrl+ filename);
                client.DownloadFile(uri, @"downloads\"+ filename);
                return filename+":"+versions.ToString();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("cannot process "+ inputFileUrl, "Problem reading web url", MessageBoxButtons.OK);
                errorlist.Items.Add("cannot process " + inputFileUrl);
                return "";
            }
        }


        

        private void Form1_Load(object sender, EventArgs e)
        {
            //get correct downloads file
            string updatefilename = "downloads.txt";
            if (!File.Exists("downloads.txt"))
            {
                updatefilename = "kodiupdates.txt";
                WebClient client = new WebClient();
                Uri uri = new Uri(@"http://www.michaelwhinfrey.com/projects/kodiupdates.txt");
                try {
                    client.DownloadFile(uri, updatefilename);
                }
                catch
                {
       
                }

                if (!File.Exists(updatefilename))
                {
                    try
                    {
                        uri = new Uri(@"http://www.computertown.com.au/webupdate/kodiupdates.txt");
                        client.DownloadFile(uri, updatefilename);
                    }
                    catch
                    {
                        MessageBox.Show("update file not found, check internet connection", "cant read file", MessageBoxButtons.OK);
                    }
                }
            }

            //read info into listbox

            try {

                string[] lines = System.IO.File.ReadAllLines(updatefilename);
                foreach (string line in lines)
                {
                    listBox1.Items.Add(line);
                }
            }
            catch
            {
                MessageBox.Show("filenot found, try running as administrator", "cant read file", MessageBoxButtons.OK);
            }

            if (!Directory.Exists("downloads"))
            {
                Directory.CreateDirectory("downloads");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = listBox1.Items.Count;
            progressBar1.Value = 0;
            label2.Text = "Downloading ...";
            //check repos for latest versions
            foreach (string item in listBox1.Items)
            {
                progressBar1.Value ++;
                string filename=GetLatestVersion(item.ToString());
                if (filename != "")
                {
                    filelist.Items.Add(filename.Split(':')[0]);
                    versions.Items.Add(filename.Split(':')[1]);
                }
                this.Update();
            }


            progressBar1.Minimum = 0;
            progressBar1.Maximum = filelist.Items.Count;
            progressBar1.Value = 0;
            label2.Text = "Extracting ...";
            

            string outputpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\kodi\addons";
            if (!Directory.Exists(outputpath))
            {
                outputpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\kodi\userdata\addon_data\";
            }

            foreach (string item in filelist.Items)
            {
                progressBar1.Value++;
                try {
                    UnZip(AppDomain.CurrentDomain.BaseDirectory + @"downloads\" + item.ToString(), outputpath);
                }
                catch
                {
                    label2.Text = "Extraction failed!";
                    progressBar1.Value = 0;
                    MessageBox.Show("was unable to extract " +item.ToString()+" to "+outputpath, "Having Trouble!", MessageBoxButtons.OK);
                    return;
                }
                this.Update();
            }

            //Windows Start -type %APPDATA%\kodi\userdata - press < Enter >
            label2.Text = "Complete!";

            if (errorlist.Items.Count > 0)
            {
                this.Height = 285;
                MessageBox.Show("updates complete, some errors encountered!", "All done", MessageBoxButtons.OK);
            }
            else  MessageBox.Show("updates complete", "All done", MessageBoxButtons.OK);

        }

        private void filelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            versions.SelectedIndex = filelist.SelectedIndex;
        }
    }
}
