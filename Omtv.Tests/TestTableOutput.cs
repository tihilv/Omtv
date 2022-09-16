using System.Text;
using Omtv.Api.Model;
using Omtv.Api.Processing;

namespace Omtv.Tests;

internal class TestTableOutput : ITableOutput
{
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    
    public void TableStart(Document document)
    {
        _stringBuilder.Append("t,");
    }

    public void RowStart(Document document)
    {
        _stringBuilder.Append("r,");
    }

    public void Cell(Document document)
    {
        if (document.CurrentTable.CurrentRow.CurrentCell.Spanned)
            _stringBuilder.Append($"c-,");
        else
            _stringBuilder.Append($"c:{document.CurrentTable.CurrentRow.CurrentCell.Content},");
    }

    public void RowEnd(Document document)
    {
        _stringBuilder.Append("/r,");
    }

    public void TableEnd(Document document)
    {
        _stringBuilder.Append("/t,");
    }

    public override String ToString()
    {
        return _stringBuilder.ToString().TrimEnd(',');
    }
}