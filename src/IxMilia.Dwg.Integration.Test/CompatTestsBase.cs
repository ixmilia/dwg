using System;
using System.Diagnostics;
using System.IO;
using IxMilia.Dwg.Test;
using Xunit;

namespace IxMilia.Dwg.Integration.Test
{
    public abstract class CompatTestsBase : TestBase
    {
        public class TestCaseDirectory
        {
            public string DirectoryPath { get; }

            public TestCaseDirectory(string subDirectoryName)
            {
                DirectoryPath = Path.Combine(
                    Path.GetDirectoryName(GetType().Assembly.Location),
                    "integration-tests",
                    subDirectoryName
                );
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }

                Directory.CreateDirectory(DirectoryPath);
            }
        }

        protected void WaitForProcess(string fileName, string arguments, TimeSpan timeout)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            var proc = Process.Start(psi);
            if (Debugger.IsAttached)
            {
                proc.WaitForExit();
            }
            else
            {
                var exited = proc.WaitForExit((int)timeout.TotalMilliseconds);
                if (!exited)
                {
                    proc.Kill(entireProcessTree: true);
                    throw new Exception("The process failed to exit within the timeout.");
                }
            }

            Assert.Equal(0, proc.ExitCode);
        }
    }
}
