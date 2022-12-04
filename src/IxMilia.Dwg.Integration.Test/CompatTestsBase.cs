using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.Unicode,
                    StandardErrorEncoding = Encoding.Unicode,
                    UseShellExecute = false,
                },
            };
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            proc.OutputDataReceived += (_, args) => stdout.AppendLine(args.Data);
            proc.ErrorDataReceived += (_, args) => stderr.AppendLine(args.Data);
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            var exited = proc.WaitForExit((int)timeout.TotalMilliseconds);
            proc.CancelOutputRead();
            proc.CancelErrorRead();
            if (!exited)
            {
                proc.Kill();
            }

            var message = $"STDOUT:\n{stdout}\nSTDERR:\n{stderr}";
            Assert.True(exited && proc.ExitCode == 0, message);
        }
    }
}
