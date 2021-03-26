using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

    class CryptoClass
    {
        public static System.Security.SecureString key = new System.Security.SecureString();
        public static void DESSetKey()
        {
            key.AppendChar('d');
            key.AppendChar('e');
            key.AppendChar('s');
            key.AppendChar('h');
            key.AppendChar('s');
            key.AppendChar('0');
            key.AppendChar('0');
            key.AppendChar('8');
        }

        public static string fromSecurityString(System.Security.SecureString ss)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(System.Runtime.InteropServices.Marshal.SecureStringToBSTR(CryptoClass.key));
        }

        //加密
        public static string RSAEncryption(string express)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "rsa_hs";//密匙容器的名称，保持加密解密一致才能解密成功
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                byte[] plaindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
                byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
                return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
            }
        }
        //加密对象
        public static string RSAEncryption(object express)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "rsa_hs";//密匙容器的名称，保持加密解密一致才能解密成功
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                MemoryStream stream = new MemoryStream();
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, express);
                stream.Position = 0;
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                //byte[] plaindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
                //byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
                byte[] encryptdata = rsa.Encrypt(data, false);
                return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
            }
        }
        //解密
        public static string RSADecrypt(string ciphertext)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "rsa_hs";
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                byte[] encryptdata = Convert.FromBase64String(ciphertext);
                byte[] decryptdata = rsa.Decrypt(encryptdata, false);
                return Encoding.Default.GetString(decryptdata);
            }
        }
        //解密对象
        public static object RSADecryptObj(string ciphertext)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "rsa_hs";
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                byte[] encryptdata = Convert.FromBase64String(ciphertext);
                byte[] decryptdata = rsa.Decrypt(encryptdata, false);
                //return Encoding.Default.GetString(decryptdata);
                MemoryStream stream = new MemoryStream();
                IFormatter formatter = new BinaryFormatter();
                stream.Write(decryptdata, 0, decryptdata.Length);
                stream.Position = 0;
                object obj = formatter.Deserialize(stream);
                return obj;
            }
        }
        // DES字符串加密
        public static string DESEncrypt(string _strQ, string strKey)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(_strQ);
            MemoryStream ms = new MemoryStream();
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(ms, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strKey)), CryptoStreamMode.Write);
            encStream.Write(buffer, 0, buffer.Length);
            encStream.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray()).Replace("+", "%");
        }

        // DES字符串解密
        public static string DESDecrypt(string _strQ, string strKey)
        {
            _strQ = _strQ.Replace("%", "+");
            byte[] buffer = Convert.FromBase64String(_strQ);
            MemoryStream ms = new MemoryStream();
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strKey)), CryptoStreamMode.Write);
            encStream.Write(buffer, 0, buffer.Length);
            encStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }


        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public static string MD5(string express)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] palindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
            byte[] encryptdata = md5.ComputeHash(palindata);//将字符串加密后也转换为字符数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为加密字符串
        }

    }
