using System.Text;
using System;
using System.IO;

namespace ValetParkingBLL.Helpers
{
    public class PasswordGenerator
    {
        private static readonly Random Random = new Random();

        public static string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()?";

            StringBuilder password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                password.Append(validChars[Random.Next(validChars.Length)]);
            }

            return password.ToString();
        }

        public  static string GetEmailTemplateText(string fileRelativePath)
        {
            string physicalPath = Directory.GetCurrentDirectory();
            string FilePath = physicalPath + fileRelativePath;
            FilePath = Path.GetFullPath(FilePath);
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            return MailText;
        }
    }
}
