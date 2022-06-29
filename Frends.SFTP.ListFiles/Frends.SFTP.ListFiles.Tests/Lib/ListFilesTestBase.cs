using NUnit.Framework;

namespace Frends.SFTP.ListFiles.Tests;

public class ListFilesTestBase
{
    [OneTimeSetUp]
    public void Setup()
    {
        Helpers.GenerateDummyFiles();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}

