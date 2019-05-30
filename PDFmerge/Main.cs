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
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using VB = Microsoft.VisualBasic;
using System.Diagnostics;


namespace PDFmerge
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MainForm_DragEnter);
            this.DragDrop += new DragEventHandler(MainForm_DragDrop);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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

                IEnumerable<string> files = listBox1.Items.Cast<string>();

                FileInfo info = new FileInfo(listBox1.Items[0].ToString());

                string targetDir = info.Directory.FullName; 

                //string[] allDirs = new string[listBox1.Items.Count];
                //string targetDir = string.Empty;
                //int i = 0;
                //foreach (string file in files)
                //{
                //    FileInfo info = new FileInfo(file);
                //    allDirs[i] = info.Directory.FullName;
                //    i++;
                //}

                //targetDir = allDirs[0];

                string prompt = "What do you want to name the merged PDF file? \r\nThe merged PDF file will be saved in: \r\n" + targetDir;

                string newFileName = VB.Interaction.InputBox(prompt, "PDF File Name", "Merged PDF " + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + " " + DateTime.Now.Hour + ";" + DateTime.Now.Minute + ".pdf");


                if (!string.IsNullOrEmpty(newFileName))
                {
                    newFileName = newFileName.Contains(".pdf") ? newFileName : newFileName + ".pdf";

                    string fullPath = Path.Combine(targetDir, newFileName);

                    bool success = MergePDFs(files, fullPath);

                    Application.DoEvents();

                    if (success)
                    {
                        //Process.Start(@"" + targetDir);
                        Process.Start(@"" + fullPath);
                        listBox1.Items.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong. Close the program and try again.");
                    }

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


        public static bool MergePDFs(IEnumerable<string> fileNames, string targetPdf)
        {
            bool merged = true;
            using (FileStream stream = new FileStream(targetPdf, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    foreach (string file in fileNames)
                    {
                        reader = new PdfReader(file);
                        pdf.AddDocument(reader);
                        reader.Close();
                    }
                }
                catch (Exception)
                {
                    merged = false;
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                finally
                {
                    if (document != null)
                    {
                        document.Close();
                    }
                }
            }
            return merged;
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
            if (listBox1.Items.Count > 0)
            {
                btnClear.Enabled = true;
            }
        }
    }
}
