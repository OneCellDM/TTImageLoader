using System.IO;
using System.Threading.Tasks;

namespace TTLOADER
{
    public interface IFileEngine
    {
        Task<bool> WriteFile(Stream data, string folder, string fileName = null, string ext = ".jpg");
    }
}