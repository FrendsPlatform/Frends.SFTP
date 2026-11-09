namespace Frends.SFTP.DeleteFiles.Tests;

using System;
using Frends.SFTP.DeleteFiles.Definitions;
using Frends.SFTP.DeleteFiles.Helpers;
using NUnit.Framework;

[TestFixture]
internal class ErrorHandlerTests
{
    [Test]
    public void Handle_ReturnsErrorResultWhenThrowingIsDisabled()
    {
        var exception = new InvalidOperationException("SFTP request failed.");
        var options = new Options { ThrowErrorOnFailure = false };

        var result = exception.Handle(options);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(exception.Message, result.Error.Message);
        Assert.AreSame(exception, result.Error.AdditionalInfo);
    }
}
