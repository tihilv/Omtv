using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    internal class ExcelTextManager
    {
        private readonly SharedStringTable _sharedStringTable;
        
        private readonly Dictionary<String, Int32> _texts;
        
        public SharedStringTable SharedStringTable => _sharedStringTable;

        internal ExcelTextManager()
        {
            _sharedStringTable = new SharedStringTable();
            _texts = new Dictionary<String, Int32>();
        }
        
        internal Int32 GetCellTextIndex(String text)
        {
            if (_texts.TryGetValue(text, out var index))
                return index;

            index = _sharedStringTable.Elements<SharedStringItem>().Count();
            _texts.Add(text, index);
            _sharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            return index;
        }
    }
}