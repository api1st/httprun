using System;
using System.IO;

namespace HttpExecutor.Tests.Integration
{
    public class PostFileBaseFixture : IDisposable
    {
        public PostFileBaseFixture()
        {
            var scriptDirectory = System.IO.Directory.GetParent("./Scripts/1-GETs.http");
            System.IO.Directory.SetCurrentDirectory(scriptDirectory.FullName);
        }

        public void Dispose()
        {

        }
    }
}