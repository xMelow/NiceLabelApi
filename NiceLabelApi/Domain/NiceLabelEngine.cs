using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NiceLabel.SDK;

namespace NiceLabelApi.Domain
{
    public class NiceLabelEngine
    {
        private readonly IPrintEngine _niceLabelPrintEngine;
        
        public NiceLabelEngine()
        {
            PrintEngineFactory.SDKFilesPath = @"C:\Program Files\NiceLabel\NiceLabel 10\bin.net";
            _niceLabelPrintEngine = PrintEngineFactory.PrintEngine;
            _niceLabelPrintEngine.Initialize();
        }

        public IReadOnlyList<string> GetVariables(Stream labelStream)
        {
            var label = _niceLabelPrintEngine.OpenLabel(labelStream);
            var result = label.Variables
                .Select(v => v.Name)
                .ToList()
                .AsReadOnly();
        
            return result;
        }

        public void PrintLabel(Stream labelStream, string printerIp)
        {
            //convert labelStream to label file name
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            if (printerIp != null)
                label.PrintSettings.PrinterName = printerIp; // update request to use installed NiceLabel printer (nicelabel printer list) or find the printer in the list of installed printers (need printer name)
            
            // label quantity also needs to be in the request
            label.Print(1);
        }
    }
}