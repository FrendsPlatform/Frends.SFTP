using NUnit.Framework;

namespace Frends.SFTP.WriteFile.Tests;

public class WriteFileTestBase
{
    [TearDown]
    public void TearDown()
    { 
        Helpers.DeleteDestinationFiles();
    }
}

