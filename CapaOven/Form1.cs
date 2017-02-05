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


        public Form1()
        {
            
            InitializeComponent();

            string test;
            test = ConfigurationManager.AppSettings["UncookedPath"];
            if(test != null) { txtUncooked.Text = test; SetFontStyle(txtUncooked, "White", false); }

            test = ConfigurationManager.AppSettings["WccPath"];
            if (test != null) { txtWcc.Text = test; SetFontStyle(txtWcc, "White", false); }

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
            cmdImportArgs = cmdImportArgs + "wcc_lite.exe cook -platform=pc -mod=" +"\""+ txtModFolder.Text + @"\uncooked" + "\"" + " -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -outdir=" + "\"" + txtModFolder.Text + @"\cooked" + "\"";
                
                //Build Cache Arguments
                cmdImportArgs = cmdImportArgs + " & wcc_lite.exe buildcache textures -platform=pc -basedir=" + "\"" + txtModFolder.Text + @"\uncooked" + "\"" + " -db=" + "\"" + txtModFolder.Text + @"\cooked\cook.db" + "\""+ " -out=" + "\"" + txtModFolder.Text + @"\packed\" + txtModName.Text + @"\content\texture.cache" + "\"";

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
                        Console.WriteLine("INFO --- Done!");  };

            
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
                Array.Resize(ref processes, assets.Count()+4);
                Array.Resize(ref args, assets.Count()+4);
                
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
            foreach(string st in args)
            {
                if(st != null)
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
            string fullPath;
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
                    s.EndsWith(".tga",StringComparison.CurrentCultureIgnoreCase) 
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

                        // Add the data
                        row.Cells["assetsColumn"].Value = str.Replace(fullPath,"");
                        row.Cells["texturetypeColumn"].Value = null;
                                        

                        Console.WriteLine(str);
                    
                    }
                    Console.WriteLine("--- Assets Loaded from Modded folder ---");
                    txtModFolder.ForeColor = Color.FromName("White");
                    txtModFolder.Font = new Font(txtModFolder.Font.Name, txtModFolder.Font.Size, FontStyle.Regular);
                }
                else { Console.WriteLine("ERROR --- The specified 'Mod Directory' does not contain a \"Modded\" folder"); }
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

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
            if(txtModName.Text == noModName) {
                      
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
                        Console.WriteLine("INFO --- Done Uncooking!");  };
                        
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if(txtUncooked.Text != noUncookedPath)
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
            if(txtUncooked.Text != noUncookedPath) { dialog.InitialDirectory = txtUncooked.Text; }
            if (txtExpDir.Text != noExpDir) { dialog.InitialDirectory = txtExpDir.Text; }
            dialog.IsFolderPicker = true;
            dialog.ShowPlacesList = true;
            dialog.ShowHiddenItems = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtExpDir.Text = dialog.FileName;
                                
                    Console.WriteLine("--- Loading Exportable Assets from selected folder ---");

                    //Gets recursive the files with specific extensions
                    switch (comboBox4.SelectedIndex){

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

        public void RunProcessesSequence(Process[] procs, string[]args, int i, string op)
        {

            if (args[i] == null)
            {
                if (i < procs.Count() - 1)
                {
                    i = i + 1;
                    RunProcessesSequence(procs, args, i, op);
                }
                else
                {
                    Console.WriteLine("\r\n\nINFO --- "+ op + " Done!");
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
                Console.WriteLine("--- Exportable Assets Refreshed---");
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

}
/*
 Find installation path
 
    using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip"))

    MessageBox.Show(key.GetValue("InstallLocation").ToString());
     
     */
