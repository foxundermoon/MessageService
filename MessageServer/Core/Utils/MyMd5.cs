using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;

namespace MessageService.Core.Utils
{
    public delegate void AsyncCheckHeadler(AsyncCheckEventArgs e);

    public class MyMd5
    {
        //支持所有哈希算法
        private static HashAlgorithm hashAlgorithm;

        //文件缓冲区
        private static byte[] buffer;

        //文件读取流
        private static Stream inputStream;

        public event AsyncCheckHeadler AsyncCheckProgress;

        /// <summary>
        /// 返回指定文件的MD5值
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Check(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] buffer = md5Provider.ComputeHash(fs);
            string resule = BitConverter.ToString(buffer);
            resule = resule.Replace("-", "");
            return resule;
        }

        public void AsyncCheck(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));

            int bufferSize = 1048576;//缓冲区大小，1MB

            buffer = new byte[bufferSize];

            //打开文件流
            inputStream = File.Open(path, FileMode.Open);
            hashAlgorithm = new MD5CryptoServiceProvider();

            //异步读取数据到缓冲区
            inputStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(AsyncComputeHashCallback), null);
        }

        private void AsyncComputeHashCallback(IAsyncResult result)
        {
            int bytesRead = inputStream.EndRead(result);

            //检查是否到达流末尾
            if (inputStream.Position < inputStream.Length)
            {
                //输出进度
                string pro = string.Format("{0:P0}", (double)inputStream.Position / inputStream.Length);

                if (null != AsyncCheckProgress)
                    AsyncCheckProgress(new AsyncCheckEventArgs(AsyncCheckState.Checking, pro));

                var output = new byte[buffer.Length];
                //分块计算哈希值
                hashAlgorithm.TransformBlock(buffer, 0, buffer.Length, output, 0);

                //异步读取下一分块
                inputStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(AsyncComputeHashCallback), null);
                return;
            }
            else
            {
                //计算最后分块哈希值
                hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
            }

            string md5 = BitConverter.ToString(hashAlgorithm.Hash).Replace("-", "");

            AsyncCheckProgress(new AsyncCheckEventArgs(AsyncCheckState.Checking, md5));

            //关闭流
            inputStream.Close();
        }
    }

    public enum AsyncCheckState
    {
        Completed,
        Checking
    }

    public class AsyncCheckEventArgs : EventArgs
    {
        public string Value { get; private set; }

        public AsyncCheckState State { get; private set; }

        public AsyncCheckEventArgs(AsyncCheckState state, string value)
        {
            this.Value = value; 
            this.State = state;
        }
    }
}