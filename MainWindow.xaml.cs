using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace iDCT2
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient m_client;
        private delegate void delUpdateUI(string sMessage);

        TcpListener m_server;
        Thread m_thrListening; 
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
        }

        //關閉時將物件清空
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (m_client != null)
            {
                m_client.Close();
                m_client = null;
            }
            if (m_server != null)
            {
                m_server.Stop();
            }
            if (m_thrListening != null)
            {
                m_thrListening.Abort();
            }
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        //輸入的連線
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int nPort = int.Parse(txtPort.Text);
                m_client = new TcpClient(txtIP.Text, nPort);
                btnConnect.IsEnabled = false;
                btnDisconnect.IsEnabled = true;
            }
            catch (ArgumentNullException a)
            {
                Console.WriteLine("ArgumentNullException:{0}", a);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException:{0}", ex);
            }
        }

        //輸入的斷線
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            m_client.Close();
            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
        }

        //接收訊息的連線
        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int nPort = Convert.ToInt32(txtPort.Text)+1000; 
                IPAddress localAddr = IPAddress.Parse(txtIP.Text); 

                m_server = new TcpListener(localAddr, nPort);
                m_server.Start();
                m_thrListening = new Thread(Listening);
                m_thrListening.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
            }
        }

        //接收訊息的監聽
        private void Listening()
        {
            try
            {
                byte[] btDatas = new byte[256]; 
                string sData = null;

                while (true)
                {
                    UpdateStatus("Waiting");
                    TcpClient client = m_server.AcceptTcpClient(); 
                    UpdateStatus("Connect");
                    sData = null;
                    NetworkStream stream = client.GetStream();
                    int i;
                    try
                    {
                        while ((i = stream.Read(btDatas, 0, btDatas.Length)) != 0) 
                        {
                            sData = Encoding.UTF8.GetString(btDatas, 0, i);
                            UdpateMessage(sData);
                            Thread.Sleep(1);
                        }
                        client.Close();
                        Thread.Sleep(1);
                    }
                    catch
                    {
                        UpdateStatus("Waiting");
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
            }
        }

        //更新接收狀態
        private void UpdateStatus(string sStatus)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(UpdateStatus);
                this.Dispatcher.Invoke(del, sStatus);
            }
            else
            {
                labStatus.Content = sStatus;
            }
        }

        //更新接收訊息
        private void UdpateMessage(string sReceiveData)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(UdpateMessage);
                this.Dispatcher.Invoke(del, sReceiveData);
            }
            else
            {
                txtMessage.Text = sReceiveData + Environment.NewLine;
            }
        }

        //輸入的訊息欄
        private void txtData_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            byte[] btData = Encoding.UTF8.GetBytes(txtData.Text); 
            try
            {
                if (m_client != null)
                {
                    NetworkStream stream = m_client.GetStream();
                    stream.Write(btData, 0, btData.Length); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Write Exception:{0}", ex);
            }
            finally
            {
                txtINPUT.Text = txtData.Text;
                txtData.Text = "";
            }

        }

    }
}
