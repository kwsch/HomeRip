using FlatSharp;
using PKHeX.Core;

namespace HomeRip.Lib;

public static class PrettyRipper
{
    public static void RipFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
            RipFile(file);
    }

    private static void RipFile(string file)
    {
        Console.WriteLine($"Pretty ripping {file}...");
        var data = File.ReadAllBytes(file);
        var table = DeserializeFrom<PersonalTable>(data);

        // Write the result to a text file
        var game = GameInfo.GetStrings("en");
        var spec = game.Species;
        var moves = game.Move;
        using var writer = new StreamWriter(Path.ChangeExtension(file, ".pretty.txt"));
        foreach (var entry in table.Table)
        {
            writer.WriteLine($"{spec[entry.Species]} {entry.Form} {entry.Flag}");
            foreach (var move in entry.Moves)
            {
                // PLA is not exactly this format. We're being silly with Mastery Level here.
                // Other formats don't have this field.
                string line;
                if (move.LevelMastery == 0)
                    line = $"\t{move.Level}\t{moves[move.Move]}\t{move.Move}";
                else
                    line = $"\t{move.Move}\t{moves[move.Level]}\t{move.Level}\t{move.LevelMastery}";
                writer.WriteLine(line);
            }

            writer.WriteLine();
        }
    }

    private static T DeserializeFrom<T>(byte[] data) where T : class, IFlatBufferSerializable<T> => T.GreedyMutableSerializer.Parse(data);
}
