using NUnit.Framework;
using Frends.SFTP.WriteFile.Definitions;

namespace Frends.SFTP.WriteFile.Tests;

public class WriteFileTestBase
{
    protected static Input _input;

    [SetUp]
    public void SetUp()
    {

    }

    [TearDown]
    public void TearDown()
    { 
        Helpers.DeleteDestinationFiles();
    }
}

