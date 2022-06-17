using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using VB = Microsoft.VisualBasic;
using System.Diagnostics;


namespace PDFmerge
{
    public partial class Merge_Main : Form
    {

        public bool ShowAfterMerge { get; set; } = true;

        private readonly string WelcomeHtmlPath = Path.Combine(MergeSettings.INSTALL_DIR, @"html\welcome.htm");

        public Merge_Main(string[] files = null)
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MainForm_DragEnter);
            this.DragDrop += new DragEventHandler(MainForm_DragDrop);

            if (files != null)
            {
                LoadFiles(files);
            }
            

        }

        
        private async void Main_Load(object sender, EventArgs e)
        {
            //await InitializeAsync();

            

            //StreamReader reader = new StreamReader(File.OpenRead(welcomeHtmlPath));

            //webview.NavigateToString(reader.ReadToEnd());

            chrome.LoadUrl(WelcomeHtmlPath);

            //reader.Close();

        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFiles(files);
        }

        private void LoadFiles(string[] files)
        {
            foreach (string file in files)
            {

                FileInfo info = new FileInfo(file);

                if (info.Extension == ".pdf")
                {
                    listBox1.Items.Add(file);
                }
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {

            if (listBox1.Items.Count > 1)
            {

                string[] files = listBox1.Items.Cast<string>().ToArray(); 

                FileInfo info = new FileInfo(listBox1.Items[0].ToString());

                string targetDir = info.Directory.FullName;

                SaveFileDialog saveDiag = new SaveFileDialog();
                saveDiag.Filter = "PDF Files | *.pdf";
                saveDiag.Title = "Save your merged PDF";                
                var result = saveDiag.ShowDialog();
                

                if (result != DialogResult.OK) return;
                if (saveDiag.FileName == string.Empty) return;

                string newFileName = saveDiag.FileName;

                //tring prompt = "What do you want to name the merged PDF file? \r\nThe merged PDF file will be saved in: \r\n" + targetDir;

                //string newFileName = VB.Interaction.InputBox(prompt, "PDF File Name", "Merged PDF " + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + " " + DateTime.Now.Hour + ";" + DateTime.Now.Minute + ".pdf");


                if (!string.IsNullOrEmpty(newFileName))
                {
                    newFileName = newFileName.Contains(".pdf") ? newFileName : newFileName + ".pdf";

                    string fullPath = Path.Combine(targetDir, newFileName);

                    CombineMultiplePDFs(files, fullPath);                                      

                    if (ShowAfterMerge) Process.Start(@"" + fullPath);
                    
                    listBox1.Items.Clear();
                    chrome.LoadUrl(WelcomeHtmlPath);
                    this.DialogResult = DialogResult.OK;

                }
                else
                {
                    MessageBox.Show("Merge failed. No file name provided.");
                }
            }
            else
            {
                MessageBox.Show("There must be at least two PDF files to merge.");
            }
        }

        public void MoveItem(int direction)
        {
            // Checking selected item
            if (listBox1.SelectedItem == null || listBox1.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listBox1.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox1.Items.Count)
                return; // Index out of range - nothing to do

            object selected = listBox1.SelectedItem;

            // Removing removable element
            listBox1.Items.Remove(selected);
            // Insert it in new position
            listBox1.Items.Insert(newIndex, selected);
            // Restore selection
            listBox1.SetSelected(newIndex, true);
        }




        public static void CombineMultiplePDFs(string[] fileNames, string outFile)
        {
            if (fileNames.Length <= 0) return;
            if (outFile == null) return;

            PdfDocument outDoc = new PdfDocument(outFile);

            foreach (string file in fileNames)
            {
                PdfDocument doc = PdfReader.Open(file, PdfDocumentOpenMode.Import); 

                for (int i = 0; i < doc.Pages.Count; i++)
                {
                    outDoc.AddPage(doc.Pages[i]);
                    doc.Close(); 
                }

            }

            outDoc.Close();
            
            Application.DoEvents();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                int selected = listBox1.SelectedIndex;

                
                if (selected > -1)
                {
                    //Remove selected item from list
                    listBox1.Items.Remove(listBox1.Items[selected]);

                    //Check the number of items remaining. 
                    if (listBox1.Items.Count > 0)
                    {
                        listBox1.SelectedIndex = selected > 0 ? selected - 1 : 0;
                    }
                    else
                    {
                        listBox1.SelectedIndex = -1;
                        btnClear.Enabled = false; 
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex < 0) return; 

            if (listBox1.Items.Count > 0)
            {
                string path = listBox1.SelectedItem.ToString();

                btnClear.Enabled = true;

                if (path != null && path != chrome.Address)
                {
                    //webview.Navigate(path);
                    chrome.LoadUrl(path);
                }

            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.Filter = "PDF Files | *.pdf";
            diag.Multiselect = true;
            var result = diag.ShowDialog(); 

            if (result == DialogResult.OK)
            {
                string[] files = diag.FileNames; 

                foreach (string file in files)
                {
                    listBox1.Items.Add(file); 
                }
            }

        }
    }

    //static class WebViewHelper
    //{

    //    public static void Navigate(this Microsoft.Web.WebView2.WinForms.WebView2 webview, string path)
    //    {
    //        if (webview != null && webview.CoreWebView2 != null)
    //        {
    //            webview.CoreWebView2.Navigate(path);
    //        }
    //    }

    //}

}
