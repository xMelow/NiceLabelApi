using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NiceLabel.SDK;
using NiceLabelApi.Domain;

namespace NiceLabelApi.Services
{
    public class NiceLabelService : INiceLabelService
    {
        private readonly NiceLabelEngine _engine;

        public NiceLabelService(NiceLabelEngine engine)
        {
            _engine = engine;
        }

        public IReadOnlyList<string> GetVariables(Stream label)
        {
            return _engine.GetVariables(label);
        }

        public void PrintLabel(Stream label, int quantity, string printerName)
        {
            _engine.PrintLabel(label, quantity, printerName);
        }

        public void PrintLabelVariables(Stream label, List<Dictionary<string, string>> variables, string printerName)
        {
            _engine.PrintLabelVariables(label, variables, printerName);
        }

        public List<IPrinter> GetPrinters()
        {
            return _engine.GetPrinters();
        }

        public byte[] GetLabelPreview(Stream label, int width, int height)
        {
            return _engine.GetLabelPreview(label, width, height);
        }

        public List<byte[]> GetLabelPreviewBatch(Stream label, List<Dictionary<string,string>> variables)
        {
            return _engine.GetLabelPreviewBatch(label, variables);
        }
    }
}