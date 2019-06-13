using System;
using System.Linq;
using NUnit.Framework;


namespace Tests
{
    [Category("MSBuild evaluation with locator")]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
            Assert.IsTrue(Microsoft.Build.Locator.MSBuildLocator.IsRegistered);

            var prj1 = Builder.LoadProject(Project1Path);
            var prj2 = Builder.LoadProject(Project2Path);
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

        private static string Project1Path = System.IO.Path.Combine(FindSolutionDir(), "VisualSolutionGenerator.WPF\\VisualSolutionGenerator.WPF.csproj");

        private static string Project2Path = System.IO.Path.Combine(FindSolutionDir(), "VisualSolutionGenerator\\VisualSolutionGenerator.csproj");
    }

    /// <summary>
    /// Class for performing the project build
    /// </summary>
    /// <remarks>
    /// The Microsoft.Build namespaces must be referenced from a method that is called
    /// after RegisterInstance so that it has a chance to change their load behavior.
    /// Here, we put Microsoft.Build calls into a separate class
    /// that is only referenced after calling RegisterInstance.
    /// </remarks>
    static class Builder
    {
        public static bool LoadProject(string path)
        {
            var prj1 = new Microsoft.Build.Evaluation.Project(path);
            return prj1 != null;
        }

    }


}