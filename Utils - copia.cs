using System.Text;

//namespace AgenciaPruebas
//{
//    internal class Utils
//    {
//    }
//}

namespace AgenciaPruebas
{
    public static class Utils
    {
        public static string ficheroLog;

        public static void LogToFile(string Message)
        {
            try
            {
                if (String.IsNullOrEmpty(ficheroLog))
                {
                    String fichero = System.IO.Path.GetTempPath();
                    if (!fichero.EndsWith("\\"))
                        fichero += "\\";
                    fichero += "Test_SII_";
                    fichero += System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                    fichero += ".LOG";
                    ficheroLog = fichero;
                }
                StreamWriter sw = new StreamWriter(ficheroLog, true);
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString());
                sb.Append(" - ");
                sb.Append(Message);
                sw.WriteLine(sb.ToString());
                sw.Close();
                sw = null;
            }
            catch (Exception)
            {
                // throw ex;
            }
        }
    }
}

