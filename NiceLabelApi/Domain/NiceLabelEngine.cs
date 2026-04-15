using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        public void PrintLabelVariables(Stream labelStream, List<Dictionary<string, string>> variables, string printerName)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            var quantity = 1;
            
            if (printerName != null)
                label.PrintSettings.PrinterName = printerName;

            var sessionPrint = label.StartSessionPrint();
            
            foreach (var labelData in variables)
            {
                foreach (var data in labelData)
                {
                    label.Variables[data.Key].SetValue(data.Value);
                }
                
                label.SessionPrint(quantity, sessionPrint);
            }
            label.EndSessionPrint(sessionPrint);
        }

        public List<IPrinter> GetPrinters()
        {
            return _niceLabelPrintEngine.Printers.ToList();
        }

        public byte[] GetLabelPreview(Stream labelStream, int width, int height, bool previewToFile, string destination)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            ILabelPreviewSettings previewSettings = new LabelPreviewSettings();
            previewSettings.Width = width;
            previewSettings.Height = height;
            previewSettings.PreviewToFile = previewToFile;
            previewSettings.ImageFormat = "PNG";
            previewSettings.Destination = destination;
            var preview = label.GetLabelPreview(previewSettings);

            if (preview is byte[] bytes)
                return bytes;

            throw new InvalidOperationException("Label preview does not return byte type");
        }
    }
}