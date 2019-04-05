using Gdc.Scd.Export.Archive.Impl;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    public class ArchiveRepositoryTest : ArchiveRepository
    {
        public ArchiveRepositoryTest() : base(null) { }

        [TestCase(@"\\some\path\to\", 1917, 11, 7, @"\\some\path\to\1917-11-07")]
        [TestCase(@"not/a/real/path/", 1945, 5, 9, @"not/a/real/path/1945-05-09")]
        [TestCase(@"C:\ProgramFiles\Windows", 1961, 4, 12, @"C:\ProgramFiles\Windows\1961-04-12")]
        public void Format_Folder_Name_Test(string path, int year, int month, int day, string expected)
        {
            this.timestamp = new DateTime(year, month, day);
            this.path = path;
            Assert.AreEqual(expected, GetPath());
        }
    }
}
