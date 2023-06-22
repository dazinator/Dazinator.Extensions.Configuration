namespace Dazinator.Extensions.Configuration.Json;

public static class StreamUtils
{
    public static Stream CreateStreamFromString(string str)
    {
        var memStream = new MemoryStream();
        var textWriter = new StreamWriter(memStream);
        textWriter.Write(str);
        textWriter.Flush();
        memStream.Seek(0, SeekOrigin.Begin);
        return memStream;
    }
}
