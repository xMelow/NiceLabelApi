using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NiceLabel.SDK;
using NiceLabelApi.DTO;

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
        
        public void PrintLabelVariables(Stream labelStream, List<Dictionary<string, string>> variables, string printerName, int amount)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            
            if (printerName != null)
                label.PrintSettings.PrinterName = printerName;

            var sessionPrint = label.StartSessionPrint();
            
            foreach (var labelData in variables)
            {
                foreach (var data in labelData)
                {
                    label.Variables[data.Key].SetValue(data.Value);
                }
                
                label.SessionPrint(amount, sessionPrint);
            }
            label.EndSessionPrint(sessionPrint);
        }

        public List<IPrinter> GetPrinters()
        {
            return _niceLabelPrintEngine.Printers.ToList();
        }

        public byte[] GetLabelPreview(Stream labelStream, int width, int height)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            ILabelPreviewSettings previewSettings = new LabelPreviewSettings
            {
                Width = width,
                Height = height,
                ImageFormat = "PNG"
            };
            var preview = label.GetLabelPreview(previewSettings);

            if (preview is byte[] bytes)
                return bytes;

            throw new InvalidOperationException("Label preview does not return byte type");
        }

        public List<byte[]> GetLabelPreviewBatch(Stream labelStream, List<Dictionary<string,string>> variables)
        {
            List<byte[]> result = new List<byte[]>();
            
            using (var label = _niceLabelPrintEngine.OpenLabel(labelStream))
            {
                ILabelPreviewSettings labelPreviewSettings = new LabelPreviewSettings
                {
                    ImageFormat = "PNG",
                    UseDefaultSize = true
                };
                
                foreach (var labelData in variables)
                {
                    foreach (var data in labelData)
                    {
                        if (!label.Variables.Any(e => e.Name == data.Key))
                            throw new InvalidOperationException($"Variable '{data.Key}' does not exist in the label template.");
                    
                        label.Variables[data.Key].SetValue(data.Value);
                    }

                    var preview = label.GetLabelPreview(labelPreviewSettings);
                
                    if (preview is byte[] bytes)
                        result.Add(bytes);
                    else 
                        throw new InvalidOperationException("label preview does not return bytes");
                }
            }
            return result;
        }
        
        public void PrintLabelWithSettings(Stream labelStream, Dictionary<string, string> labelVariables, Dictionary<string, string> printSettings, string printerName)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
            var variablesList = string.Join("\n", labelVariables.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var quantity = 1;
            
            label.PrintSettings.PrinterName = printerName;
            label.SetVariableValues(variablesList, true);
            
            // PrintSpeed only accepts whole-number strings: decimals (e.g. "1.5") are silently
            // ignored (driver keeps its previous speed) rather than erroring, even though the
            // printer driver itself supports fractional speeds via its own Preferences dialog.
            foreach (var setting in printSettings)
            {
                label.PrintSettings.SetParameter(setting.Key, setting.Value);
            }
            label.Print(quantity);
        }

        public void PrintToPdf(List<Stream> labels, Dictionary<string, string> variables, string outputFileName, bool appendToFile)
        {
            foreach (var labelStream in labels)
            {
                ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
                label.PrintSettings.PrintToPdf = true;
                label.PrintSettings.OutputFileName = outputFileName;

                var variablesList = string.Join("\n", variables.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                label.SetVariableValues(variablesList, true);
                
                if (appendToFile)
                {
                    label.PrintSettings.AppendToOutputFile = true;
                }

                label.Print(1);
            }
        }
        
        public void PrintLabelBatch(List<Stream> labels, Dictionary<string, string> variables, Dictionary<string, string> printerSettings, string printerName)
        {
            foreach (var labelStream in labels)
            {
                ILabel label = _niceLabelPrintEngine.OpenLabel(labelStream);
                var variablesList = string.Join("\n", variables.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                label.SetVariableValues(variablesList, true);
                foreach (var setting in printerSettings)
                {
                    label.PrintSettings.SetParameter(setting.Key, setting.Value);
                }
                label.PrintSettings.PrinterName = printerName;
                
                label.Print(1);
            }
        }
        
        public NiceLabelLabel GetLabelDetails(Stream file)
        {
            ILabel label = _niceLabelPrintEngine.OpenLabel(file);
            var labelVariables = label.Variables?.Select(v => v.Name).ToList() ?? new List<string>();
            var labelFunctions = label.Functions?.Select(f => f.Name).ToList() ?? new List<string>();
            var labelObjects = label.LabelObjects?.Select(o => o.Name).ToList() ?? new List<string>();
            var labelConversionIssues = label.ConversionIssues?.Select(issue => issue.ToString()).ToList() ?? new List<string>();
            
            List<NiceLabelDataSet> labelDatasets = null;
            if (label.DataSets != null)
            {
                labelDatasets = new List<NiceLabelDataSet>();
                
                for (int i = 0; i < label.DataSets.Count; i++)
                {
                    labelDatasets.Add(new NiceLabelDataSet
                    {
                        IsFileDatabase = label.DataSets[i].IsFileDatabase,
                        FileDatabaseFileName = label.DataSets[i].FileDatabaseFileName,
                        ConnectionString = label.DataSets[i].ConnectionString,
                        TableName = label.DataSets[i].TableName,
                        IsCustomSql = label.DataSets[i].IsCustomSql,
                        CustomSql = label.DataSets[i].CustomSql,
                        Fields = label.DataSets[i].Fields?.Select(v => v.FieldName).ToList() ?? new List<string>()
                    });
                }
            }

            NiceLabelLabelSettings labelSettings = null;
            if (label.LabelSettings != null)
            {
                NiceLabelLabelPageLayout labelPageLayout = null;
                if (label.LabelSettings.PageLayout != null)
                {
                    labelPageLayout = new NiceLabelLabelPageLayout
                    {
                        MediaWidth = label.LabelSettings.PageLayout.MediaWidth,
                        MediaHeight = label.LabelSettings.PageLayout.MediaHeight,
                        Columns = label.LabelSettings.PageLayout.Columns,
                        Rows = label.LabelSettings.PageLayout.Rows,
                        HorizontalGap = label.LabelSettings.PageLayout.HorizontalGap,
                        VerticalGap = label.LabelSettings.PageLayout.VerticalGap,
                        HorizontalOffset = label.LabelSettings.PageLayout.HorizontalOffset,
                        VerticalOffset = label.LabelSettings.PageLayout.VerticalOffset,
                        HorizontalRadius = label.LabelSettings.PageLayout.HorizontalRadius,
                        VerticalRadius = label.LabelSettings.PageLayout.VerticalRadius
                    };
                }
                
                labelSettings = new NiceLabelLabelSettings
                {
                    Width = label.LabelSettings.Width,
                    Height = label.LabelSettings.Height,
                    OriginalPrinterName = label.LabelSettings.OriginalPrinterName,
                    OriginalPrinterDriver = label.LabelSettings.OriginalPrinterDriver,
                    DimensionUnit = label.LabelSettings.DimensionUnit,
                    PageLayout = labelPageLayout
                };
            }

            return new NiceLabelLabel
            {
                LabelSettings = labelSettings,
                ConversionIssues = labelConversionIssues,
                Variables = labelVariables,
                Functions = labelFunctions,
                LabelObjects = labelObjects,
                DataSet = labelDatasets,
                FilePath = label.FilePath,
                Author = label.Author,
                Title = label.Title
            };
        }
    }
}
