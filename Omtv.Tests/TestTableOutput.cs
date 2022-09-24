﻿using System.Text;
using Omtv.Api.Model;
using Omtv.Api.Processing;

namespace Omtv.Tests;

internal class TestTableOutput : ITableOutput
{
    private readonly StringBuilder _stringBuilder = new StringBuilder();

    public ValueTask StartAsync(Document document)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask TableStartAsync(Document document)
    {
        _stringBuilder.Append("t,");
        return ValueTask.CompletedTask;
    }

    public ValueTask RowStartAsync(Document document)
    {
        _stringBuilder.Append("r,");
        return ValueTask.CompletedTask;
    }

    public ValueTask CellAsync(Document document)
    {
        if (document.Table.Row.Cell.Spanned)
            _stringBuilder.Append($"c-,");
        else
            _stringBuilder.Append($"c:{document.Table.Row.Cell.Content},");
        return ValueTask.CompletedTask;
    }

    public ValueTask RowEndAsync(Document document)
    {
        _stringBuilder.Append("/r,");
        return ValueTask.CompletedTask;
    }

    public ValueTask TableEndAsync(Document document)
    {
        _stringBuilder.Append("/t,");
        return ValueTask.CompletedTask;
    }

    public ValueTask EndAsync(Document document)
    {
        return ValueTask.CompletedTask;
    }

    public override String ToString()
    {
        return _stringBuilder.ToString().TrimEnd(',');
    }
}