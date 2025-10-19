using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Sample.XUnitTests;

public class TestOutputHelperTextWriterAdapter :
    TextWriter
{
    readonly ITestOutputHelper _output;
    string _currentLine = "";

    public TestOutputHelperTextWriterAdapter(ITestOutputHelper output)
    {
        _output = output;
        Encoding = Encoding.UTF8;
    }

    public override Encoding Encoding { get; }
    public bool Enabled { get; set; } = true;

    public override void Write(char value)
    {
        if (!Enabled) return;

        if (value == '\n')
            WriteCurrentLine();
        else
            _currentLine += value;
    }

    void WriteCurrentLine()
    {
        _output.WriteLine(_currentLine);
        _currentLine = "";
    }

    protected override void Dispose(bool disposing)
    {
        if (_currentLine != "") WriteCurrentLine();

        base.Dispose(disposing);
    }
}