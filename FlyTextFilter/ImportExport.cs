using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using Newtonsoft.Json;

namespace FlyTextFilter;

public class ImportExport
{
    private const string ExportPrefix = "FTF_";

    public static string ExportFlyTextSettings(ConcurrentDictionary<FlyTextKind, FlyTextSetting> setting)
        => CompressString(JsonConvert.SerializeObject(setting, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new ConcurrentDictionaryConverter<FlyTextKind, FlyTextSetting>() },
        }));

    public static ConcurrentDictionary<FlyTextKind, FlyTextSetting> ImportFlyTextSettings(string import)
        => JsonConvert.DeserializeObject<ConcurrentDictionary<FlyTextKind, FlyTextSetting>>(DecompressString(import), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Converters = { new ConcurrentDictionaryConverter<FlyTextKind, FlyTextSetting>() },
        }) ?? throw new InvalidOperationException();

    public static string CompressString(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            gs.Write(bytes, 0, bytes.Length);
        }

        return ExportPrefix + Convert.ToBase64String(mso.ToArray());
    }

    public static string DecompressString(string s)
    {
        if (!s.StartsWith(ExportPrefix))
            throw new ApplicationException("This is not a FlyTextFilter export.");
        var data = Convert.FromBase64String(s[ExportPrefix.Length..]);
        var lengthBuffer = new byte[4];
        Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
        var uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

        var buffer = new byte[uncompressedSize];
        using (var ms = new MemoryStream(data))
        {
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            _ = gzip.Read(buffer, 0, uncompressedSize);
        }

        return Encoding.UTF8.GetString(buffer);
    }
}
