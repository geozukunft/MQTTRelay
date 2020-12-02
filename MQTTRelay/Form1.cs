using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.IO;
using System.IO.Ports;

namespace MQTTRelay
{
    public partial class Form1 : Form
    {
        MqttClient client;
        string clientID;
        public delegate void SendDataDelegate(String myString);
        public SendDataDelegate Delegate1;


        public Form1()
        {
            InitializeComponent();
            string adress = "Serveradress";
            string user = "username";
            string pw = "SUPERSECUREPASSWORDNOTFORYOUREYES";

            client = new MqttClient(adress, 8883, true, null, null, MqttSslProtocols.TLSv1_2);

            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            client.Connect(Guid.NewGuid().ToString(), user, pw);


            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            this.Delegate1 = new SendDataDelegate(sendData);


            client.Subscribe(new string[] { "car" }, new byte[] { 2 });



        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            consoleLog.Invoke(this.Delegate1, new object[] { indata } );

        }

        public void sendData(String myString)
        {
            consoleLog.Text = myString;
            //client.Publish("school/aiit", Encoding.UTF8.GetBytes(myString), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
            serialPort1.WriteLine(myString);
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(cboPorts.SelectedIndex == -1)
            {
                MessageBox.Show("Please select COM-POrt");
            }
            else
            {
                serialPort1.PortName = cboPorts.SelectedItem.ToString();
                if (!serialPort1.IsOpen)
                {
                    try
                    {
                        serialPort1.BaudRate = 9600;
                        serialPort1.Open();
                        MessageBox.Show("Port opened");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("A error occurred!");
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                MessageBox.Show("Port closed");
            }
            else
            {
                MessageBox.Show("Port not open");
            }
        }

        private void closeApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client.Disconnect();
            Application.Exit();
        }

        private void cboPorts_Click(object sender, EventArgs e)
        {
            ListCom();
        }


        private void ListCom()
        {
            //Write all available ports into String
            string[] ports = SerialPort.GetPortNames();

            //Display available Ports
            foreach (string port in ports)
            {
                cboPorts.Items.Add(port);
            }

        }

        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            consoleLog.Invoke(this.Delegate1, new object[] { ReceivedMessage });


        }

    }
}
