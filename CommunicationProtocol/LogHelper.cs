using System;
using System.IO;

namespace CommunicationProtocol
{
    public class LogHelper
    {
        public static void WriteToFile(string pErrorMessage, object pClass, string pFileName = "", bool pEraseFile = false)
        {
            WriteToFile(pErrorMessage, pClass.GetType().Name, pFileName, pEraseFile);
        }

        public static void WriteToFile(string pErrorMessage, string pClassName, string pFileName = "", bool pEraseFile = false)
        {
            DateTime nowDate = DateTime.Now;
            string shortDate = string.Format("{0:yyyy-MM-dd}", nowDate);
            string filename = string.Empty;
            if (string.IsNullOrEmpty(pFileName))
            {
                filename = string.Format("{0}.log", shortDate);
            }
            else
            {
                filename = string.Format("{0}.log", pFileName);
            }

            filename = filename.Replace("/", "-");
            string rootPath = Path.GetFullPath("./Log/");
            string fullFilename = string.Format(@"{0}{1}", rootPath, filename);

            #region Create the folder
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            #endregion

            #region Create/Overwrite the file
            if (!File.Exists(fullFilename) || pEraseFile)
            {
                FileStream f = File.Create(fullFilename);
                f.Close();
            }
            #endregion

            #region Write in the file
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(fullFilename, true);
                writer.WriteLine(string.Format("[{0} - {1}] : {2}", DateTime.Now, pClassName, pErrorMessage));
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
            #endregion
        }
    }
}
