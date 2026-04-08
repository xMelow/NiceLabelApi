using System.Collections.Generic;
using NiceLabel.SDK;

namespace NiceLabelApi.Models
{
    public class PrinterResponse
    {
        public List<IPrinter> Printers { get; set; }
    }
}