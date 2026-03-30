using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NiceLabelApi.Services
{
    public interface INiceLabelService
    {
        IReadOnlyList<string> GetVariables(Stream file);
        void PrintLabel(Stream label, string printerIpAddress);
    }
}