using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DbClrZabbixSP
{

    [DataContract]
    public class ZS_Data
    {
        [DataMember]
        public string host { get; set; }

        [DataMember]
        public string key { get; set; }

        [DataMember]
        public string value { get; set; }
        public ZS_Data(string Zbxhost, string Zbxkey, string Zbxval)
        {
            host = Zbxhost;
            key = Zbxkey;
            value = Zbxval;
        }
    }

    [DataContract]
    public class ZS_Response
    {
        [DataMember]
        public string response { get; set; }

        [DataMember]
        public string info { get; set; }

    }

    [DataContract]
    public class ZS_Request
    {
        [DataMember]
        public string request { get; set; }

        [DataMember]
        public ZS_Data[] data { get; set; }

        public ZS_Request(string ZbxHost, string ZbxKey, string ZbxVal)
        {
            request = "sender data";
            data = new ZS_Data[] { new ZS_Data(ZbxHost, ZbxKey, ZbxVal) };
        }

        public ZS_Response Send(string ZbxServer, int ZbxPort = 10051, int ZbxTimeOut = 500)
        {
            var jr = MyJsonConverter.Serialize<ZS_Request>(new ZS_Request(data[0].host, data[0].key, data[0].value));

            using (TcpClient lTCPc = new TcpClient(ZbxServer, ZbxPort))
            using (NetworkStream lStream = lTCPc.GetStream())
            {
                byte[] Header = Encoding.ASCII.GetBytes("ZBXD\x01");
                byte[] DataLen = BitConverter.GetBytes((long)jr.Length);
                byte[] Content = Encoding.ASCII.GetBytes(jr);
                byte[] Message = new byte[Header.Length + DataLen.Length + Content.Length];
                Buffer.BlockCopy(Header, 0, Message, 0, Header.Length);
                Buffer.BlockCopy(DataLen, 0, Message, Header.Length, DataLen.Length);
                Buffer.BlockCopy(Content, 0, Message, Header.Length + DataLen.Length, Content.Length);

                lStream.Write(Message, 0, Message.Length);
                lStream.Flush();
                int counter = 0;
                while (!lStream.DataAvailable)
                {
                    if (counter < ZbxTimeOut / 50)
                    {
                        counter++;
                        Task.Delay(50);
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }

                byte[] resbytes = new Byte[1024];
                lStream.Read(resbytes, 0, resbytes.Length);
                string s = Encoding.UTF8.GetString(resbytes);

                int pFrom = s.IndexOf("{");
                int pTo = s.LastIndexOf("}") + 1;

                string jsonRes = s.Substring(pFrom, pTo - pFrom);
                return MyJsonConverter.Deserialize<ZS_Response>(jsonRes);

            }
        }
    }

    public static class MyJsonConverter
    {
        public static T Deserialize<T>(string body)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(body);
                writer.Flush();
                stream.Position = 0;
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
            }
        }

        public static string Serialize<T>(T item)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(ms, item);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
