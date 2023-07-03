using System.IO;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace TTLOADER
{
    class FileEngine : IFileEngine
    {
        IFileEngine fileEngine = DependencyService.Get<IFileEngine>();

        public Task<bool> WriteFile(Stream data, string folder, string fileName = null,string ext=".jpg")
        {

            return fileEngine.WriteFile(data, folder, fileName,ext);
        }
    }
}
