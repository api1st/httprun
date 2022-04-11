using System;
using System.Runtime.InteropServices;

namespace HttpExecutor.Tests.Integration
{
    public class PostFileBaseFixture : IDisposable
    {
        public PostFileBaseFixture()
        {
            var scriptDirectory = System.IO.Directory.GetParent("./Scripts/1-GETs.http");
            if (scriptDirectory != null)
            {
                Console.WriteLine("Parent directory of Scripts is: " + scriptDirectory.FullName);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (scriptDirectory.Parent != null)
                    {
                        var dir = $"{scriptDirectory.Parent.FullName}/Scripts/";
                        Console.WriteLine($"Trying to set '{dir}' as current directory");
                        if (!System.IO.Directory.Exists(dir))
                        {
                            Console.WriteLine("That directory does not exist.");
                        }
                        System.IO.Directory.SetCurrentDirectory(scriptDirectory.Parent.FullName + "/Scripts/");
                    }
                }
                else
                {
                    System.IO.Directory.SetCurrentDirectory(scriptDirectory.FullName);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}