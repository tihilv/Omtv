using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Omtv.Api.Model;
using Omtv.Api.Processing;

namespace Omtv.Csv
{
    public class CsvTableOutput: ITableOutput
    {
        private readonly StreamWriter _writer;
        private readonly String _separator;

        private Boolean _newLine;
        
        public CsvTableOutput(Stream outputStream, String separator = ";")
        {
            _separator = separator;
            _writer = new StreamWriter(outputStream, Encoding.UTF8, 1024, true);
        }

        public async ValueTask StartAsync(Document document)
        {
        }

        public async ValueTask TableStartAsync(Document document)
        {
            if (!String.IsNullOrEmpty(document.Table.Name))
                await _writer.WriteLineAsync(document.Table.Name);
        }

        public async ValueTask RowStartAsync(Document document)
        {
            _newLine = true;
        }

        public async ValueTask CellAsync(Document document)
        {
            if (!_newLine)
                await _writer.WriteAsync($"{_separator}{document.Table.Row.Cell.Content}");
            else
            {
                await _writer.WriteAsync(document.Table.Row.Cell.Content);
                _newLine = false;
            }
        }

        public async ValueTask RowEndAsync(Document document)
        {
            await _writer.WriteLineAsync();
        }

        public async ValueTask TableEndAsync(Document document)
        {
            
        }

        public async ValueTask EndAsync(Document document)
        {
            await _writer.FlushAsync();
        }
    }
}