using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using System.IO;
    using System.Linq;

    public class FileTest
    {
        public static void FileMd5Test()
        {
            var sb = new StringBuilder();
            var files = Directory.GetFiles(@"E:\data\video\20190321");
            var questionLines = File.ReadAllLines(@"E:\data\video\20190321\questionid.txt");
            var questionIds = questionLines.Select(p => new { Number = p.Split('\t')[0], QuestionId = p.Split('\t')[1] });
            foreach (var fileName in files)
            {
                if (!fileName.EndsWith(".mp4"))
                {
                    continue;
                }
                var number = Path.GetFileNameWithoutExtension(fileName);
                var questionId = questionIds.FirstOrDefault(p => p.Number.Equals(number));
                if (questionId == null)
                {
                    Console.WriteLine($"{fileName}找不到对应的题目id");
                    continue;
                }
                var file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                var sb1 = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb1.Append(retVal[i].ToString("x2"));
                }
                sb.AppendFormat($"{questionId.QuestionId}\t{sb1.ToString().ToLower()}\t1\r\n");
            }
            File.WriteAllText(@"E:\data\video\20190321\QuestionMicroLesson.txt", sb.ToString());
        }
    }
}
