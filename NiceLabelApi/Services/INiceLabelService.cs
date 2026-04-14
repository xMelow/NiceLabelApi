using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NiceLabel.SDK;

namespace NiceLabelApi.Services
{
    public interface INiceLabelService
    {
        IReadOnlyList<string> GetVariables(Stream file);
        void PrintLabel(Stream label, int quantity, string printerIpAddress);
        void PrintLabelVariables(Stream label, List<Dictionary<string, string>> variables, string printerName);
        List<IPrinter> GetPrinters();

        void GetLabelPreview(Stream label);
    }
}