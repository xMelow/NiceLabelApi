using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Console.WriteLine(label.PrintSettings.PrinterName);
            System.Diagnostics.Debug.WriteLine(label.PrintSettings.PrinterName);
            if (printerName != null)
                label.PrintSettings.PrinterName = printerName;
            
            label.Print(quantity);
        }
    }
}