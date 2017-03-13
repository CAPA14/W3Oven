using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;

namespace CapaOven
{
    public partial class Form1 : Form
    {

        IEnumerable<string> assets;
        IEnumerable<string> expassets;

        string noWccPath = "Browse the wcc_lite.exe path";
        string noUncookedPath = "Browse the Uncooked game directory";
        string noModDirectory = "Browse the Mod Directory(one level above the Modded folder)";
        string noModName = "Type the mod name";
        string noUncookTo = "Browse to where you want to Uncook";
        string noUncookFrom = "Browse the path to 'content' folder of the Game or Mod/DLC";
        string noExpDir = "Browse the folder where the assets you want to Export are";
        string noExpOutDir = "Browse the path to folder to where you want to export";
        string noComboModname = "Type a Mod Name or Select a Mod";
        string noWorkspace = "Browse the folder where you want or stores your mods";



        public Form1()
        {

            InitializeComponent();

            string test;
            test = ConfigurationManager.AppSettings["UncookedPath"];
            if (test != null) { txtUncooked.Text = test; SetFontStyle(txtUncooked, "White", false); }

            test = ConfigurationManager.AppSettings["WccPath"];
            if (test != null) { txtWcc.Text = test; SetFontStyle(txtWcc, "White", false); }
                        
            test = ConfigurationManager.AppSettings["WorkspacePath"];
            if (test != null) {
                txtWorkspace.Text = test; SetFontStyle(txtWorkspace, "White", false);
                RefreshComboMods();
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxWriter(outBox));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            openFileDialog1.Filter = "wcc_lite.exe|wcc_lite.exe";
            openFileDialog1.Title = "Select wcc_lite.exe";
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            txtWcc.Text = openFileDialog1.FileName;

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            config.AppSettings.Settings.Remove("WccPath");
            config.AppSettings.Settings.Add("WccPath", txtWcc.Text);

            config.Save(ConfigurationSaveMode.Modified);

            SetFontStyle(txtWcc, "White", false);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int i = 0;
            string strOut;
            string cmdImportArgs = "/c ";

            if (txtWcc.Text == noWccPath) { Console.WriteLine("ERROR --- Select the Wcc_lite.exe path");
                return;
            }

            if (txtUncooked.Text == noUncookedPath) { Console.WriteLine("ERROR --- Select the 'Uncooked Directory', folder where you uncooked the whole game");
                return;
            }

            if (txtModFolder.Text == noModDirectory)
            {
                Console.WriteLine("ERROR --- Mod Directory is missing, select the mod folder");
                return;
            }

            if (txtModName.Text == noModName)
            {
                Console.WriteLine("ERROR --- Mod Name is missing, type a mod name");
                return;
            }

            if (assets != null && !checkBox3.Checked)
            {

                //Prepare outStrings

                foreach (string str in assets)
                {

                    if (!checkBox1.Checked || (dataGridView1.Rows[i].Selected))
                    {

                        strOut = str;

                        strOut = strOut.Replace("modded", "uncooked");
                        strOut = strOut.Replace(".tga", ".xbm");
                        strOut = strOut.Replace(".png", ".xbm");
                        strOut = strOut.Replace(".jpg", ".xbm");
                        strOut = strOut.Replace(".dds", ".xbm");
                        strOut = strOut.Replace(".fbx", ".w2mesh");


                        //Prepare Arguments of Import
                        cmdImportArgs = cmdImportArgs + "wcc_lite.exe"
                                        + " import " + "-depot=" + "\"" + txtUncooked.Text + "\""
                                         + " -file=" + "\"" + str + "\"" +
                                        " -out=" + "\"" + strOut + "\"";

                        if (dataGridView1.Rows[i].Cells["texturetypeColumn"].Value != null)
                        {

                            cmdImportArgs = cmdImportArgs + " -texturegroup=" + dataGridView1.Rows[i].Cells["texturetypeColumn"].Value;
                        }

                        cmdImportArgs = cmdImportArgs + " & ";
                    }

                    i++;

                } //End of Import Args    
            }
            else if (checkBox3.Checked) {
                Console.WriteLine("\n\nINFO --- Skipping Import!");

            }
            else {

                Console.WriteLine("ERROR --- Assets not loaded! To load assets select a valid 'Mod Diretory'");
                return;
            }

            //Cook Arguments
            cmdImportArgs = cmdImportArgs + "wcc_lite.exe cook -platform=pc -mod=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -outdir=" + "\"" + txtModFolder.Text + @"\cooked" + "\"";

            //Build Cache Arguments
            cmdImportArgs = cmdImportArgs + " & wcc_lite.exe buildcache textures -platform=pc -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -db=" + "\"" + txtModFolder.Text + @"\cooked\cook.db" + "\"" + " -out=" + "\"" + txtModFolder.Text + @"\packed\" + txtModName.Text + @"\content\texture.cache" + "\"";

            if (!checkBox2.Checked)
            {
                //Pack Arguments
                cmdImportArgs = cmdImportArgs + " & wcc_lite pack -dir=" + "\"" + txtModFolder.Text + @"\Cooked" + "\"" + " -outdir=" + "\"" + txtModFolder.Text + @"\Packed\" + txtModName.Text + @"\content" + "\"";

                //Generate Metadata Arguments
                cmdImportArgs = cmdImportArgs + " & wcc_lite metadatastore -path=" + "\"" + txtModFolder.Text + @"\Packed\" + txtModName.Text + @"\content" + "\"";
            }
            Console.WriteLine("\nINFO --- Executing Lines ---\n");
            Console.WriteLine(cmdImportArgs.Replace("&", "\r\n"));
            Console.WriteLine("\nINFO --- Started Cooking Mod ---\n");


            //Call the fuking CMD
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = (txtWcc.Text).Replace("wcc_lite.exe", "");


            p.StartInfo.Arguments = cmdImportArgs;

            p.OutputDataReceived += new DataReceivedEventHandler(
                (s, ev) =>
                {
                    Console.WriteLine(ev.Data);
                }
            );
            p.ErrorDataReceived += new DataReceivedEventHandler((s, ev) => { Console.WriteLine(ev.Data); });

            p.Start();
            p.BeginOutputReadLine();
            p.EnableRaisingEvents = true;
            p.Exited += (s, ev) => {
                Console.WriteLine("INFO --- Done!"); };


        }

        private void button3_ClickAlt(object sender, EventArgs e)
        {
            int i = 0;
            string strOut;

            Process[] processes = null;
            string[] args = null;

            if (txtWcc.Text == noWccPath)
            {
                Console.WriteLine("\nERROR --- Select the Wcc_lite.exe path\n");
                return;
            }

            if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("\nERROR --- Select the 'Uncooked Directory', folder where you uncooked the whole game\n");
                return;
            }

            if (txtModFolder.Text == noModDirectory)
            {
                Console.WriteLine("\nERROR --- Mod Directory is missing, select the mod folder\n");
                return;
            }

            if (txtModName.Text == noModName)
            {
                Console.WriteLine("\nERROR --- Mod Name is missing, type a mod name\n");
                return;
            }

            if (assets != null && !checkBox3.Checked)
            {
                Array.Resize(ref processes, assets.Count() + 4);
                Array.Resize(ref args, assets.Count() + 4);

                //Prepare outStrings
                foreach (string str in assets)
                {

                    if (!checkBox1.Checked || (dataGridView1.Rows[i].Selected))
                    {

                        strOut = str;

                        strOut = strOut.Replace("modded", "uncooked");
                        strOut = strOut.Replace(".tga", ".xbm");
                        strOut = strOut.Replace(".png", ".xbm");
                        strOut = strOut.Replace(".jpg", ".xbm");
                        strOut = strOut.Replace(".dds", ".xbm");
                        strOut = strOut.Replace(".fbx", ".w2mesh");


                        //Prepare Arguments of Import
                        args[i] = args[i]
                                        + " import " + "-depot=" + "\"" + txtUncooked.Text + "\""
                                         + " -file=" + "\"" + str + "\"" +
                                        " -out=" + "\"" + strOut + "\"";

                        if (dataGridView1.Rows[i].Cells["texturetypeColumn"].Value != null)
                        {

                            args[i] = args[i] + " -texturegroup=" + dataGridView1.Rows[i].Cells["texturetypeColumn"].Value;
                        }

                    }

                    i++;

                } //End of Import Args  

                //Call the import chain!


            }
            else if (checkBox3.Checked)
            {
                Console.WriteLine("\n\nINFO --- Skipping Import!\n");
                Array.Resize(ref processes, 4);
                Array.Resize(ref args, 4);
            }
            else
            {

                Console.WriteLine("\nERROR --- Assets not loaded! To load assets select a valid 'Mod Diretory'\n");
                return;
            }

            //Cook Arguments
            args[i] = args[i] + " cook -platform=pc -mod=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -outdir=" + "\"" + txtModFolder.Text + @"\cooked" + "\"";

            //Build Cache Arguments
            i++;
            args[i] = args[i] + " buildcache textures -platform=pc -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -db=" + "\"" + txtModFolder.Text + @"\cooked\cook.db" + "\"" + " -out=" + "\"" + txtModFolder.Text + @"\packed\" + txtModName.Text + @"\content\texture.cache" + "\"";

            if (!checkBox2.Checked)
            {
                //Pack Arguments
                i++;
                args[i] = args[i] + " pack -dir=" + "\"" + txtModFolder.Text + @"\Cooked" + "\"" + " -outdir=" + "\"" + txtModFolder.Text + @"\Packed\" + txtModName.Text + @"\content" + "\"";
                i++;
                //Generate Metadata Arguments
                args[i] = args[i] + " metadatastore -path=" + "\"" + txtModFolder.Text + @"\Packed\" + txtModName.Text + @"\content" + "\"";
            }
            Console.WriteLine("\r\n\nINFO --- Executing Lines ---\n\n");
            foreach (string st in args)
            {
                if (st != null)
                    Console.WriteLine("wcc_lite.exe" + st);
            }
            Console.WriteLine("\r\n\nINFO --- Started Cooking Mod ---\n\n");


            RunProcessesSequence(processes, args, 0, "Build mod");

        }


        private void button2_Click(object sender, EventArgs e)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtUncooked.Text = dialog.FileName;

                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                config.AppSettings.Settings.Remove("UncookedPath");
                config.AppSettings.Settings.Add("UncookedPath", txtUncooked.Text);

                config.Save(ConfigurationSaveMode.Modified);

                SetFontStyle(txtUncooked, "White", false);
            }

        }



        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string fullPath, filename, texgroup;
            texgroup = null;
            int rowId;//Needed for the datagrid filling
            DataGridViewRow row;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtModFolder.Text = dialog.FileName;
                fullPath = txtModFolder.Text + @"\modded";

                if (Directory.Exists(fullPath)) {
                    Console.WriteLine("--- Loading Assets from \"Modded\" folder ---");

                    //Gets recursive the files with specific extensions
                    var files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Where(s =>
                    s.EndsWith(".tga", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".jp*", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".fbx", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".dds", StringComparison.CurrentCultureIgnoreCase)
                    );

                    assets = files; //Stores files to a IEnumerable<string>

                    //Clears the table before filling
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();

                    foreach (string str in files)
                    {
                        rowId = dataGridView1.Rows.Add();

                        // Adds a new row
                        row = dataGridView1.Rows[rowId];

                        filename = Path.GetFileNameWithoutExtension(str);
                        if (filename.EndsWith("_d01") || filename.EndsWith("_d") || filename.EndsWith("_a01")){
                            texgroup = "WorldDiffuse";
                        }
                        else if (filename.EndsWith("_n01") || filename.EndsWith("_n"))
                        {
                            texgroup = "NormalmapGloss";
                        }
                        else if (filename.EndsWith("_s01") || filename.EndsWith("_s") || filename.EndsWith("_s02"))
                        {
                            texgroup = "WorldSpecular";
                        }
                        else if (filename.EndsWith("_e01"))
                        {
                            texgroup = "WorldEmissive";
                        }
                        else
                        {
                            texgroup = null;
                        }

                        // Add the data
                        row.Cells["assetsColumn"].Value = str.Replace(fullPath, "");
                        row.Cells["texturetypeColumn"].Value = texgroup;


                        Console.WriteLine(str);

                    }
                    Console.WriteLine("--- Assets Loaded from Modded folder ---");
                    SetFontStyle(txtModFolder, "White", false);
                }
                else { Console.WriteLine("ERROR --- The specified 'Mod Directory' does not contain a \"Modded\" folder"); }
            }
        }

       private void RefreshCookAssets()
        {
            string fullPath, filename, texgroup;
            texgroup = null;
            int rowId;//Needed for the datagrid filling
            DataGridViewRow row;

                            
                fullPath = txtModFolder.Text + @"\modded";

                if (Directory.Exists(fullPath))
                {
                    Console.WriteLine("--- Loading Assets from \"Modded\" folder ---");

                    //Gets recursive the files with specific extensions
                    var files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Where(s =>
                    s.EndsWith(".tga", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".jp*", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".fbx", StringComparison.CurrentCultureIgnoreCase)
                    || s.EndsWith(".dds", StringComparison.CurrentCultureIgnoreCase)
                    );

                    assets = files; //Stores files to a IEnumerable<string>

                    //Clears the table before filling
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();

                    foreach (string str in files)
                    {
                        rowId = dataGridView1.Rows.Add();

                        // Adds a new row
                        row = dataGridView1.Rows[rowId];

                        filename = Path.GetFileNameWithoutExtension(str);
                    if (filename.EndsWith("_d01") || filename.EndsWith("_d") || filename.EndsWith("_a01"))
                    {
                        texgroup = "WorldDiffuse";
                    }
                    else if (filename.EndsWith("_n01") || filename.EndsWith("_n"))
                    {
                        texgroup = "NormalmapGloss";
                    }
                    else if (filename.EndsWith("_s01") || filename.EndsWith("_s") || filename.EndsWith("_s02"))
                    {
                        texgroup = "WorldSpecular";
                    }
                    else if (filename.EndsWith("_e01"))
                    {
                        texgroup = "WorldEmissive";
                    }
                    else
                    {
                        texgroup = null;
                    }

                    // Add the data
                    row.Cells["assetsColumn"].Value = str.Replace(fullPath, "");
                        row.Cells["texturetypeColumn"].Value = texgroup;


                        Console.WriteLine(str);

                    }
                    Console.WriteLine("--- Assets Loaded from Modded folder ---");
                    SetFontStyle(txtModFolder, "White", false);
                    SetFontStyle(txtModName, "White", false);
                }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            //WindowState = FormWindowState.Minimized;
            Close();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtUncookTo.Text = dialog.FileName;
                SetFontStyle(txtUncookTo, "White", false);
            }
        }

        private void modname_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtModName.Text == noModName) {

                txtModName.Text = "";
                SetFontStyle(txtModName, "White", false);
            }

        }
        private void modname_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtModName.Text == "") {

                txtModName.Text = noModName;
                SetFontStyle(txtModName, "Gray", true);

            }

        }


        private void modDir_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtModFolder.Text == noModDirectory)
            {

                txtModFolder.Text = "";
                SetFontStyle(txtModFolder, "White", false);
            }

        }
        private void modDir_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtModFolder.Text == "")
            {

                txtModFolder.Text = noModDirectory;
                SetFontStyle(txtModFolder, "Gray", true);

            }

        }

        private void txtExpDir_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtExpDir.Text == noExpDir)
            {

                txtExpDir.Text = "";
                SetFontStyle(txtExpDir, "White", false);
            }

        }
        private void txtExpDir_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtExpDir.Text == "")
            {

                txtExpDir.Text = noExpDir;
                SetFontStyle(txtExpDir, "Gray", true);

            }

        }

        private void txtExpOutDir_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtExpOutDir.Text == noExpOutDir)
            {

                txtExpOutDir.Text = "";
                SetFontStyle(txtExpOutDir, "White", false);
            }

        }
        private void txtExpOutDir_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtExpOutDir.Text == "")
            {

                txtExpOutDir.Text = noExpOutDir;
                SetFontStyle(txtExpOutDir, "Gray", true);

            }

        }

        private void txtUncookFrom_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtUncookFrom.Text == noUncookFrom)
            {

                txtUncookFrom.Text = "";
                SetFontStyle(txtUncookFrom, "White", false);
            }
        }
        private void txtUncookFrom_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtUncookFrom.Text == "")
            {

                txtUncookFrom.Text = noUncookFrom;
                SetFontStyle(txtUncookFrom, "Gray", true);

            }
        }

        private void txtUncookTo_OnFocusEnter(object sender, EventArgs e)
        {
            if (txtUncookTo.Text == noUncookTo)
            {

                txtUncookTo.Text = "";
                SetFontStyle(txtUncookTo, "White", false);
            }

        }
        private void txtUncookTo_OnFocusLeave(object sender, EventArgs e)
        {
            if (txtUncookTo.Text == "")
            {

                txtUncookTo.Text = noUncookTo;
                SetFontStyle(txtUncookTo, "Gray", true);

            }

        }

        private void comboModname_OnFocusEnter(object sender, EventArgs e)
        {
            if (comboModname.Text == noComboModname)
            {

                comboModname.Text = "";
                SetFontStyleCombo(comboModname, "White", false);
            }

        }
        private void comboModname_OnFocusLeave(object sender, EventArgs e)
        {
            if (comboModname.Text == "")
            {

                comboModname.Text = noComboModname;
                SetFontStyleCombo(comboModname, "Gray", true);

            }

        }

        public void SetFontStyle(TextBox txtbox, string color, bool setItalic)
        {
            txtbox.ForeColor = Color.FromName(color);
            if (setItalic)
            {
                txtbox.Font = new Font(txtbox.Font.Name, txtbox.Font.Size, FontStyle.Italic);

            }
            else
            {
                txtbox.Font = new Font(txtbox.Font.Name, txtbox.Font.Size, FontStyle.Regular);
            }
        }

        public void SetFontStyleCombo(ComboBox combo, string color, bool setItalic)
        {
            combo.ForeColor = Color.FromName(color);
            if (setItalic)
            {
                combo.Font = new Font(combo.Font.Name, combo.Font.Size, FontStyle.Italic);

            }
            else
            {
                combo.Font = new Font(combo.Font.Name, combo.Font.Size, FontStyle.Regular);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string imgformat;
            string cmdArgs;

            if (txtUncookFrom.Text == noUncookFrom)
            {
                Console.WriteLine("ERROR --- 'Uncook From' path is missing!");
                return;
            }

            if (txtUncookTo.Text == noUncookTo)
            {
                Console.WriteLine("ERROR --- 'Uncook To' path is missing!");
                return;
            }

            //Generate Metadata Arguments
            cmdArgs = "/c wcc_lite uncook -indir=" + "\"" + txtUncookFrom.Text + "\"" + " -outdir=" + "\"" + txtUncookTo.Text + "\"";

            //-imgfmt = combobox1
            imgformat = comboBox1.Text;
            if (imgformat == null) { imgformat = "tga"; }

            cmdArgs = cmdArgs + " -imgfmt=" + imgformat + " -skiperrors";

            Console.WriteLine("\nINFO --- Executing Lines ---\r\n");
            Console.WriteLine("\n" + cmdArgs + "\n");
            Console.WriteLine("\r\nINFO --- Started Uncooking ---\n");


            //Call the fuking CMD
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = (txtWcc.Text).Replace("wcc_lite.exe", "");

            p.StartInfo.Arguments = cmdArgs; //Arguments

            p.OutputDataReceived += new DataReceivedEventHandler(
                (s, ev) =>
                {
                    Console.WriteLine(ev.Data);
                }
            );
            p.ErrorDataReceived += new DataReceivedEventHandler((s, ev) => { Console.WriteLine(ev.Data); });

            p.Start();
            p.BeginOutputReadLine();
            p.EnableRaisingEvents = true;
            p.Exited += (s, ev) => {
                Console.WriteLine("INFO --- Done Uncooking!"); };

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (txtUncooked.Text != noUncookedPath)
                txtUncookTo.Text = txtUncooked.Text;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtUncookFrom.Text = dialog.FileName;
                SetFontStyle(txtUncookFrom, "White", false);
            }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {

            int rowId;//Needed for the datagrid filling
            IEnumerable<string> files = null;
            DataGridViewRow row;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (txtUncooked.Text != noUncookedPath) { dialog.InitialDirectory = txtUncooked.Text; }
            if (txtExpDir.Text != noExpDir) { dialog.InitialDirectory = txtExpDir.Text; }
            dialog.IsFolderPicker = true;
            dialog.ShowPlacesList = true;
            dialog.ShowHiddenItems = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtExpDir.Text = dialog.FileName;

                Console.WriteLine("--- Loading Exportable Assets from selected folder ---");

                //Gets recursive the files with specific extensions
                switch (comboBox4.SelectedIndex) {

                    case 0:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".xbm", StringComparison.CurrentCultureIgnoreCase)
                        || s.EndsWith(".w2mesh", StringComparison.CurrentCultureIgnoreCase)

                        );
                        break;
                    case 1:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".xbm", StringComparison.CurrentCultureIgnoreCase)

                        );
                        break;
                    case 2:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".w2mesh", StringComparison.CurrentCultureIgnoreCase)

                        );

                        break;

                }


                expassets = files; //Stores files to a IEnumerable<string>

                //Clears the table before filling
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();

                foreach (string str in files)
                {
                    rowId = dataGridView2.Rows.Add();

                    // Adds a new row
                    row = dataGridView2.Rows[rowId];

                    // Add the data
                    row.Cells["expAssetsColumn"].Value = str.Replace(txtUncooked.Text, "");


                    Console.WriteLine(str);

                }
                Console.WriteLine("--- Exportable Assets Loaded ---");

                SetFontStyle(txtExpDir, "White", false);

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtExpOutDir.Text = dialog.FileName;

                SetFontStyle(txtExpOutDir, "White", false);
            }

        }

        /*private void button11_Click(object sender, EventArgs e)
        {
            int i = 0;
            string strOut;
            string cmdExpArgs = "/c ";

            if (txtWcc.Text == noWccPath)
            {
                Console.WriteLine("ERROR --- Select the Wcc_lite.exe path");
                return;
            }

            if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("ERROR --- Select the 'Uncooked Directory', folder where you uncooked the whole game");
                return;
            }

            if (txtExpDir.Text == noExpDir)
            {
                Console.WriteLine("ERROR --- 'Export Directory' is missing, select the folder");
                return;
            }

            if (txtExpOutDir.Text == noExpOutDir)
            {
                Console.WriteLine("ERROR --- 'Export Out Directory' is missing, select the folder");
                return;
            }

            if (expassets != null)
            {

                //Prepare outStrings

                foreach (string str in expassets)
                {

                    if (!checkBox4.Checked || dataGridView2.Rows[i].Selected)
                    {

                        strOut = str;

                        strOut = strOut.Replace(txtUncooked.Text, txtExpOutDir.Text);
                        strOut = strOut.Replace(".xbm", "." + comboBox3.Text);
                        strOut = strOut.Replace(".w2mesh", ".fbx");

                        //Prepare Arguments of Export wcc_lite export -depot= <dirpath> \Uncooked -file=<filepath> -out=< filepath >
                        cmdExpArgs = cmdExpArgs + "wcc_lite.exe"
                            + " export " + "-depot=" + "\"" + txtUncooked.Text + "\""
                            + " -file=" + "\"" + str.Replace(txtUncooked.Text+@"\", "") + "\""
                            + " -out=" + "\"" + strOut + "\"";
                      /*  if (str.EndsWith(".w2mesh"))
                        {
                            cmdExpArgs = cmdExpArgs + " -fbx=" + comboBox2.Text;
                        }                
                            

                        
                      cmdExpArgs = cmdExpArgs + " & ";
                    }

                    i++;

                } //End of Export Args    
            }
           
            else
            {
                Console.WriteLine("ERROR --- No Exportable Assets found in the selected folder!");
                return;
            }

          
            Console.WriteLine("\nINFO --- Executing Lines ---\n");
            Console.WriteLine(cmdExpArgs.Replace("&", "\r\n"));
            Console.WriteLine("\nINFO --- Started Exporting ---\n");


            //Call the fuking CMD
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = (txtWcc.Text).Replace("wcc_lite.exe", "");


            p.StartInfo.Arguments = cmdExpArgs;

            p.OutputDataReceived += new DataReceivedEventHandler(
                (s, ev) =>
                {
                    Console.WriteLine(ev.Data);
                }
            );
            p.ErrorDataReceived += new DataReceivedEventHandler((s, ev) => { Console.WriteLine(ev.Data); });

            p.Start();
            p.BeginOutputReadLine();
            p.EnableRaisingEvents = true;
            p.Exited += (s, ev) => {
                Console.WriteLine("INFO --- Export Done!");
            };
        }*/

        private void button12_Click(object sender, EventArgs e)
        {
            Process[] processes = null;
            string[] args = null;


            int i = 0;
            string strOut;


            if (txtWcc.Text == noWccPath)
            {
                Console.WriteLine("ERROR --- Select the Wcc_lite.exe path");
                return;
            }

            if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("ERROR --- Select the 'Uncooked Directory', folder where you uncooked the whole game");
                return;
            }

            if (txtExpDir.Text == noExpDir)
            {
                Console.WriteLine("ERROR --- 'Export Directory' is missing, select the folder");
                return;
            }

            if (txtExpOutDir.Text == noExpOutDir)
            {
                Console.WriteLine("ERROR --- 'Export Out Directory' is missing, select the folder");
                return;
            }

            if (expassets != null)
            {
                Array.Resize(ref processes, expassets.Count());
                Array.Resize(ref args, expassets.Count());
                //Prepare outStrings

                foreach (string str in expassets)
                {

                    if (!checkBox4.Checked || dataGridView2.Rows[i].Selected)
                    {

                        strOut = str;

                        strOut = strOut.Replace(txtUncooked.Text, txtExpOutDir.Text);
                        strOut = strOut.Replace(".xbm", "." + comboBox3.Text);
                        strOut = strOut.Replace(".w2mesh", ".fbx");

                        //Prepare Arguments of Export wcc_lite export -depot= <dirpath> \Uncooked -file=<filepath> -out=< filepath >
                        args[i] = args[i]
                            + " export " + "-depot=" + "\"" + txtUncooked.Text + "\""
                            + " -file=" + "\"" + str.Replace(txtUncooked.Text + @"\", "") + "\""
                            + " -out=" + "\"" + strOut + "\"";
                        if (str.EndsWith(".w2mesh"))
                        {
                            args[i] = args[i] + " -fbx=" + comboBox2.Text;
                        }

                    }

                    i++;

                } //End of Export Args
                Console.WriteLine("\nINFO --- Executing Lines ---\n\n");
                foreach (string st in args)
                {
                    if (st != null)
                        Console.WriteLine("wcc_lite.exe" + st);
                }
                Console.WriteLine("\n\nINFO --- Started Exporting ---\n");

                //Call the FUKING Processes in Sequence
                RunProcessesSequence(processes, args, 0, "Export");
            }
            else
            {
                Console.WriteLine("ERROR --- No Exportable Assets found in the selected folder!");
                return;
            }

        }

        public void RunProcessesSequence(Process[] procs, string[] args, int i, string op)
        {
            string src;
            string dest;

            if (args[i] == null)
            {
                if (i < procs.Count() - 1)
                {
                    i = i + 1;
                    RunProcessesSequence(procs, args, i, op);
                }
                else
                {
                    Console.WriteLine("\r\n\nINFO --- " + op + " Done!");

                    if (op == "Build mod" && checkBox8.Checked == true)
                    {


                        src = txtModFolder.Text + @"\packed\" + txtModName.Text;

                        dest = ConfigurationManager.AppSettings["ModsPath"];
                        dest = dest + @"\" + txtModName.Text;

                        if (checkBox5.Checked == true)
                        {
                            src = src + @"\content\texture.cache";
                            dest = dest + @"\content\texture.cache";

                            if (checkBox6.Checked == true)
                            {
                                dest = dest.Replace(@"The Witcher 3\mods", @"The Witcher 3\DLC");
                            }
                            if (File.Exists(dest))
                            {
                                File.Delete(dest);
                                File.Copy(src, dest);
                                Console.WriteLine("\r\n\nINFO --- Copied texture.cache to " + dest);
                            }
                            else if (!File.Exists(dest) && Directory.Exists(dest.Replace("texture.cache", "")))
                            {
                                File.Copy(src, dest);
                                Console.WriteLine("\r\n\nINFO --- Copied texture.cache to " + dest);
                            }
                            else
                            {
                                Console.WriteLine("\r\n\nERROR --- Specified Mod/DLC Name is not Installed");
                            }

                        }
                        else
                        {

                            CopyDir copyvar = new CopyDir();
                            copyvar.Copy(src, dest);
                            Console.WriteLine("\r\n\nINFO --- Mod Installed to " + dest);
                        }
                    }
                }
            }
            else
            {
                procs[i] = new Process();
                procs[i].StartInfo.RedirectStandardError = true;
                procs[i].StartInfo.RedirectStandardOutput = true;
                procs[i].StartInfo.UseShellExecute = false;
                procs[i].StartInfo.CreateNoWindow = true;
                procs[i].StartInfo.WorkingDirectory = (txtWcc.Text).Replace("wcc_lite.exe", "");
                procs[i].StartInfo.FileName = txtWcc.Text;



                procs[i].StartInfo.Arguments = args[i];

                procs[i].OutputDataReceived += new DataReceivedEventHandler(
                    (s, ev) =>
                    {
                        Console.WriteLine(ev.Data);
                    }
                );
                procs[i].ErrorDataReceived += new DataReceivedEventHandler((s, ev) => { Console.WriteLine(ev.Data); });

                procs[i].Start();
                procs[i].BeginOutputReadLine();
                procs[i].EnableRaisingEvents = true;
                procs[i].Exited += (s, ev) =>
                {
                    if (i < procs.Count() - 1)
                    {
                        i = i + 1;
                        RunProcessesSequence(procs, args, i, op);
                    }
                    else
                    {
                        Console.WriteLine("\r\n\nINFO --- " + op + " Done!");

                        if (op == "Build mod" && checkBox8.Checked == true)
                        {

                            src = txtModFolder.Text + @"\packed\" + txtModName.Text;

                            dest = ConfigurationManager.AppSettings["ModsPath"];
                            dest = dest + @"\" + txtModName.Text;

                            if (checkBox5.Checked == true)
                            {
                                src = src + @"\content\texture.cache";
                                dest = dest + @"\content\texture.cache";

                                if (checkBox6.Checked == true)
                                {
                                    src = src.Replace(@"The Witcher 3\mods", @"The Witcher 3\DLC");
                                }
                                if (File.Exists(dest))
                                {
                                    File.Delete(dest);
                                    File.Copy(src, dest);
                                    Console.WriteLine("\r\n\nINFO --- Copied texture.cache to " + dest);
                                }
                                else if (!File.Exists(dest) && Directory.Exists(dest.Replace("texture.cache", "")))
                                {
                                    File.Copy(src, dest);
                                    Console.WriteLine("\r\n\nINFO --- Copied texture.cache to " + dest);
                                }
                                else
                                {
                                    Console.WriteLine("\r\n\nERROR --- Specified Mod/DLC Name is not Installed");
                                }

                            }
                            else
                            {

                                CopyDir copyvar = new CopyDir();
                                copyvar.Copy(src, dest);
                                Console.WriteLine("\r\n\nINFO --- Mod Installed to " + dest);
                            }
                        }
                    }
                };
            }
        }

        private void checkBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (expassets != null)
            {
                int rowId;//Needed for the datagrid filling
                IEnumerable<string> files = null;
                DataGridViewRow row;

                Console.WriteLine("--- Refreshing Exportable Assets ---");

                //Gets recursive the files with specific extensions
                switch (comboBox4.SelectedIndex)
                {

                    case 0:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".xbm", StringComparison.CurrentCultureIgnoreCase)
                        || s.EndsWith(".w2mesh", StringComparison.CurrentCultureIgnoreCase)

                        );
                        break;
                    case 1:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".xbm", StringComparison.CurrentCultureIgnoreCase)

                        );
                        break;
                    case 2:
                        files = Directory.EnumerateFiles(txtExpDir.Text, "*.*", SearchOption.AllDirectories)
                        .Where(s =>
                        s.EndsWith(".w2mesh", StringComparison.CurrentCultureIgnoreCase)

                        );

                        break;

                }


                expassets = files; //Stores files to a IEnumerable<string>

                //Clears the table before filling
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();

                foreach (string str in files)
                {
                    rowId = dataGridView2.Rows.Add();

                    // Adds a new row
                    row = dataGridView2.Rows[rowId];

                    // Add the data
                    row.Cells["expAssetsColumn"].Value = str.Replace(txtUncooked.Text, "");

                    Console.WriteLine(str);
                }
                Console.WriteLine("--- Exportable Assets Refreshed ---");
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            string test;
            test = ConfigurationManager.AppSettings["ModsPath"];

            if (test == null) {
                Console.WriteLine(@"--- Select your 'The Witcher 3\Mods' folder ---");
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.Title = @"Select your 'The Witcher 3\Mods' folder";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                    config.AppSettings.Settings.Remove("ModsPath");
                    config.AppSettings.Settings.Add("ModsPath", dialog.FileName);

                    config.Save(ConfigurationSaveMode.Modified);
                    Console.WriteLine(@"INFO --- Mods folder path Saved! ---");
                    checkBox5.Enabled = true;

                }
                else
                {
                    checkBox8.Checked = false;
                    checkBox5.Enabled = false;

                }
            }

            if (checkBox8.Checked == true)
            {
                checkBox5.Enabled = true;

            }
            else
            {
                checkBox5.Enabled = false;
                checkBox5.Checked = false;

            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                checkBox6.Enabled = true;
            }
            else
            {
                checkBox6.Enabled = false;
                checkBox6.Checked = false;
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtWorkspace.Text = dialog.FileName;

                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                config.AppSettings.Settings.Remove("WorkspacePath");
                config.AppSettings.Settings.Add("WorkspacePath", txtWorkspace.Text);

                config.Save(ConfigurationSaveMode.Modified);

                SetFontStyle(txtWorkspace, "White", false);

                RefreshComboMods();
            }
        }

        private void RefreshComboMods()
        {
            string item;
            string[] directories = null;
            directories = Directory.GetDirectories(txtWorkspace.Text);
            comboModname.Items.Clear();
            foreach (string d in directories)
            {
                if (Directory.Exists(d + @"\modded"))
                {
                    item = d.Replace(txtWorkspace.Text + @"\", "");
                    comboModname.Items.Add(item);
                }

            }
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            IEnumerable<string> filenames;
            filenames = null;
            DialogResult result;


            if (comboModname.Text == noComboModname)
            {
                Console.WriteLine("\nERROR --- Mod name not Specified! ---");
            }
            else if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("\nERROR --- Uncooked folder needs to be set! ---");
            }
            else if (txtWorkspace.Text == noWorkspace)
            {
                Console.WriteLine("\nERROR --- No Workspace was set! ---");
            }
            else
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = false;
                dialog.Multiselect = true;
                dialog.InitialDirectory = txtUncooked.Text;

                string moddir, outfile, outdir;
                moddir = txtWorkspace.Text +@"\" + comboModname.Text + @"\modded";
                
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {

                    filenames = dialog.FileNames;

                                
                    foreach (string asset in filenames)
                    {
                    
                        if (File.Exists(asset.Replace(txtUncooked.Text, moddir)))
                        {
                             result = MessageBox.Show("\n\nFile " + asset + " already exist. \n\nWould you like to replace the file?" , "Warning!",MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                File.Delete(asset.Replace(txtUncooked.Text, moddir));
                                File.Copy(asset, asset.Replace(txtUncooked.Text, moddir));
                                Console.WriteLine("\nFile added - " + asset.Replace(txtUncooked.Text, moddir));
                            }
                        
                        }
                        else
                        {
                            outfile = Path.GetFileName(asset);
                            outdir = asset.Replace(txtUncooked.Text, moddir);
                            outdir = outdir.Replace(outfile,"");
                            if (!Directory.Exists(moddir))
                            {
                                Console.WriteLine("\n[INFO] --- Mod Created: " + comboModname.Text);
                            }
                            Directory.CreateDirectory(outdir);
                            File.Copy(asset, asset.Replace(txtUncooked.Text, moddir));
                            Console.WriteLine("\nFile added - " + asset.Replace(txtUncooked.Text, moddir));

                            RefreshComboMods();
                        }
                    }
                }
            }
        }

        public class TextBoxWriter : TextWriter
        {
            TextBox _output;

            public TextBoxWriter(TextBox output)
            {
                _output = output;
            }

            public override void WriteLine(string value)
            {
                Write(value + System.Console.Out.NewLine);
            }

            public override void Write(string value)
            {
                if (_output.InvokeRequired)
                {
                    _output.BeginInvoke((Action<string>)Write, value);
                }
                else {
                    _output.AppendText(value);
                }
            }

            public override void Write(char value)
            {
                Write(value.ToString());
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        class CopyDir
        {
            public void Copy(string sourceDirectory, string targetDirectory)
            {
                DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
                DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

                CopyAll(diSource, diTarget);
            }

            public void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                Directory.CreateDirectory(target.FullName);

                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    Console.WriteLine(@"Copied {0}\{1}", target.FullName, fi.Name);
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }

            // Output will vary based on the contents of the source directory.
        }

        private void buttonLoadtoCook_Click(object sender, EventArgs e)
        {
            if (comboModname.Text == noComboModname)
            {
                Console.WriteLine("\nERROR --- Mod not Selected! ---");
            }
            else if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("\nERROR --- Uncooked folder needs to be set! ---");
            }
            else if (txtWorkspace.Text == noWorkspace)
            {
                Console.WriteLine("\nERROR --- No Workspace was set! ---");
            }
            else
            {
                tabControl1.SelectedIndex = 0;
                txtModName.Text = comboModname.Text;
                txtModFolder.Text = txtWorkspace.Text + @"\" + comboModname.Text;
                RefreshCookAssets();
            }
        }

        private void buttonDeleteMod_Click(object sender, EventArgs e)
        {
            string modpath = txtWorkspace.Text + @"\" + comboModname.Text;

            if (Directory.Exists(modpath))
            {
                DialogResult result = MessageBox.Show("You are about to delete the mod: \n\n" + comboModname.Text + "\n\nAre you sure you want to delete this mod?", "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Directory.Delete(modpath,true);
                    Console.WriteLine("\nINFO --- Mod " + comboModname.Text + " ---");
                    comboModname.Text = noComboModname;
                    SetFontStyleCombo(comboModname, "Gray", true);
                    RefreshComboMods();
                                        
                }
            }
        }

        private void buttonOpenMod_Click(object sender, EventArgs e)
        {
            string modpath = txtWorkspace.Text + @"\" + comboModname.Text;
            string windir;

            if (Directory.Exists(modpath)){
                windir = Environment.GetEnvironmentVariable("windir");
                Process.Start(windir + @"\Explorer.exe", modpath);
            }
            else
            {
                Console.WriteLine("\nERROR --- Mod folder not found! ---");
            }

        }

        private void buttonOpenUncooked_Click(object sender, EventArgs e)
        {
            
            string windir;

            if (txtUncooked.Text == noUncookedPath)
            {
                Console.WriteLine("\nERROR --- Uncooked folder needs to be set! ---");
            }
            else
            {
                windir = Environment.GetEnvironmentVariable("windir");
                Process.Start(windir + @"\Explorer.exe", txtUncooked.Text);
            }
            
        }
    }//End Form
}//End Namespace
/*
 Find installation path
 
    using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip"))

    MessageBox.Show(key.GetValue("InstallLocation").ToString());
     
     */
