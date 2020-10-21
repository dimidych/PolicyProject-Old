using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JsonWorker
{
    public static class JsonWorker<T> where T : class
    {
        public static string Serialize(T instance)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof (T));

            using (var stream = new MemoryStream())
            {
                jsonSerializer.WriteObject(stream, instance);
                var result = Encoding.UTF8.GetString(stream.ToArray(), 0, (int) stream.Length);
                return result;
            }
        }

        public static T Deserialize(string jsonInstance)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof (T));
            var jsonAsByteArray = GetBytes(jsonInstance);

            using (var stream = new MemoryStream(jsonAsByteArray))
            {
                var result = jsonSerializer.ReadObject(stream) as T;
                return result;
            }
        }

        public static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length/sizeof (char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}