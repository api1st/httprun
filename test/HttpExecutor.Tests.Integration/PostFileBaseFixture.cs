using System;

namespace HttpExecutor.Tests.Integration
{
    public class PostFileBaseFixture : IDisposable
    {
        public PostFileBaseFixture()
        {
            var scriptDirectory = System.IO.Directory.GetParent("./Scripts/1-GETs.http");
            if (scriptDirectory != null)
            {
                System.IO.Directory.SetCurrentDirectory(scriptDirectory.FullName);
            }
        }

        public void Dispose()
        {

        }
    }
}