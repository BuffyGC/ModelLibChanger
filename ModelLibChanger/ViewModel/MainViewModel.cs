using ModelLibChanger.Classes;
using ModelLibChanger.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;

namespace ModelLibChanger.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Layout> LayoutCollection { get => layoutCollection; set => SetField(ref layoutCollection, value); }
        private ObservableCollection<Layout> layoutCollection = new ObservableCollection<Layout>();

        public string CommunityFolder { get => communityFolder; set => SetField(ref communityFolder, value); }
        private string communityFolder;

        public RelayCommand CommandLoadAddOns { get => commandLoadAddOns = commandLoadAddOns ?? new RelayCommand(ExecuteLoadAddOns); }
        private RelayCommand commandLoadAddOns;

        public RelayCommand CommandRename { get => commandRename= commandRename?? new RelayCommand(ExecuteRename); }
        private RelayCommand commandRename;

        public string BaseTitle { get => baseTitle; set => SetField(ref baseTitle, value); }
        private string baseTitle;


        public RelayCommand CommandSelectPath { get => commandSelectPath = commandSelectPath ?? new RelayCommand(ExecuteSelectPath); }
        private RelayCommand commandSelectPath;



        public MainViewModel()
        {
#if DEBUG
            baseTitle = string.Format("{0} {1} Developer", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
#else
            baseTitle = string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
#endif
            CommunityFolder = CommFolderDetector.GetCommFolder();

            if (communityFolder.Length == 0)
            {
                MessageBox.Show("Could not detect a community folder. Program will exit!");
                Application.Current.Shutdown();
            }
        }



        private void ExecuteSelectPath(object obj)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog openFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    
                    ShowNewFolderButton = false,
                    Description = "Add-On-Path",
                    SelectedPath = CommunityFolder,
                    RootFolder = Environment.SpecialFolder.MyComputer
                };

                System.Windows.Forms.DialogResult dlgr = openFolderBrowserDialog.ShowDialog();

                if (dlgr == System.Windows.Forms.DialogResult.OK)
                {
                    CommunityFolder = openFolderBrowserDialog.SelectedPath;
                    LayoutCollection = new ObservableCollection<Layout>();

                    ExecuteLoadAddOns(null);
                }
            }

            catch (Exception)
            {

            }
        }




        private void ExecuteRename(object obj)
        {
            string errors = string.Empty;
            string messages = string.Empty;
            UIServices.SetBusyState();

            int count = 0;

            foreach (Layout layout in LayoutCollection)
            {
                if (layout.IsSelected)
                {
                    try
                    {
                        List<string> modelLibBgls = layout.GetModelLibBglName();
                        string newName = layout.GetNewModelLibBglName();

                        layout.ChangeModelLibName(newName);

                        foreach (string modelLibBgl in modelLibBgls)
                        {
                            string ModelLibFile = layout.Path + "\\" + modelLibBgl;

                            string path = System.IO.Path.GetDirectoryName(modelLibBgl);

                            string NewModelLibFile = layout.Path + "\\" + path + "\\" + newName;

                            if (LongFile.Exists(ModelLibFile))
                            {
                                if (LongFile.Exists(NewModelLibFile))
                                {
                                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(NewModelLibFile, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                                }
                                LongFile.Move(ModelLibFile, NewModelLibFile);
                            }
                        }

                        if (LongFile.Exists(layout.Path + "\\layout.json.bak"))
                            LongFile.Delete(layout.Path + "\\layout.json.bak");

                        LongFile.Copy(layout.Path + "\\layout.json", layout.Path + "\\layout.json.bak");

                        Layout newLayout = layout; // ReadLayoutFromFolder(layout.Path + "\\layout.json");

                        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                        jsonOptions.WriteIndented = true;
                        jsonOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);
                        jsonOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;

                        string json = JsonSerializer.Serialize(layout, jsonOptions);

                        json = json.Replace("\r\n", "\n");
                        json = json.Replace(@"\\", "/");

                        LongFile.WriteAllText(newLayout.Path + "\\layout.json", json, new System.Text.UTF8Encoding(false));

                        count++;

                        messages += "\r\n" + layout.Name;
                    }
                    catch (Exception ex)
                    {
                        if (errors.Length > 0)
                            errors += "\r\n";
                        errors += ex.Message;
#if DEBUG
                        errors += "\r\n" + ex.StackTrace;
#endif
                    }
                }
            }

            if (errors.Length > 0)
                MessageBox.Show(errors);

            MessageBox.Show(count + " add-ons with modellib.bgl/layout.json were corrected:\r\n" + messages);

            ExecuteLoadAddOns(null);
        }

        private void ExecuteLoadAddOns(object obj)
        {
            try
            {
                UIServices.SetBusyState();

                LayoutCollection = new ObservableCollection<Layout>();

                IEnumerable<string> layoutFiles = System.IO.Directory.EnumerateFiles(LongFile.GetWin32LongPath(CommunityFolder), "layout.json", System.IO.SearchOption.AllDirectories);

                foreach (string layoutFile in layoutFiles)
                {
                    Layout layout = ReadLayoutFromFolder(layoutFile);


                    if (layout == null || layout.Content == null)
                    {
                        System.Diagnostics.Debugger.Break();
                    }

                    if (layout != null && layout.Content!= null && layout.Content.Count() > 0 && layout.Action.Length > 0) // layout.HasModelLibBgl())
                    {
                        LayoutCollection.Add(layout);
                    }
                }

                if (LayoutCollection.Count == 0)
                    MessageBox.Show("No ( more ) add-ons with incorrect modelLib.BGL/layout.json found.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
#if DEBUG
                MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
#else
                MessageBox.Show(e.Message);
#endif

            }
        }


        private Layout ReadLayoutFromFolder(string layoutFile)
        {
            Layout layoutJson = null;

            try
            {
                string json = LongFile.ReadAllText(layoutFile);

                json = json.Replace("/", "\\\\");

                JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                jsonOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;


                layoutJson = (Layout)JsonSerializer.Deserialize(json, typeof(Layout), jsonOptions) ;

                layoutJson.Path = System.IO.Path.GetDirectoryName(layoutFile).Replace(@"\\?\", "");
                layoutJson.Name = System.IO.Path.GetFileNameWithoutExtension(layoutJson.Path); // GetRelativePath(System.IO.Path.GetDirectoryName(layoutFile), CommunityFolder);


                // return layoutJson;
            }
            catch (Exception)
            {
#if DEBUG
#endif
            }

            Layout layout = new Layout()
            {
                Path = System.IO.Path.GetDirectoryName(layoutFile).Replace(@"\\?\", ""),
                Name = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetDirectoryName(layoutFile)) // GetRelativePath(System.IO.Path.GetDirectoryName(layoutFile), CommunityFolder),
            };

            string layoutPath = System.IO.Path.GetFullPath(layoutFile);

            IEnumerable<string> projectFiles = System.IO.Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(layoutFile), "*.*", System.IO.SearchOption.AllDirectories);

            foreach (string projectFile in projectFiles)
            {
                string relativePath = GetRelativePath(projectFile, System.IO.Path.GetDirectoryName(layoutPath));

                Content content = new Content
                {
                    Path = relativePath,
                    Size = new System.IO.FileInfo(LongFile.GetWin32LongPath(projectFile)).Length,
                    Date = new System.IO.FileInfo(LongFile.GetWin32LongPath(projectFile)).LastWriteTimeUtc.ToFileTimeUtc()
                };

                if (!relativePath.StartsWith("_CVT_", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(relativePath, "business.json", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(relativePath, "layout.json", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(relativePath, "layout.json.bak", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(relativePath, "manifest.json", StringComparison.OrdinalIgnoreCase)
                    && !relativePath.StartsWith("MarketplaceData", StringComparison.OrdinalIgnoreCase))
                {
                    layout.Content.Add(content);
                }
            }

            layout.LayoutFromJson = layoutJson;

            layout.CheckAction();

            return layout;
        }

        private string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec.Replace(@"\\?\", ""));
            // Folders must end in a slash
            if (!folder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                folder += System.IO.Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder.Replace(@"\\?\", ""));
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }

    }
}
