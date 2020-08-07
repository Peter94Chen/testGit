using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace testGit
{
    class TCPServer
    {
        public static string data, SaveTo;
        public static int Port;
        TcpListener obj_server;
        Form1 _F1;


        public TCPServer(Form1 F1)
        {
            _F1 = F1;
            obj_server = new TcpListener(IPAddress.Parse(F1.txt_target_IP.Text), Port);
        }

        public void Startserver()
        {
            obj_server.Start();
            while (true)
            {
                TcpClient tc = obj_server.AcceptTcpClient();
                SocketHandler obj_hadler = new SocketHandler(tc, _F1);
                System.Threading.Thread obj_thread = new
                System.Threading.Thread(obj_hadler.ProcessSocketRequest);
                obj_thread.Start();
            }
        }

        class SocketHandler
        {
            NetworkStream ns;
            Form1 _F1;
            public SocketHandler(TcpClient tc, Form1 F1)
            {
                _F1 = F1;
                ns = tc.GetStream();
            }

            public void ProcessSocketRequest()
            {
                FileStream fs = null;
                long current_file_pointer = 0;
                Boolean loop_break = false;

                _F1.Invoke(new Action(() =>
                {
                    _F1.label4.Text = "Recieveing";
                }
                ));

                while (true)
                {
                    if (ns.ReadByte() == 2)
                    {
                        byte[] cmd_buffer = new byte[3];
                        //接收傳送端Command確認要執行的動作  
                        ns.Read(cmd_buffer, 0, cmd_buffer.Length);
                        //從資料流讀取傳送端送來的資料  
                        byte[] recv_data = ReadStream();
                        switch (Convert.ToInt32(Encoding.UTF8.GetString(cmd_buffer)))
                        {
                            case 125: //Command 125 開始接收檔案  
                                {
                                    //已接收到的檔名作為建立檔案的名稱  
                                    fs = new FileStream(@"" + SaveTo +
                                    Encoding.UTF8.GetString(recv_data)
                                    , FileMode.CreateNew);
                                    //傳送 Command 126示意接收端要繼續接收檔案  Command
                                    //後面接著目前接收到的資料長度  
                                    byte[] data_to_send =
                                    CreateDataPacket(Encoding.UTF8.GetBytes("126"),
                                    Encoding.UTF8.GetBytes
                                    (Convert.ToString(current_file_pointer)));
                                    ns.Write(data_to_send, 0, data_to_send.Length);
                                    ns.Flush();
                                }
                                break;
                            case 127:  //Command: 127 接收檔案  
                                {
                                    //將檔案指標移動到當前接收位置  
                                    fs.Seek(current_file_pointer, SeekOrigin.Begin);
                                    //從資料流獲取內容儲存至檔案流中  
                                    fs.Write(recv_data, 0, recv_data.Length);
                                    //設定當前接收指標位置為目前所接收到的檔案長度  
                                    current_file_pointer = fs.Position;
                                    //傳送繼續接收資料Command 126
                                    //接收到的資料長度給傳送端  
                                    byte[] data_to_send =
                                    CreateDataPacket(Encoding.UTF8.GetBytes("126"),
                                    Encoding.UTF8.GetBytes
                                    (Convert.ToString(current_file_pointer)));
                                    ns.Write(data_to_send, 0, data_to_send.Length);
                                    ns.Flush();
                                }
                                break;
                            case 128: //Command: 128 傳送端 傳輸檔案 完畢  
                                {
                                    fs.Close();
                                    loop_break = true;
                                    _F1.Invoke(new Action(() =>
                                    {
                                        _F1.label4.Text = "Complete";
                                    }
                                    ));
                                    Thread.Sleep(1000);
                                    _F1.Invoke(new Action(() =>
                                    {
                                        _F1.label4.Text = "Listening";
                                    }
                                    ));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (loop_break == true)
                    {
                        ns.Close();
                        break;
                    }
                }
            }

            public byte[] ReadStream()
            {
                byte[] data_buff = null;

                int b = 0;
                string buff_Length = "";
                while ((b = ns.ReadByte()) != 4)
                {
                    buff_Length += (char)b;
                }
                int data_Length = Convert.ToInt32(buff_Length);
                data_buff = new byte[data_Length];
                int byte_Read = 0;
                int byte_Offset = 0;
                while (byte_Offset < data_Length)
                {
                    byte_Read = ns.Read(data_buff, byte_Offset, data_Length - byte_Offset);
                    byte_Offset += byte_Read;
                }

                return data_buff;
            }

            private byte[] CreateDataPacket(byte[] cmd, byte[] data)
            {
                byte[] initialize = new byte[1];
                initialize[0] = 2;
                byte[] separator = new byte[1];
                separator[0] = 4;
                byte[] dataLength =
                Encoding.UTF8.GetBytes(Convert.ToString(data.Length));
                MemoryStream ms = new MemoryStream();
                ms.Write(initialize, 0, initialize.Length);
                ms.Write(cmd, 0, cmd.Length);
                ms.Write(dataLength, 0, dataLength.Length);
                ms.Write(separator, 0, separator.Length);
                ms.Write(data, 0, data.Length);

                return ms.ToArray();
            }
        }
    }
}

