using System;
using System.Collections.Generic;

namespace Manx_Search_Data.TestUtil;

public class ClosedSourceDocument : Document
{
    internal override List<DocumentLine> LoadLocalFile()
    {
        throw new NotImplementedException();
    }
}