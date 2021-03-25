using System;
using System.Text;

namespace ModelLibChanger.Classes
{
    public class LongFile
    {
        public const int MAX_PATH = 260;

        public static bool Exists(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.Exists(path);
        }

        public static void Delete(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);
            
            System.IO.File.Delete(path);
        }

        public static void WriteAllText(string path, string contents)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            System.IO.File.WriteAllText(path, contents);
        }

        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            System.IO.File.WriteAllText(path, contents, encoding);
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            System.IO.File.WriteAllBytes(path, bytes);
        }

        public static void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName.Length >= MAX_PATH)
                sourceFileName = GetWin32LongPath(sourceFileName);

            if (destFileName.Length >= MAX_PATH)
                destFileName = GetWin32LongPath(destFileName);

            System.IO.File.Copy(sourceFileName, destFileName, overwrite);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            if (sourceFileName.Length >= MAX_PATH)
                sourceFileName = GetWin32LongPath(sourceFileName);

            if (destFileName.Length >= MAX_PATH)
                destFileName = GetWin32LongPath(destFileName);

            System.IO.File.Move(sourceFileName, destFileName);
        }

        public static byte[] ReadAllBytes(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.ReadAllBytes(path);
        }

        public static string ReadAllText(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.ReadAllText(path);
        }


        public static string[] ReadAllLines(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.ReadAllLines(path);
        }

        public static System.IO.StreamWriter AppendText(string path)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.AppendText(path);
        }



        public static void WriteAllLines(string path, string[] contents)
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            System.IO.File.WriteAllLines(path, contents);

        }

        public static System.IO.FileStream Create(string path) 
        {
            if (path.Length >= MAX_PATH)
                path = GetWin32LongPath(path);

            return System.IO.File.Create(path);
        }

        public static string GetWin32LongPath(string path)
        {
            if (path.StartsWith(@"\\?\")) return path;

            if (path.StartsWith("\\"))
            {
                path = @"\\?\UNC\" + path.Substring(2);
            }
            else if (path.Contains(":"))
            {
                path = @"\\?\" + path;
            }
            else
            {
                var currdir = Environment.CurrentDirectory;
                path = Combine(currdir, path);
                while (path.Contains("\\.\\")) path = path.Replace("\\.\\", "\\");
                path = @"\\?\" + path;
            }
            return path.TrimEnd('.'); ;
        }

        private static string Combine(string path1, string path2)
        {
            return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.'); ;
        }

    }
}
