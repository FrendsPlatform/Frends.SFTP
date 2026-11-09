global using SFTP = Frends.SFTP.UploadFiles.Tests.Lib.SftpTestAdapter;

using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests.Lib;

internal static class SftpTestAdapter
{
    internal static Task<Result> UploadFiles(
        Source source,
        Destination destination,
        Connection connection,
        Options options,
        Info info,
        CancellationToken cancellationToken) =>
        Frends.SFTP.UploadFiles.SFTP.UploadFiles(
            new Input
            {
                Source = source,
                Destination = destination,
                Info = info,
            },
            connection,
            options,
            cancellationToken);
}
