namespace HomeRip.Lib;

/// <summary>
/// Utility class for writing BinLinker files.
/// </summary>
public static class BinLinkerWriter
{
    public static byte[] Compress<T>(T[] arr, ReadOnlySpan<byte> ident, Func<T, byte[]> sel, int padTo = 0)
    {
        // Convert th
        var result = new byte[arr.Length][];
        for (int i = 0; i < arr.Length; i++)
            result[i] = sel(arr[i]);
        return Write32(result, ident, padTo);
    }

    public static byte[] Write32(byte[][] data, ReadOnlySpan<byte> identifier, int padTo = 4)
    {
        using var ms = new MemoryStream(4096);
        using var bw = new BinaryWriter(ms);

        bw.Write(identifier[..2]);
        bw.Write((ushort)data.Length);
        const int start = 0;

        // Preallocate the offset map
        int count = data.Length;
        for (int i = 0; i < count; i++)
            bw.Write((uint)0);
        bw.Write((uint)0);

        // Write each file, then update the offset map
        for (int i = 0; i < count; i++)
        {
            // Write File Offset
            var fileOffset = bw.BaseStream.Length - start;
            bw.Seek(start + 4 + (i * sizeof(uint)), SeekOrigin.Begin);
            bw.Write((uint)fileOffset);
            // Write File to Stream
            bw.Seek(0, SeekOrigin.End);
            bw.Write(data[i]);
            if (padTo != 0)
            {
                while ((ms.Position - start) % padTo != 0)
                    bw.Write((byte)0);
            }
        }

        // Cap the File
        {
            var fileOffset = bw.BaseStream.Length - start;
            bw.Seek(start + 4 + (count * sizeof(uint)), SeekOrigin.Begin);
            bw.Write((uint)fileOffset);
        }

        // Return the byte array
        return ms.ToArray();
    }
}
