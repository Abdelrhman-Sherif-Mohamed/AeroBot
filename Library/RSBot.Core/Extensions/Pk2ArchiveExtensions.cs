using System;
using System.Drawing;
using System.IO;
using RSBot.Core.Client;
using RSBot.FileSystem;

namespace RSBot.Core.Extensions;

public static class Pk2Extensions
{
    /// <summary>
    ///     Reads an image from the game archives. Supports DDJ/DDS files as well as the usual image
    ///     formats (png, jpg, bmp, gif) so that custom clients that ship different media files keep working.
    /// </summary>
    /// <param name="file">The archive file.</param>
    /// <returns>The decoded image or an empty placeholder if the media file is not supported.</returns>
    public static Image ToImage(this IFile file)
    {
        if (file == null)
            return new Bitmap(16, 16);

        byte[] buffer;

        try
        {
            buffer = file.OpenRead().ReadAllBytes();
        }
        catch
        {
            return new Bitmap(16, 16);
        }

        try
        {
            //Some clients store regular image files inside their media archive
            var standardImage = TryLoadStandardImage(buffer);
            if (standardImage != null)
                return standardImage;

            //DDJ files contain a short header followed by a DDS image. Do not assume the header
            //is always 20 bytes, just search for the DDS signature.
            var ddsOffset = FindDdsOffset(buffer);
            if (ddsOffset < 0)
                return new Bitmap(16, 16);

            var ddsBuffer = new byte[buffer.Length - ddsOffset];
            Array.Copy(buffer, ddsOffset, ddsBuffer, 0, ddsBuffer.Length);

            return DDSImage.ToBitmap(ddsBuffer);
        }
        catch
        {
            return new Bitmap(16, 16);
        }
    }

    private static Image TryLoadStandardImage(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 8)
            return null;

        var isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
        var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        var isBmp = buffer[0] == 0x42 && buffer[1] == 0x4D;
        var isGif = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;

        if (!isPng && !isJpeg && !isBmp && !isGif)
            return null;

        try
        {
            using var stream = new MemoryStream(buffer);
            using var image = Image.FromStream(stream);

            //Clone the image so it does not depend on the stream anymore
            return new Bitmap(image);
        }
        catch
        {
            return null;
        }
    }

    private static int FindDdsOffset(byte[] buffer)
    {
        const int maxHeaderLength = 256;

        var searchLength = Math.Min(buffer.Length - 4, maxHeaderLength);
        for (var i = 0; i <= searchLength; i++)
        {
            if (buffer[i] != 0x44 || buffer[i + 1] != 0x44 || buffer[i + 2] != 0x53 || buffer[i + 3] != 0x20)
                continue;

            //The DDS header is 128 bytes long and must fit into the buffer
            if (buffer.Length - i >= 128)
                return i;
        }

        return -1;
    }
}
