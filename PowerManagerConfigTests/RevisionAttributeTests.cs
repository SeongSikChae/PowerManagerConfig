using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class RevisionAttributeTests
    {
        [TestMethod]
        public void RevisionAttributeTest()
        {
            RevisionAttribute? attr = typeof(RevisionAttribute).Assembly.GetCustomAttribute<RevisionAttribute>(); 
            Assert.IsNotNull(attr);
            Trace.WriteLine(attr.Revision);
        }
    }
}