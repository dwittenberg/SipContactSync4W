using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhonerLiteSync.Model
{
    [Serializable]
    public class PathGui
    {
        private string l;

        public string LocalPath
        {
            get => l;
            set
            {
                l = value;
                LocalPathOk = File.Exists(l);
                SetAllOk();
            }
        }

        public bool LocalPathOk { get; private set; }

        private string e;

        public string ExternPath
        {
            get => e;
            set
            {
                if (value == null || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                e = value;
                var f = new FileInfo(value);

                if (Directory.Exists(f.DirectoryName))
                {
                    ExternPathOk = true;
                    SetAllOk();
                    return;
                }

                var d = Directory.CreateDirectory(f.DirectoryName);
                ExternPathOk = d.Exists;
                SetAllOk();
            }
        }

        public bool ExternPathOk { get; private set; }

        public bool AllOk { get; private set; }

        private void SetAllOk()
        {
            AllOk = LocalPathOk && ExternPathOk;
        }
    }
}
