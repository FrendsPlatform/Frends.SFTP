﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Result object of one individual file transfer.
    /// </summary>
    internal class SingleFileTransferResult
    {
        internal bool Success { get; set; }

        internal bool ActionSkipped { get; set; }

        internal IList<string> ErrorMessages { get; set; }

        internal string TransferredFile { get; set; }

        internal string TransferredFilePath { get; set; }

        internal SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}