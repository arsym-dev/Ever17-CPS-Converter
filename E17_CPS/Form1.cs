using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

// Ported from https://github.com/dsp2003/ae/blob/master/src/format/graphics/AG_KID_Engine.pas

namespace E17_CPS
{
    public partial class Form1 : Form
    {
        // File settings
        CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog();
        FileSystemWatcher folderWatcher = new FileSystemWatcher();
        private string[] image_extensions = { ".png", ".jpeg", ".jpg", ".gif", ".bmp", ".prt" };
        string imageFullPath = null;
        string imageName;
        Bitmap image = null;

        public Form1()
        {
            InitializeComponent();

            // Set up filewatcher
            folderWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            folderWatcher.Filter = "*";
            folderWatcher.Changed += new FileSystemEventHandler(OnFolderChange);
            folderWatcher.Renamed += new RenamedEventHandler(OnFolderChange);
            folderWatcher.Deleted += new FileSystemEventHandler(OnFolderChange);

            // Set up folder selecter
            openFolderDialog.Title = "Select a folder";
            openFolderDialog.IsFolderPicker = true;
            openFolderDialog.AddToMostRecentlyUsedList = false;
            openFolderDialog.AllowNonFileSystemItems = false;
            openFolderDialog.EnsureFileExists = true;
            openFolderDialog.EnsurePathExists = true;
            openFolderDialog.EnsureReadOnly = false;
            openFolderDialog.EnsureValidNames = true;
            openFolderDialog.Multiselect = false;
            openFolderDialog.ShowPlacesList = true;

            // Set up listbox for files
            listboxFiles.DisplayMember = "Message";
            listboxFiles.ValueMember = "FileInfo";
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var res = openFolderDialog.ShowDialog();
            if (res == CommonFileDialogResult.Ok)
            {
                textboxDirectoryImage.Text = openFolderDialog.FileName;

                if (textboxDirectoryCPS.Text == "")
                    textboxDirectoryCPS.Text = openFolderDialog.FileName;
            }
        }

        private void buttonBrowseCPS_Click(object sender, EventArgs e)
        {
            var res = openFolderDialog.ShowDialog();
            if (res == CommonFileDialogResult.Ok)
            {
                textboxDirectoryCPS.Text = openFolderDialog.FileName;
            }
        }

        private void textboxDirectory_TextChanged(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(textboxDirectoryImage.Text);

            if (dir.Exists)
            {
                // Valid folder
                folderWatcher.Path = textboxDirectoryImage.Text;
                folderWatcher.EnableRaisingEvents = true;
                PopulateFileListbox();
                //listboxFiles.SelectedIndex = 0;
            }
            else
            {
                // invalid folder
                folderWatcher.EnableRaisingEvents = false;
                listboxFiles.Items.Clear();
            }
        }


        private void PopulateFileListbox()
        {
            // Get current index
            int prev_idx = listboxFiles.SelectedIndex;
            var prev_entry = (listboxFiles.SelectedItem as ListboxFileEntry);


            listboxFiles.Items.Clear();

            // Get list of all valid files
            DirectoryInfo dir = new DirectoryInfo(textboxDirectoryImage.Text);
            var files = GetFilesByExtensions(dir, image_extensions);

            foreach (var f in files)
            {
                listboxFiles.Items.Add(new ListboxFileEntry(f.FullName, f.Name));
            }

            // Select the last item we had before
            if (listboxFiles.Items.Count > 0 && prev_entry != null)
            {
                bool found_old_entry = false;
                foreach (var fn in listboxFiles.Items)
                {
                    ListboxFileEntry entry = (ListboxFileEntry)fn;
                    if (entry.FileInfo.Name == prev_entry.FileInfo.Name)
                    {
                        listboxFiles.SelectedItem = entry;
                        found_old_entry = true;
                        break;
                    }
                }

                // Didn't find the exact entry, so let's keep the cursor in the same area
                if (!found_old_entry && listboxFiles.Items.Count > 0)
                    listboxFiles.SelectedIndex = Math.Min(prev_idx, listboxFiles.Items.Count - 1);
            }

        }


        #region Folder Watcher
        private void OnFolderChange(object sender, FileSystemEventArgs e)
        {
            var ext = (Path.GetExtension(e.FullPath) ?? string.Empty).ToLower();

            if (!image_extensions.Any(ext.Equals))
                // File changed is not a valid image, ignore it
                return;

            // Reload the file list
            this.BeginInvoke(new MethodInvoker(delegate
            {
                PopulateFileListbox();
            }));
        }

        private IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }
        #endregion

        private void listboxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listboxFiles.SelectedItem == null)
                return;
            
            // Display image

            // Get the currently selected item in the ListBox.
            ListboxFileEntry listboxEntry = (ListboxFileEntry)listboxFiles.SelectedItem;
            imageFullPath = listboxEntry.FileInfo.FullName;
            imageName = listboxEntry.FileInfo.Name;

            if (listboxEntry.FileInfo.Exists)
            {
                //pictureboxMain.ImageLocation = fullpath;
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }

                var ext = (Path.GetExtension(imageName) ?? string.Empty).ToLower();
                if (ext != ".prt")
                {
                    try
                    {
                        // Open file, make a copy of the image, then close the file
                        using (var bmpTemp = new Bitmap(imageFullPath))
                        {
                            image = new Bitmap(bmpTemp);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image " + listboxEntry.FileInfo.FullName + "\n\nReason: " + ex.Message);
                        return;
                    }

                    picturebox.Image = image;
                }
            }
            else
            {
                MessageBox.Show("The following file does not exist: " + imageFullPath, "Error opening file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonSaveCPS_Click(object sender, EventArgs e)
        {
            if (imageFullPath == null)
                return;

            try
            {
                CompressImage(imageFullPath);
                MessageBox.Show("Converted 1 image.");
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonBatch_Click(object sender, EventArgs e)
        {
            int successful = 0;
            int max_files = listboxFiles.Items.Count;
            for (int i=0; i< max_files; i++)
            {
                try
                {
                    string filepath = (listboxFiles.Items[i] as ListboxFileEntry).FileInfo.FullName;
                    CompressImage(filepath);
                    successful++;
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            MessageBox.Show("Converted " + successful.ToString() + "/" + max_files.ToString() + " images.");
        }

        void CompressImage(string filepath)
        {
            string filename = Path.GetFileName(filepath);

            int index = filename.LastIndexOf('.');
            string filename_no_ext = filename.Substring(0, index);
            string ext = filename.Substring(index + 1).ToLower();

            MemoryStream stream_bmp, stream_prt, stream_cps;

            // Convert to 8-bit indexed BMP
            //Bitmap bmp = BMP_format.CopyToBpp(image, 8);

            if (ext == "prt")
            {
                stream_prt = new MemoryStream();
                // Use original PRT
                FileStream fin_stream = new FileStream(filepath, FileMode.Open);
                fin_stream.CopyTo(stream_prt);
                fin_stream.Close();
                fin_stream.Dispose();
            }
            else
            {
                stream_bmp = new MemoryStream();
                if (ext != "bmp")
                {
                    // Convert file to BMP
                    // Open file, make a copy of the image, then close the file
                    Bitmap bmp;
                    using (var bmpTemp = new Bitmap(filepath))
                    {
                        bmp = new Bitmap(bmpTemp);
                        Console.WriteLine(bmp.PixelFormat);
                    }

                    bmp.Save(stream_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else
                {
                    // Use original BMP
                    FileStream fin_stream = new FileStream(filepath, FileMode.Open);
                    fin_stream.CopyTo(stream_bmp);
                    fin_stream.Close();
                    fin_stream.Dispose();
                }

                // Convert to PRT
                string filename_prt = null; // Path.Combine(textboxDirectoryCPS.Text, filename_no_ext + ".prt");;
                stream_prt = PRT_format.Convert(stream_bmp, false, filename_prt);
            }
            

            // Convert to CPS
            string filename_cps = Path.Combine(textboxDirectoryCPS.Text, filename_no_ext + ".cps");
            stream_cps = CPS_format.Compress(stream_prt, false, filename_cps);

            stream_prt.Dispose();
            stream_cps.Dispose();
        }
    }


    public class ListboxFileEntry
    {
        public FileInfo FileInfo { get; set; }
        public string Message { get; set; }

        public ListboxFileEntry(string filename, string message = null)
        {
            FileInfo = new FileInfo(filename);

            if (message != null)
                this.Message = message;
            else
                this.Message = FileInfo.Name;
        }

    }
}
