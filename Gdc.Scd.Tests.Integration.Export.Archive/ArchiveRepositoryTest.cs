using NUnit.Framework;
using System;
using Gdc.Scd.Export.ArchiveJob.Impl;
using Gdc.Scd.Tests.Util;
using Gdc.Scd.Export.ArchiveJob.Dto;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    public class ArchiveRepositoryTest : ArchiveRepository
    {
        public ArchiveRepositoryTest() : base(null)
        {
            this.path = StreamUtil.Location();
        }

        [TestCase(@"\\some\path\to\", 1917, 11, 7, @"\\some\path\to\1917-11-07")]
        [TestCase(@"not/a/real/path/", 1945, 5, 9, @"not/a/real/path/1945-05-09")]
        [TestCase(@"C:\ProgramFiles\Windows", 1961, 4, 12, @"C:\ProgramFiles\Windows\1961-04-12")]
        public void Format_Folder_Name_Test(string path, int year, int month, int day, string expected)
        {
            this.timestamp = new DateTime(year, month, day);
            this.path = path;
            Assert.AreEqual(expected, GetPath());
        }

        [TestCase]
        public void SaveCountryArchive_Test()
        {
            var excel = StreamUtil.ReadBin("TestData", "hdd.xlsx");
            var cnt = new CountryDto { Id = -1, Name = "Fake country", ISO = "no name" };
            var archive = new ArchiveDto { ArchiveName = "Hdd retention here" };

            Save(cnt, archive, excel);
        }
    }
}
