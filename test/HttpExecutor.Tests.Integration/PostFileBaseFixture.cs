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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (scriptDirectory.Parent != null)
                    {
                        System.IO.Directory.SetCurrentDirectory(scriptDirectory.Parent.FullName);
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