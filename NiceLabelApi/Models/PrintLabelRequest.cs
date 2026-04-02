using System.IO;

namespace NiceLabelApi.Models
{
    public class PrintLabelRequest
    {
        public Stream LabelFile { get; set; }
        public int Quantity { get; set; }
        public string PrinterName { get; set; }
    }
}