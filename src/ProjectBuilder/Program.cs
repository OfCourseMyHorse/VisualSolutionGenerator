// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;

namespace BuilderApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Header();
            var projectFilePath = Args(args.Skip(1).ToArray());

            MSBuildLocator.RegisterDefaults();

            var result = new Builder().Build(projectFilePath);
            Console.WriteLine();            

            Console.ForegroundColor = result ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Build result: {result}");
            Console.ResetColor();

            Console.ReadKey();
        }        

        private static void Header()
        {
            Console.WriteLine($"Sample MSBuild Builder App.");
            Console.WriteLine();
        }

        private static string Args(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0])) Usage();
            var projectFilePath = args[0];
            return projectFilePath;
        }

        private static void Usage()
        {
            Console.WriteLine("BuilderApp.exe <path>");
            Console.WriteLine("    path = path to .*proj file to build");
            Environment.Exit(-1);
        }
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
    public class Builder
    {
        public bool Build(string projectFile)
        {
            var assembly = typeof(Project).Assembly;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            Console.WriteLine();
            Console.WriteLine($"BuildApp running using MSBuild version {fvi.FileVersion}");
            Console.WriteLine(Path.GetDirectoryName(assembly.Location));
            Console.WriteLine();

            var pre = ProjectRootElement.Open(projectFile);
            var project = new Project(pre);
            return project.Build(new Logger());
        }

        public Project Loader(string projectFile)
        {
            var assembly = typeof(Project).Assembly;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            Console.WriteLine();
            Console.WriteLine($"BuildApp running using MSBuild version {fvi.FileVersion}");
            Console.WriteLine(Path.GetDirectoryName(assembly.Location));
            Console.WriteLine();

            var pre = ProjectRootElement.Open(projectFile);
            return new Project(pre);            
        }

        private class Logger : ILogger
        {
            public void Initialize(IEventSource eventSource)
            {
                eventSource.AnyEventRaised += (_, args) => { Console.WriteLine(args.Message); };
            }

            public void Shutdown()
            {
            }

            public LoggerVerbosity Verbosity { get; set; }
            public string Parameters { get; set; }
        }
    }
}
