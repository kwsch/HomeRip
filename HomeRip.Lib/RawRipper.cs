using FlatSharp;

namespace HomeRip.Lib;

public static class RawRipper
{
    public static void RipFiles<TTable, TSub>(IEnumerable<string> files, Func<TTable, IEnumerable<TSub>>? sel = null)
        where TTable : class, IFlatBufferSerializable<TTable> where TSub : notnull
    {
        foreach (var file in files)
            RipFile(file, sel);
    }

    public static void RipFile<TTable, TSub>(string file, Func<TTable, IEnumerable<TSub>>? sel = null) where TTable : class, IFlatBufferSerializable<TTable> where TSub : notnull
    {
        Console.WriteLine($"Ripping {file}...");
        var data = File.ReadAllBytes(file);
        TTable result = DeserializeFrom<TTable>(data);

        var jsonFile = Path.ChangeExtension(file, ".json");
        DumpJson(result, jsonFile);

        if (sel != null)
        {
            var list = sel(result);
            var table = TableUtil.GetTable(list);
            var tableFile = Path.ChangeExtension(file, ".tsv");
            File.WriteAllText(tableFile, table);
        }
    }

    private static T DeserializeFrom<T>(byte[] data) where T : class, IFlatBufferSerializable<T> => T.GreedyMutableSerializer.Parse(data);

    private static void DumpJson<T>(T flat, string filePath) where T : class
    {
        var opt = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        var json = System.Text.Json.JsonSerializer.Serialize(flat, opt);

        var fileName = Path.ChangeExtension(filePath, ".json");
        File.WriteAllText(fileName, json);
    }
}
