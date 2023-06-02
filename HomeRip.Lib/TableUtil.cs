using System.Collections;

namespace HomeRip.Lib;

// Loaned from pkNX with permission.
public static class TableUtil
{
    /// <summary>
    /// Converts an enumerable list of <see cref="T"/> to a tab separated sheet.
    /// </summary>
    /// <typeparam name="T">Object Type</typeparam>
    /// <param name="arr">Array of type T</param>
    /// <returns>2 dimensional sheet of cells</returns>
    public static string GetTable<T>(IEnumerable<T> arr) where T : notnull => string.Join(Environment.NewLine, GetTableRaw(arr));

    private const char sep = '\t';
    private static IEnumerable<string> GetTableRaw<T>(IEnumerable<T> arr) where T : notnull
        => Table(arr).Select(row => string.Join(sep, row));

    private static IEnumerable<IEnumerable<string>> Table<T>(IEnumerable<T> arr) where T : notnull
    {
        var type = typeof(T);
        yield return GetNames(type);
        foreach (var z in arr)
            yield return GetValues(z, type);
    }

    private static IEnumerable<string> GetNames(Type type)
    {
        foreach (var z in type.GetProperties())
            yield return z.Name;
        foreach (var z in type.GetFields())
            yield return z.Name;
    }

    private static IEnumerable<string> GetValues(object obj, Type type)
    {
        foreach (var z in type.GetProperties())
            yield return GetFormattedString(z.GetValue(obj, null));

        foreach (var z in type.GetFields())
            yield return GetFormattedString(z.GetValue(obj));
    }

    private static string GetFormattedString(object? obj)
    {
        if (obj == null)
            return string.Empty;
        if (obj is ulong u)
            return u.ToString("X16");
        if (obj is IEnumerable x and not string)
            return string.Join('|', JoinEnumerator(x.GetEnumerator()).Select(GetFormattedString));

        var objType = obj.GetType();
        if (objType.IsEnum)
            return obj.ToString() ?? string.Empty;
        var mi = objType.GetMethods().First(z => z.Name == nameof(obj.ToString));
        if (mi.DeclaringType == objType)
            return obj.ToString() ?? string.Empty;

        var props = objType.GetProperties().Select(z => GetFormattedString(z.GetValue(obj)));
        var fields = objType.GetFields().Select(z => GetFormattedString(z.GetValue(obj)));
        return string.Join('|', props.Concat(fields));
    }

    private static IEnumerable<object> JoinEnumerator(IEnumerator x)
    {
        while (x.MoveNext())
            yield return x.Current;
    }
}
