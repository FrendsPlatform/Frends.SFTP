namespace Frends.SFTP.DownloadFiles.Definitions
{
    internal class FileOperations
    {
        internal static async Task AppendAsync(string source, string remoteFile, bool addNewLine, CancellationToken cancellationToken)
        {
            if (addNewLine)
            {
                using (var stream = File.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream))
                {
                    // Determines if there is a new line at the end of the file. If not, one is appended.
                    long readPosition = stream.Length == 0 ? 0 : -1;
                    stream.Seek(readPosition, SeekOrigin.End);
                    string end = reader.ReadToEnd();
                    if (end.Length != 0 && !end.Equals(Environment.NewLine, StringComparison.Ordinal))
                    {
                        writer.Write(Environment.NewLine);
                    }
                    writer.Flush();
                }
            }

            using (var srcStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            {
                using (var outStream = new FileStream(remoteFile, FileMode.Append, FileAccess.Write))
                {
                    byte[] buff = new byte[4096];
                    int r;
                    while ((r = await srcStream.ReadAsync(buff, 0, buff.Length, cancellationToken)) != 0)
                    {
                        await outStream.WriteAsync(buff, 0, r, cancellationToken);
                    }
                    outStream.Flush();
                }
            }
        }
            

        internal static async Task CopyAsync(string source, string remoteFile, bool overwrite, CancellationToken cancellationToken)
        {
            FileMode fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (Stream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            {
                using (Stream destinationStream = new FileStream(remoteFile, fileMode, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destinationStream, bufferSize: 81920, cancellationToken);
                }
            }
        }

        internal static async Task MoveAsync(string source, string remoteFile, CancellationToken cancellationToken)
        {
            await CopyAsync(source, remoteFile, false, cancellationToken);
            File.Delete(source);
        }
    }
}
