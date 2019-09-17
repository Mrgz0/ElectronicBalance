using Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ElectronicBalance
{
    public class SerialPortService
    {
        private static SerialPortService _Instance = null;
        public static SerialPortService Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new SerialPortService();
                }
                return _Instance;
            }
        }

        private SerialPort serialPort = null;

        public void Connect()
        {
            serialPort = new SerialPort();

            serialPort.PortName = CommonConf.Instance.Com; //通信端口
            serialPort.BaudRate = 9600; //串行波特率
            serialPort.DataBits = 8; //每个字节的标准数据位长度
            serialPort.StopBits = StopBits.One; //设置每个字节的标准停止位数
            serialPort.Parity = Parity.None; //设置奇偶校验检查协议
            serialPort.ReadTimeout = 3000; //单位毫秒
            serialPort.WriteTimeout = 3000; //单位毫秒
            serialPort.ReceivedBytesThreshold = 1; //串口对象在收到这样长度的数据之后会触发事件处理函数,一般都设为1
            serialPort.DataReceived += new SerialDataReceivedEventHandler(CommDataReceived); //设置数据接收事件（监听）

            serialPort.Open();
        }

        public delegate void DataReceiedHandler(string data);
        public event DataReceiedHandler DataReceived;

        public void Disconnect()
        {
            serialPort.Close();
        }

        public void CommDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            int len = serialPort.BytesToRead;
            Byte[] readBuffer = new Byte[len];
            serialPort.Read(readBuffer, 0, len);
            string msg = Encoding.UTF8.GetString(readBuffer, 0, len);

            string value = GetWeight(msg);
            DataReceived.Invoke(value);
        }

        private static string GetWeight(string str)
        {
            string result = "0", str1 = "0";
            str = str.Trim();
            if (str != null && str != string.Empty)
            {
                // 正则表达式替换非数字字符（不包含小数点.） 
                str = Regex.Replace(str, @"[^\d\\.\d]", "&");
                //将串口输入的数据转化为:&&AAA.BBB&&&AAA.BBB&&&AAA.BBB&&&AAA.BBB&&&AAA.BBB&&&
                var strList = str.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                int lenght = strList.Length;
                if (lenght > 0)
                {
                    if (lenght > 3)
                    {//  如果有多个电子称输入值，则取倒数第二个数值，作为最新值(不取最后一个值，是为了避免出现&&AAA.BBB&&AA情况下取值不正确)
                        str = strList[lenght - 2];
                        str1 = strList[lenght - 3];
                        if (str1.Length > str.Length)
                            str = str1;
                    }
                    else if (lenght == 2)
                    {
                        str = strList[0];
                        str1 = strList[1];
                        if (str1.Length > str.Length)
                            str = str1;
                    }
                    else
                        str = strList[0];
                }
                else
                    str = "0";//如果无输入，则默认为0

                // 如果是数字，则转换为decimal类型 
                if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                {
                    result = str;
                }
            }
            return result;
        }
    }

    public class WebSocketServerService
    {
        private static WebSocketServerService _Instance = null;
        public static WebSocketServerService Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new WebSocketServerService();
                }
                return _Instance;
            }
        }

        private WebSocketServer server = null;
        private List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();

        public void Start()
        {
            FleckLog.Level = LogLevel.Error;
            server = new WebSocketServer("ws://127.0.0.1:" + CommonConf.Instance.Port.ToString());

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                };
            });
        }

        public void DataReceied(string data)
        {
            foreach (var socket in allSockets.ToList())
            {
                socket.Send(data);
            }
        }

        public void Close()
        {
            server.Dispose();

        }

    }

    public class CommonConf
    {
        public static CommonConf _Instance = null;
        public static CommonConf Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new CommonConf();
                }
                return _Instance;
            }
        }
        public int Port { get; set; }
        public string Com { get; set; }

        private const string ConfPath = ".\\conf";
        public void SaveConf()
        {
            try
            {
                string content = string.Format("{0},{1}", Port, Com);
                using (var sw = new StreamWriter(new FileStream(ConfPath, FileMode.OpenOrCreate, FileAccess.Write), Encoding.UTF8))
                {
                    sw.Write(content);
                }

            }
            catch
            {
            }
        }
        public void LoadConf()
        {
            try
            {
                string content;
                using (var sr = new StreamReader(new FileStream(ConfPath, FileMode.Open, FileAccess.Read), Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
                var arr = content.Split(new char[] { ',' });
                Port = Int32.Parse(arr[0]);
                Com = arr[1];
            }
            catch
            {

            }
            if (Port == 0)
            {
                Port = 2019;
            }

            if (string.IsNullOrEmpty(Com))
            {
                var comPorts = SerialPort.GetPortNames();
                if (comPorts.Length > 0)
                {
                    Com = comPorts[0];
                }
            }

        }
    }

    public class Manager
    {
        public static Manager _Instance = null;
        public static Manager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Manager();
                }
                return _Instance;
            }
        }
        public event SerialPortService.DataReceiedHandler DataReceived;
        public void Start()
        {
            WebSocketServerService.Instance.Start();
            SerialPortService.Instance.DataReceived += WebSocketServerService.Instance.DataReceied;
            SerialPortService.Instance.DataReceived += DataReceived;
            SerialPortService.Instance.Connect();
        }
        public void Init()
        {
            CommonConf.Instance.LoadConf();
        }
        public void Stop()
        {
            SerialPortService.Instance.Disconnect();
            WebSocketServerService.Instance.Close();
        }
    }
}
