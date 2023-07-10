using System.Diagnostics;
using System.Text;

namespace PowerManagerConfig.Tests
{
    internal sealed class TraceWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override async Task WriteAsync(string? value)
        {
            await Task.Run(() =>
            {
                Trace.WriteLine(value);
            });
        }

        public override async Task WriteLineAsync(string? value)
        {
            await Task.Run(() =>
            {
                Trace.WriteLine(value);
            });
        }
    }
}
