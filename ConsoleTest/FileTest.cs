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
            var files = Directory.GetFiles(@"E:\data\video\20190226");
            var questionLines = File.ReadAllLines(@"E:\data\video\20190226\questionid.txt");
            var questionIds = questionLines.Select(p => new { Number = p.Split('\t')[0], QuestionId = p.Split('\t')[1] });
            foreach (var fileName in files)
            {
                if (!fileName.EndsWith(".mp4"))
                {
                    continue;
                }
                var number = fileName.Split('.')[2];
                var questionId = questionIds.FirstOrDefault(p => p.Number.Equals(number));
                if (questionId == null)
                {
                    Console.WriteLine($"{fileName}找不到对应的题目id");
                    continue;
                }
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb1 = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb1.Append(retVal[i].ToString("x2"));
                }
                sb.AppendFormat($"{questionId.QuestionId}\t{sb1.ToString().ToLower()}\t1\r\n");
            }
            File.WriteAllText(@"E:\data\video\20190226\QuestionMicroLesson.txt", sb.ToString());
        }


    }
}
