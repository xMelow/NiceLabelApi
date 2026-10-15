using System.Collections.Generic;

namespace NiceLabelApi.DTO
{
    public class NiceLabelLabel
    {
        public NiceLabelLabelSettings LabelSettings { get; set; }
        public List<string> ConversionIssues { get; set; }
        public List<string> Variables { get; set; }
        public List<string> Functions { get; set; }
        public List<string> LabelObjects { get; set; }
        public NiceLabelDataSet DataSet { get; set; }
        public string FilePath { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
    }

    public class NiceLabelLabelSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string OriginalPrinterName { get; set; }
        public string OriginalPrinterDriver { get; set; }
        public NiceLabelLabelPageLayout PageLayout { get; set; }
    }

    public class NiceLabelLabelPageLayout
    {
        public int MediaWidth { get; set; }
        public int MediaHeight { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int HorizontalGap { get; set; }
        public int VerticalGap { get; set; }
        public int HorizontalOffset { get; set; }
        public int VerticalOffset { get; set; }
        public int HorizontalRadius { get; set; }
        public int VerticalRadius { get; set; }
    }

    public class NiceLabelDataSet
    {
        public bool IsFileDatabase { get; set; }
        public string FileDatabaseFileName { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public bool IsCustomSql { get; set; }
        public string CustomSql { get; set; }
        public List<string> Fields { get; set; }
    }
}


