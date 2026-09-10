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
        void PrintVariableLabel(Stream label, List<Dictionary<string, string>> variables, string printerName, int amount);
        List<IPrinter> GetPrinters();
        byte[] GetLabelPreview(Stream label, int width, int height);
        List<byte[]> GetLabelPreviewBatch(Stream label, List<Dictionary<string,string>> variables);
        void PrintLabelWithSettings(Stream label,  Dictionary<string, string> labelVariables, Dictionary<string, string> printSettings, string printerName);
        void PrintToPdf(List<Stream> labels, Dictionary<string, string> variables, string outputFileName, bool appendToFile);
        void PrintLabelBatch(List<Stream> labels, Dictionary<string, string> variables, Dictionary<string, string> printerSettings, string printerName);
    }
}