using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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

        public void PrintLabel(Stream labelStream, int quantity, string printerName)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            
            if (printerName != null)
                label.PrintSettings.PrinterName = printerName;
            
            label.Print(quantity);
        }
        
        public void PrintLabelVariables(Stream labelStream, Dictionary<string, string> variables, string printerName)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            
            // set label variables
            
            // set quantity to the amount of variables in the variables?
            var quantity = 1;
            
            if (printerName != null)
                label.PrintSettings.PrinterName = printerName;
            
            label.Print(quantity);
        }

        public List<IPrinter> GetPrinters()
        {
            return _niceLabelPrintEngine.Printers.ToList();
        }
    }
}