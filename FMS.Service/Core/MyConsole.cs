using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using FMS.Service.DAO;
using System.Text;
using System.Security.Cryptography;

namespace FMS.Service
{
    public class MyConsole
    {

        /// <summary>
        /// 获取Web.Config中AppSetting的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppString(string key)
        {
            if (ConfigurationManager.AppSettings[key] == null)
            {
                throw new Exception("config缺少" + key);
            }
            return ConfigurationManager.AppSettings[key].ToString();
        }

        public static string ReadFile(string path)
        {
            var file = HostingEnvironment.MapPath(path);
            if (!File.Exists(file)) throw new HttpException(404, "文件读取失败");
            var result = "";
            using (var sr = new StreamReader(file))
            {
                try
                {
                    result = sr.ReadToEnd();
                }
                catch (Exception ex)
                {
                    MyConsole.Log(ex);
                    throw ex;
                }

            }
            return result;
        }

        public static void Log(string msg, string type = "")
        {
            try
            {
                string path = GetAppString("log");

                if (!Directory.Exists(path + "console/")) Directory.CreateDirectory(path + "console/");
                string file = DateTime.Now.ToString("yyyyMMdd") + ".log";
                FileStream fs = new FileStream(path + "console/" + file, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                string desc = DateTime.Now.ToString("HH:mm:ss") + "\t" + type;
                sw.WriteLine(desc);
                sw.WriteLine(msg);
                sw.Close();
                fs.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void Log(Exception e, string type = "")
        {
            Log(e.Message + "\n" + e.StackTrace, type);
        }

        #region 腾讯云相关

        private static CipherData _cipher;
        private static string configfile = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "init.data");

        public static void ReadConfig()
        {
            if (!File.Exists(configfile))
            {
                _cipher = new CipherData();
                SaveConfig();
                return;
            }
            using (var fs = new FileStream(configfile, FileMode.OpenOrCreate))
            {
                using (var sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    var content = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(content))
                    {
                        try
                        {
                            _cipher = Newtonsoft.Json.JsonConvert.DeserializeObject<CipherData>(content);
                        }
                        catch (Exception)
                        {
                            _cipher = new CipherData();
                        }
                    }
                    else _cipher = new CipherData();
                    sr.Close();
                }
            }
        }

        public static void SaveConfig(CipherData data = null)
        {
            using (var fs = new FileStream(configfile, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    if (data != null) _cipher = data;
                    string content = Newtonsoft.Json.JsonConvert.SerializeObject(_cipher);
                    sw.Write(content);
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
                fs.Close();
                fs.Dispose();
            }
        }

        public static CipherData Cipher
        {
            set
            {
                _cipher = value;
            }
            get
            {
                if (_cipher == null)
                {
                    ReadConfig();
                }
                return _cipher;
            }
        }


        //public static string SignatureHash(string type, string src, string key, bool t)
        //{
        //    HMACSHA1 hmacsha1 = new HMACSHA1();
        //    hmacsha1.Key = Encoding.UTF8.GetBytes(key);
        //    byte[] dataBuffer = Encoding.UTF8.GetBytes(src);
        //    byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
        //    byte[] buff = hashBytes.Concat(dataBuffer).ToArray();
        //    return Convert.ToBase64String(buff);
        //    //var hash = "";
        //    //var bypes = Encoding.UTF8.GetBytes(src);
        //    //return Convert.ToBase64String(bypes);
        //}
        //public static string SignatureHash(string type, string src, string key, bool t)
        //{
        //    HMACSHA1 hmacsha1 = new HMACSHA1();
        //    hmacsha1.Key = Encoding.UTF8.GetBytes(key);
        //    byte[] dataBuffer = Encoding.UTF8.GetBytes(src);
        //    byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
        //    byte[] buff = hashBytes.Concat(dataBuffer).ToArray();
        //    return Convert.ToBase64String(buff);
        //    //var hash = "";
        //    //var bypes = Encoding.UTF8.GetBytes(src);
        //    //return Convert.ToBase64String(bypes);
        //}
        #endregion

        #region

        static char[] reserveChar = new char[] { '/', '?', '*', ':', '|', '\\', '<', '>', '\"' };

        /// <summary>
        /// 远程路径Encode处理,会保证开头是/，结尾也是/
        /// </summary>
        /// <param name="remotePath"></param>
        /// <returns></returns>
        public static string EncodeRemotePath(string remotePath)
        {
            if (remotePath == "/")
            {
                return remotePath;
            }
            var endWith = remotePath.EndsWith("/");
            String[] part = remotePath.Split('/');
            remotePath = "";
            foreach (var s in part)
            {
                if (s != "")
                {
                    if (remotePath != "")
                    {
                        remotePath += "/";
                    }
                    remotePath += HttpUtility.UrlEncode(s).Replace("+", "%20");
                }
            }

            remotePath = (remotePath.StartsWith("/") ? "" : "/") + remotePath + (endWith ? "/" : "");
            return remotePath;
        }

        /// <summary>
        /// 标准化远程目录路径,会保证开头是/，结尾也是/ ,如果命名不规范，存在保留字符，会返回空字符
        /// </summary>
        /// <param name="remotePath">要标准化的远程路径</param>
        /// <returns></returns>
        public static string StandardizationRemotePath(string remotePath)
        {
            if (String.IsNullOrEmpty(remotePath))
            {
                return "";
            }

            if (!remotePath.StartsWith("/"))
            {
                remotePath = "/" + remotePath;
            }

            if (!remotePath.EndsWith("/"))
            {
                remotePath = remotePath + "/";
            }

            int index1 = 1;
            int index2 = 0;
            while (index1 < remotePath.Length)
            {
                index2 = remotePath.IndexOf('/', index1);
                if (index2 == index1)
                {
                    return "";
                }

                var folderName = remotePath.Substring(index1, index2 - index1);
                if (folderName.IndexOfAny(reserveChar) != -1)
                {
                    return "";
                }

                index1 = index2 + 1;

            }
            return remotePath;
        }
        #endregion
    }
}