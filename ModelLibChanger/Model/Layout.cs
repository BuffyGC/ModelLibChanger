using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace ModelLibChanger.Model
{
    public class Layout : BaseModel
    {
        [JsonPropertyName("content")]
        public ObservableCollection<Content> Content { get => content; set => SetField(ref content, value) ; } 
        private ObservableCollection<Content> content = new ObservableCollection<Content>();

        [JsonIgnore]
        public string Name{ get => name; set => SetField(ref name, value); }
        private string name;

        [JsonIgnore]
        public string Action { get => action; set => SetField(ref action, value); }
        private string action;

        [JsonIgnore]
        public string Path { get => path; set => SetField(ref path, value); }
        private string path;

        [JsonIgnore]
        public bool IsSelected { get => isSelected; set => SetField(ref isSelected, value); }
        private bool isSelected = false;

        [JsonIgnore]
        public Layout LayoutFromJson { get => layoutFromJson; set => SetField(ref layoutFromJson, value); }
        private Layout layoutFromJson = null;


        public override string ToString()
        {
            if (HasModelLibBgl())
                return Name + " (" + Content.Count + " files)" + " -->[" + GetNewModelLibBglName() + "]";

            return Name;
        }

        public bool HasModelLibBgl()
        {
            var v = Content.Where(e => e.Path.EndsWith("\\modellib.bgl", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (v != null)
                return true;

            return LayoutFromJson != null && LayoutFromJson.HasModelLibBgl();
        }

        public string GetNewModelLibBglName()
        {
            if (HasModelLibBgl())
            {
                foreach (Content content in Content)
                {
                    if (content.Path.EndsWith("-SHAPE.BGL", StringComparison.OrdinalIgnoreCase))
                    {
                        string filename = System.IO.Path.GetFileName(content.Path);
                        string s = filename.Remove(filename.IndexOf("-SHAPE.BGL", StringComparison.OrdinalIgnoreCase));
                        return s + "-modelLib.BGL";
                    }
                }
            }
            else
            {
                if (LayoutFromJson != null && LayoutFromJson.HasModelLibBgl())
                {
                    return LayoutFromJson.GetNewModelLibBglName();
                }
            }

            return Name + "-modelLib.BGL";
        }

        public void ChangeModelLibName(string newName)
        {
            if (!newName.StartsWith("\\"))
                newName = "\\" + newName;

            var v = Content.Where(e => e.Path.EndsWith("\\modellib.bgl", StringComparison.OrdinalIgnoreCase));

            foreach (Content content in v)
            {
                content.Path = content.Path.Substring(0, content.Path.IndexOf("\\modellib.bgl", StringComparison.OrdinalIgnoreCase));
                content.Path += newName;
            }
        }

        internal List<string> GetModelLibBglName()
        {
            List<string> r = new List<string>();

            var v = Content.Where(e => e.Path.EndsWith("\\modellib.bgl", StringComparison.OrdinalIgnoreCase));

            foreach (Content content in v)
            {
                r.Add(content.Path);
            }

            return r;
        }

        internal void CheckAction()
        {
            Action = String.Empty;

            if (LayoutFromJson == null)
                Action += "Rebuild (invalid layout.json). ";

            if (HasModelLibBgl())
                Action += "Rename modelLib.BGL to " + GetNewModelLibBglName() + ". ";

            if (Content != null && LayoutFromJson != null && LayoutFromJson.Content != null)
                if (Content.Count != LayoutFromJson.Content.Count)
                    Action += "Rebuild (file count difference). ";

        }
    }
}
