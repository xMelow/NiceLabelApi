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
        byte[] GetLabelPreview(Stream label, int width, int height);
        List<byte[]> GetLabelPreviewBatch(Stream label, List<Dictionary<string,string>> variables);
    }
}