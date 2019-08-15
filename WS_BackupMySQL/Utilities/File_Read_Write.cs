using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    class File_Read_Write
    {
        public static void Check_Exits(string Path)
        {
            string temp = "";
            List<string> List_Folder = new List<string>();
            List_Folder.AddRange(Path.Split('\\').ToList());
            temp = List_Folder[0];
            for (int i = 0; i<List_Folder.Count - 1;i++)
            {
                
                if (!System.IO.Directory.Exists(temp)) // kiểm tra xem có tồn tại hay không
                {
                    System.IO.Directory.CreateDirectory(temp);
                }
                temp = temp + "\\" + List_Folder[i+1] ;
            }
            
            string CreateFileTxt = Path;
            if (!System.IO.File.Exists(CreateFileTxt))
            {
                System.IO.FileStream cr = new System.IO.FileStream(CreateFileTxt, System.IO.FileMode.Create);
                cr.Close();
                cr.Dispose();
            }
        }

        public static string Read_File(string path)
        {
            Check_Exits(path);
            string temp = "";
            try
            {
                System.IO.StreamReader docFile = new System.IO.StreamReader(path);
                temp = docFile.ReadLine();
                docFile.Close();
                docFile.Dispose();
            }
            catch { }
            return temp;
        }
        public static void Write_File(string path, string Write, bool GhiTiepVaoFile)
        {
            Check_Exits(path);
            try
            {
                System.IO.StreamWriter GhiFile = new System.IO.StreamWriter(path, GhiTiepVaoFile, System.Text.Encoding.Unicode);
                GhiFile.WriteLine(Write);
                GhiFile.Close();
                GhiFile.Dispose();
            }
            catch { }

        }

        public static bool Check_Exits_Folder(string Path)
        {
            string temp = "";
            bool exists = true;         // true: đã tồn tại, false: chưa tồn tại
            List<string> List_Folder = new List<string>();
            List_Folder.AddRange(Path.Split('\\').ToList());
            temp = List_Folder[0];
            for (int i = 0; i < List_Folder.Count; i++)
            {

                if (!System.IO.Directory.Exists(temp)) // kiểm tra xem có tồn tại hay không
                {

                    return false;
                }
                if(i != List_Folder.Count - 1)
                    temp = temp + "\\" + List_Folder[i + 1];
            }
            return exists;
        }

        public static void Create_Exits_Folder(string Path)
        {
            string temp = "";
            List<string> List_Folder = new List<string>();
            List_Folder.AddRange(Path.Split('\\').ToList());
            temp = List_Folder[0];
            for (int i = 0; i < List_Folder.Count; i++)
            {

                if (!System.IO.Directory.Exists(temp)) // kiểm tra xem có tồn tại hay không
                {
                    System.IO.Directory.CreateDirectory(temp);
                }
                if (i != List_Folder.Count - 1)
                    temp = temp + "\\" + List_Folder[i + 1];
            }
        }
    }
}
