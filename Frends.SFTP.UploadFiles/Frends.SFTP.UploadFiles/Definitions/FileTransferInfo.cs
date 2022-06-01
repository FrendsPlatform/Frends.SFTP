namespace Frends.SFTP.UploadFiles.Definitions
{
    internal class FileTransferInfo
    {
        public string TransferName { get; set; } // 0

        public Guid BatchId { get; set; } // 1

        public string SourceFile { get; set; } // 2

        public DateTime TransferStarted { get; set; } // 3

        public DateTime TransferEnded { get; set; } // 4

        public string DestinationFile { get; set; } // 5

        public long FileSize { get; set; } // 6

        public string ErrorInfo { get; set; } // 7

        public TransferResult Result { get; set; } // 10

        public string DestinationAddress { get; set; } // 13

        public string SourcePath { get; set; } // 15

        public string DestinationPath { get; set; } // 16

        public string ServiceId { get; set; } // 17

        public string RoutineUri { get; set; } // 18

        public Guid SingleFileTransferId { get; set; }

        public override string ToString()
        {
            return string.Format(
            $@"{ErrorInfo}

            TransferName: {TransferName}
            BatchId: {BatchId}
            Sourcefile: {SourceFile}
            DestinationFile: {DestinationFile}
            SourcePath: {SourcePath}
            DestinationPath: {DestinationPath}
            DestinationAddress: {DestinationAddress}
            TransferStarted: {TransferStarted}
            TransferEnded: {TransferEnded}
            TransferResult: {Result}
            FileSize: {FileSize} bytes
            ServiceId: {string.Empty}
            RoutineUri: {ServiceId}");
        }
    }
}
