using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public void PrintLabel(Stream label, int quantity, string printerIpAddress)
        {
            _engine.PrintLabel(label, quantity, printerIpAddress);
        }
    }
}