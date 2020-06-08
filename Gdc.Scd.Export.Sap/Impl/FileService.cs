using Gdc.Scd.Export.Sap.Enitities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gdc.Scd.Export.Sap.Interfaces;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class FileService : IFileService
    {
        public string CreateFileOnServer(List<ReleasedData> sapUploadDatas, int fileNumber)
        {
            var fileName = Config.SapFileName + "." + fileNumber.ToString("0000");
            var writePath = Config.ExportDirectory.EndsWith("\\") ? Config.ExportDirectory : Config.ExportDirectory + "\\";

            using (StreamWriter sw = new StreamWriter(writePath + fileName, false, System.Text.Encoding.Default))
            {
                var text = CreateTextFromConfig(Config.FileHeader);
                sw.WriteLine(text);
            }

            using (StreamWriter sw = new StreamWriter(writePath + fileName, true, System.Text.Encoding.Default))
            {
                foreach (var suData in sapUploadDatas)
                {
                    var text = CreateTextFromConfig(Config.FileLine1);
                    text = CreateTextFromDto(text, suData);
                    sw.WriteLine(text);

                    var text2 = CreateTextFromConfig(Config.FileLine2);
                    text2 = CreateTextFromDto(text2, suData);
                    sw.WriteLine(text2);
                }
            }

            return fileName;
        }

        private string CreateTextFromConfig(string template)
        {
            string pattern = @"(?<=\[)[^]]*(?=\])";
            // Create a Regex  
            Regex rg = new Regex(pattern);

            // Get all matches  
            MatchCollection matchedFields = rg.Matches(template);

            // Print all matched authors  
            for (int count = 0; count < matchedFields.Count; count++)
            {
                template = template.Replace("[" + matchedFields[count].Value + "]",
                    (new Config()).GetAttribute<string>(matchedFields[count].Value));
            };

            return template;
        }

        private string CreateTextFromDto(string template, ReleasedData dto)
        {
            string pattern = @"(?<=\{)[^}]*(?=\})";
            // Create a Regex  
            Regex rg = new Regex(pattern);

            // Get all matches  
            MatchCollection matchedFields = rg.Matches(template);

            // Print all matched authors  
            for (int count = 0; count < matchedFields.Count; count++)
                template = template.Replace("{" + matchedFields[count].Value + "}",
                    dto.GetAttribute<string>(matchedFields[count].Value));

            return template;
        }

        public bool SendFileToSap(string filename)
        {
            var result = true;
            try
            {
                var errCode = NetCopyFile(filename, Config.ExportDirectory, filename, Config.ExportHost, Config.Admission);
                if (errCode != 0)
                {
                    result = false;
                    var msg = string.Format("net copy file: ERROR : '{0}'", errCode);
                    //Log.Error(msg);
                }
            }
            catch (Exception ex)
            {
                result = false;
                var msg = string.Format(
                    "Error occured when try to send file '{0}' from dir='{1}' to host='{2}' with params='{3}' ",
                    filename, Config.ExportDirectory, Config.ExportHost, Config.Admission);
                //Log.Error(ex, msg);

            }

            return result;
        }

        private int NetCopyFile(string filenameFrom, string pathFrom, string filenameTo, string pathTo, string additionalParams)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = "ncopy.exe",
                    UseShellExecute = true,
                    WorkingDirectory = pathFrom,
                    Arguments = filenameFrom + " " + pathTo + "!" + filenameTo + " " + additionalParams
                }
            };
            //Log.Info("net copy file: sending " + filenameFrom + "...");
            //Log.Info("net copy file: startcode=" + proc.Start() + "...");
            //Log.Info("net copy file: waiting...");
            proc.WaitForExit(30000);

            if (proc.ExitCode != 0)
            {
                //Log.Info("net copy file: ERROR " + proc.ExitCode);
            }
            else
            {
                //Log.Info("net copy file: ok");
            }

            return proc.ExitCode;
        }
    }
}
