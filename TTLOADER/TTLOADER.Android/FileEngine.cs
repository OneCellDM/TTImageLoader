using Android.Content;

using Java.IO;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TTLOADER;
using Xamarin.Forms;

[assembly: Dependency(typeof(TTLOADER.Droid.SaveFile))]
namespace TTLOADER.Droid
{

    public class SaveFile : IFileEngine
    {
        public async Task<bool> WriteFile(System.IO.Stream stream, string folder, string fileName = null,string ext=".jpg")
        {
            try
            {
                Context context = Android.App.Application.Context;


                string root = null;

                //Get the root path in android device.
                if (Android.OS.Environment.IsExternalStorageEmulated)
                {
                    root = Android.OS.Environment.ExternalStorageDirectory.ToString();
                    root = System.IO.Path.Combine(root, folder);
                }
                else
                    root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string path = string.Empty;
                Java.IO.File file = null;
                if (string.IsNullOrEmpty(fileName))
                {

                    do
                    {

                        var _fileName = Path.GetRandomFileName().Split('.').FirstOrDefault();
                        
                        path = System.IO.Path.Combine(root, $"{_fileName}{ext}");
                        file = new Java.IO.File(path);
                    }
                    while (file.Exists());


                }
                else {

                    file = new Java.IO.File(path);
                    //Remove if the file exists
                    if (file.Exists()) file.Delete();
                }
                
                //Write the stream into the file
                FileOutputStream outs = new FileOutputStream(file);
                byte[] buffer = new byte[1024];

                while (stream.Read(buffer, 0, buffer.Length) > 0)
                {
                    outs.Write(buffer);
                }
                outs.Flush();
                outs.Close();
                return true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}