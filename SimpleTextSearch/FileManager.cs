using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTextSearch
{
    public class FileManager
    {
        public static SortedDictionary<int, string> docsIds { get; private set; }
        private static readonly object docsIdsCreation_lock = new object();
        private static readonly object getID_lock = new object();
        public static bool Ready { get; private set; }
        private int getID()
        {
            lock (getID_lock)
            {
                return docsIds == null ? 0 : docsIds.Count();
            }
        }
        public FileManager()
        {
            lock (docsIdsCreation_lock)
            {
                if (docsIds == null)
                    docsIds = new SortedDictionary<int, string>();
            }
        }
        public async Task BrowseFolders()
        {
            Ready = false;
            // Create CommonOpenFileDialog 
            var dlg = new CommonOpenFileDialog("Select Folder/s");
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.DefaultDirectory = Directory.GetCurrentDirectory();
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = true;
            dlg.ShowPlacesList = true;
            // Display CommonOpenFileDialog by calling ShowDialog method 
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folders = dlg.FileNames;
                ShowConsoleSafely();
                await Task.Run(() => 
                {
                    foreach (var folder in folders)
                        GetAllFromDirectory(folder);
                });
                HideConsoleSafely();
            }
            Ready = true;
        }
        // Get all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        private void GetAllFromDirectory(string targetDirectory)
        {
            // Get the list of text files found in the directory.
            if (Directory.Exists(targetDirectory) && DirectoryHasPermission(targetDirectory, FileSystemRights.Read))
            {
                string[] fileEntries = Directory.GetFiles(targetDirectory, "*.txt");
                foreach (string file in fileEntries)
                {
                    if (!docsIds.ContainsValue(file))
                    {
                        docsIds.Add(getID(), file);
                        Console.WriteLine("File Added: " + file);
                    }
                }
                //Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    GetAllFromDirectory(subdirectory);
            }
        }
        public async Task BrowseFiles()
        {
            Ready = false;
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Select File/s";
            // Set filter for file extension and default file extension and other
            dlg.DefaultExt = ".png";
            dlg.Filter = "Text Files|*.txt|All|*.*";
            dlg.Multiselect = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();
            // Get the selected file names
            if (result == true)
            {
                var files = dlg.FileNames;
                ShowConsoleSafely();
                await Task.Run(() =>
                {
                    foreach (var file in files)
                    {
                        if (!docsIds.ContainsValue(file))
                        {
                            docsIds.Add(getID(), file);
                            Console.WriteLine("File Added: " + file);
                        }
                    }
                });
                HideConsoleSafely();
            }
            Ready = true;
        }
        public string ReadFileText(string filePath)
        {
            if (File.Exists(filePath))
                return File.ReadAllText(filePath, Encoding.Default).ToLower();
            return string.Empty;
        }

        /// <summary>
        /// Test a directory for create file access permissions
        /// </summary>
        /// <param name="DirectoryPath">Full path to directory </param>
        /// <param name="AccessRight">File System right tested</param>
        /// <returns>State [bool]</returns>
        public bool DirectoryHasPermission(string DirectoryPath, FileSystemRights AccessRight)
        {
            if (string.IsNullOrEmpty(DirectoryPath)) return false;
            try
            {
                AuthorizationRuleCollection rules = Directory.GetAccessControl(DirectoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (identity.Groups.Contains(rule.IdentityReference))
                    {
                        if ((AccessRight & rule.FileSystemRights) == AccessRight)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }
        #region Console Debugging Area
        private static bool IsConsole = false;
        private static bool IsConsoleAlloc = false;
        public static void ShowConsoleSafely()
        {
            if (!IsConsoleAlloc)
            {
                AllocConsole();
                IsConsole = IsConsoleAlloc = true;
            }
            var handle = GetConsoleWindow();
            if (!IsConsole)
            {
                ShowWindow(handle, SW_SHOW);
                IsConsole = true;
            }
        }
        public static void HideConsoleSafely()
        {
            var handle = GetConsoleWindow();
            if (IsConsole)
            {
                ShowWindow(handle, SW_HIDE);
                IsConsole = false;
            }
        }
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        [DllImport("Kernel32")]
        private static extern void AllocConsole();
        #endregion
    }
}
