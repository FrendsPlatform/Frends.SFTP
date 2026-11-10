namespace Frends.SFTP.DeleteDirectory.Tests;

using System;
using System.Threading.Tasks;
using Frends.SFTP.DeleteDirectory.Enums;
using NUnit.Framework;

/// <summary>
/// Test class.
/// </summary>
[TestFixture]
internal class UnitTests : UnitTestBase
{
    [Test]
    public async Task DeleteDirectory_SimpleDelete()
    {
        var result = await SFTP.DeleteDirectory(_input, _connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public async Task DeleteDirectory_TestWithDirectoryNotExisting_NotExistsOptions_Info()
    {
        _input.Directory = "/does/not/exist";
        _options.ThrowNotExistError = NotExistsOptions.Skip;
        var result = await SFTP.DeleteDirectory(_input, _connection, _options, default);
        Assert.AreEqual("Directory /does/not/exist does not exists.", result.ErrorMessage);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.Count);
    }

    [Test]
    public void DeleteDirectory_TestWithDirectoryNotExisting_NotExistsOptions_Throw()
    {
        _input.Directory = "/does/not/exist";
        _options.ThrowNotExistError = NotExistsOptions.Throw;
        Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteDirectory(_input, _connection, _options, default));
    }
}