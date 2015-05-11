using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MessageService.Core.Utils
{
    public class HashHelper
    {
        public static string MD5Stream(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(fs);
            fs.Close();

            byte[] b = md5.Hash;
            md5.Clear();

            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append(b[i].ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 对文件进行MD5加密
        /// </summary>
        /// <param name="filePath"></param>
        public static string MD5File(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bufferSize = 1048576; // 缓冲区大小，1MB
            byte[] buff = new byte[bufferSize];

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.Initialize();

            long offset = 0;
            while (offset < fs.Length)
            {
                long readSize = bufferSize;
                if (offset + readSize > fs.Length)
                {
                    readSize = fs.Length - offset;
                }

                fs.Read(buff, 0, Convert.ToInt32(readSize)); // 读取一段数据到缓冲区

                if (offset + readSize < fs.Length) // 不是最后一块
                {
                    md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
                }
                else // 最后一块
                {
                    md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));
                }

                offset += bufferSize;
            }

            fs.Close();
            byte[] result = md5.Hash;
            md5.Clear();

            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}