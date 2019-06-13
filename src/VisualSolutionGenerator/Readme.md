

https://github.com/Microsoft/msbuild/tree/master/src/MSBuild.UnitTests

 https://slntools.codeplex.com/

 http://stackoverflow.com/questions/707107/parsing-visual-studio-solution-files



 // http://stackoverflow.com/questions/6511380/how-do-i-build-a-solution-programatically-in-c

    // http://stackoverflow.com/questions/33329144/making-a-solution-file-without-using-visual-studio

    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/7a21108c-17a5-4304-a3ce-33ad201f0d11/how-do-i-programmatically-list-all-of-the-projects-in-a-solution?forum=csharpgeneral
    // using SOLUTION = Microsoft.Build.Construction.SolutionFile;

    // using KK = Microsoft.Build.Construction.ProjectInSolution;


    // {9A24C608-BCA7-4C68-836C-BAF990AFED85}  .Debug|Any CPU    .ActiveCfg  = Debug|Any CPU
    // {9A24C608-BCA7-4C68-836C-BAF990AFED85}  .Debug|Any CPU    .Build.0    = Debug|Any CPU
    // {9A24C608-BCA7-4C68-836C-BAF990AFED85}  .Release|Any CPU  .ActiveCfg  = Release|Any CPU
    // {9A24C608-BCA7-4C68-836C-BAF990AFED85}  .Release|Any CPU  .Build.0    = Release|Any CPU


	TODO:

	Create class ProjectPlatformInfo

	ProjectWin32Info : ProjectPlatformInfo
	ProjectAndroidInfo : ProjectPlatformInfo
	ProjectUniversalInfo : ProjectPlatformInfo