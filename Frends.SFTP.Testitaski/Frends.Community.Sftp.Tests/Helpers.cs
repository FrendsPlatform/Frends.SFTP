using System;
using System.Reflection;
using Frends.Community.Sftp.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Frends.Community.Sftp.Tests
{
    internal class Helpers
    {
        public static Sftp CreateSftpTaskInstance()
        {
            var sftpService = new Lazy<ISftpService>(() =>
            {
                var serviceProvider = new ServiceCollection()
                                            .AddTransient<ISftpService, MockSftpService>()
                                            .BuildServiceProvider();

                return serviceProvider.GetService<ISftpService>();
            });

            var sftpTask = new Sftp();
            SetPrivateFieldValue(sftpTask, "_sftpService", sftpService);

            return sftpTask;
        }

        public static void SetPrivateFieldValue(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
