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

        public async ValueTask TableStartAsync(Document document)
        {
            if (!String.IsNullOrEmpty(document.CurrentTable.Name))
                await _writer.WriteLineAsync(document.CurrentTable.Name);
        }

        public async ValueTask RowStartAsync(Document document)
        {
            _newLine = true;
        }

        public async ValueTask CellAsync(Document document)
        {
            if (!_newLine)
                await _writer.WriteAsync($"{_separator}{document.CurrentTable.CurrentRow.CurrentCell.Content}");
            else
            {
                await _writer.WriteAsync(document.CurrentTable.CurrentRow.CurrentCell.Content);
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

        public async ValueTask DoneAsync(Document document)
        {
            await _writer.FlushAsync();
        }
    }
}