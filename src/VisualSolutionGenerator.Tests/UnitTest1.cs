using NUnit.Framework;
using System.Linq;

namespace Tests
{
    [Category("Visual Solution Generator Load Tests")]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            if (!Microsoft.Build.Locator.MSBuildLocator.IsRegistered)
            {
                Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
                Assert.That(Microsoft.Build.Locator.MSBuildLocator.IsRegistered);
            }            
        }        

        [Test]
        public void Test1()
        {
            var solutionPath = TestContext.CurrentContext.TestDirectory;

            while(solutionPath.Length > 3)
            {
                if (System.IO.Directory.EnumerateFiles(solutionPath, "*.sln").Any()) break;
                solutionPath = System.IO.Path.GetDirectoryName(solutionPath);
            }

            var projects = new VisualSolutionGenerator.FileBaseInfo.Collection(new System.IO.DirectoryInfo(solutionPath));

            projects.ProbeFolder(solutionPath, null, null, true);

            foreach(var f in projects.FailedFiles)
            {
                TestContext.WriteLine($"{f.FileName} {f.ErrorMessage}");
            }

            Assert.That(projects.FailedFiles, Has.Count.Empty);
        }
    }
}