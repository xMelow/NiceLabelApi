using System.Collections.Generic;
using System.IO;

namespace NiceLabelApi.Services
{
    public interface INiceLabelService
    {
        IReadOnlyList<string> GetVariables(Stream file);
        void PrintLabel(Stream label);
    }
}