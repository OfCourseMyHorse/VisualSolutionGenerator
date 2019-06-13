using System;
using System.Linq;
using NUnit.Framework;


namespace Tests
{
    [Category("MSBuild evaluation with environment variables")]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Environment.SetEnvironmentVariable("VSINSTALLDIR", @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community");
            Environment.SetEnvironmentVariable("VisualStudioVersion", @"15.0");

            var prj1 = new Microsoft.Build.Evaluation.Project(Project1Path);
            var prj2 = new Microsoft.Build.Evaluation.Project(Project2Path);
        }



        private static string FindSolutionDir()
        {
            var solutionPath = TestContext.CurrentContext.TestDirectory;

            while (solutionPath.Length > 3)
            {
                var xdir = System.IO.Path.Combine(solutionPath, "src");

                if (System.IO.Directory.Exists(xdir))
                {
                    if (System.IO.Directory.EnumerateFiles(xdir, "*.sln").Any()) return xdir;
                }

                solutionPath = System.IO.Path.GetDirectoryName(solutionPath);
            }

            throw new System.IO.DirectoryNotFoundException();
        }

        private static string Project1Path => System.IO.Path.Combine(FindSolutionDir(), "VisualSolutionGenerator.WPF\\VisualSolutionGenerator.WPF.csproj");

        private static string Project2Path => System.IO.Path.Combine(FindSolutionDir(), "VisualSolutionGenerator\\VisualSolutionGenerator.csproj");
    }
}