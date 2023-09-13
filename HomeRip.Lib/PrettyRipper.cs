using FlatSharp;
using PKHeX.Core;

namespace HomeRip.Lib;

public static class PrettyRipper
{
    public static void RipFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            bool hayabusa = Path.GetFileName(file).StartsWith("hayabusa");
            if (hayabusa)
                RipFileLA(file);
            else
                RipFile(file);
        }
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
            writer.WriteLine($"{GetSafe(spec, entry.Species)} {entry.Form} {entry.IsPresentInGame}");
            writer.WriteLine($"Type: {(MoveType)entry.Type1}{(entry.Type1 != entry.Type2 ? $"/{(MoveType)entry.Type2}" : "")}");
            writer.WriteLine($"Base Stats: {entry.HP:000}.{entry.ATK:000}.{entry.DEF:000}.{entry.SPA:000}.{entry.SPD:000}.{entry.SPE:000}");
            writer.WriteLine($"Abilities: {GetSafe(game.Ability, entry.Ability1)}/{GetSafe(game.Ability, entry.Ability2)}/{GetSafe(game.Ability, entry.AbilityH)}");
            foreach (var move in entry.Moves)
            {
                // PLA is not exactly this format. We're being silly with Mastery Level here.
                // Other formats don't have this field.
                var line = $"\t{move.Level}\t{move.Move}\t{GetSafe(moves, move.Move)}";
                writer.WriteLine(line);
            }

            writer.WriteLine();
        }
    }

    public static string GetSafe(IReadOnlyList<string> arr, int index)
    {
        if (index < 0 || index >= arr.Count)
            return $"{index}";
        return arr[index];
    }

    private static void RipFileLA(string file)
    {
        Console.WriteLine($"Pretty ripping {file}...");
        var data = File.ReadAllBytes(file);
        var table = DeserializeFrom<PersonalTableLA>(data);

        // Write the result to a text file
        var game = GameInfo.GetStrings("en");
        var spec = game.Species;
        var moves = game.Move;
        using var writer = new StreamWriter(Path.ChangeExtension(file, ".pretty.txt"));
        foreach (var entry in table.Table)
        {
            writer.WriteLine($"{spec[entry.Species]} {entry.Form} {entry.IsPresentInGame}");
            foreach (var move in entry.Moves)
            {
                // PLA is not exactly this format. We're being silly with Mastery Level here.
                // Other formats don't have this field.
                string line = $"\t{move.Level}\t{move.LevelMastery}\t{move.Move}\t{moves[move.Move]}";
                writer.WriteLine(line);
            }

            writer.WriteLine();
        }
    }

    private static T DeserializeFrom<T>(byte[] data) where T : class, IFlatBufferSerializable<T> => T.GreedyMutableSerializer.Parse(data);
}
