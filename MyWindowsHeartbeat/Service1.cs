using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace MyWindowsHeartbeat
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        
        protected override void OnStart(string[] args)
        {
            Start_();
        }

        protected  override void OnStop()
        {
            Stop_();            
        }

        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private System.Threading.Timer timer;

               

        UdpClient cli = new UdpClient();
        byte[] msg = null;

        System.DateTime lastRecv = DateTime.MinValue;
        System.DateTime PartUpSince = DateTime.MaxValue;
        DateTime OwnUpSince = DateTime.Now;

        public void Start_()
        {
            #region Ereignisse 

            if (!System.Diagnostics.EventLog.SourceExists("MyWindowsHeartbeat"))
                System.Diagnostics.EventLog.CreateEventSource("MyWindowsHeartbeat", "Application");

            #endregion

            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, Settings1.Default.OwnPort));

                cli.Connect(new IPEndPoint(IPAddress.Parse(Settings1.Default.PartnerServer), Settings1.Default.PartnerPort));

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                msg = encoding.GetBytes("Hallo Schatzi");

                lastRecv = System.DateTime.Now.AddSeconds(Settings1.Default.OwnInitWait);

                timer = new System.Threading.Timer(new System.Threading.TimerCallback(timer_Tick), null, 1000, 1000);

                eventLog1.WriteEntry("Gestartet");
            }
            catch (System.Exception ex)
            {
                this.eventLog1.WriteEntry(ex.ToString());
                
            }
        }

        public void DoScript(string s)
        {
            try
            {
                if (s.Length > 0)
                {

                    System.Diagnostics.Process.Start(s);

                }
                
            }
            catch (System.Exception ex)
            {
                eventLog1.WriteEntry(s + "  " + ex.Message, EventLogEntryType.Error); 
            }
        }

        public void Stop_()
        {
            timer.Dispose();
            server.Close();

            eventLog1.WriteEntry("Beendet");

            DoScript(Settings1.Default.OwnDownScript);
            
        }

        bool warnSend = false;
        bool ErrorSend = false;
        bool OwnUp = false;
        bool PartnerUp = false; 

        private void timer_Tick(object sender)
        {

            #region Partner vermisst
            if ((lastRecv < DateTime.Now.AddSeconds(-Settings1.Default.LimitWarningTimeout)) & !warnSend)
            {
                eventLog1.WriteEntry("kein Lifetick von dem anderen Teilnehmer (Warngrenze)", EventLogEntryType.Warning);
                warnSend = true;
            }
            #endregion

            #region Partner Weg => Error

            if ((lastRecv < DateTime.Now.AddSeconds(-Settings1.Default.LimitErrorTimeout)) & !ErrorSend)
            {
                eventLog1.WriteEntry("kein Lifetick von dem anderen Teilnehmer (Error) => failover", EventLogEntryType.Error);
                ErrorSend = true;
                PartnerUp = false;

                PartUpSince = DateTime.MaxValue;
                DoScript(Settings1.Default.PartnerDownScript);
            }

            #endregion

            #region Partner wieder da
            if (!PartnerUp & PartUpSince < DateTime.Now.AddSeconds(-Settings1.Default.PartnerBackAliveWait))
            {
                PartnerUp = true;
                ErrorSend = false;
                warnSend = false;

                DoScript(Settings1.Default.PartnerUpScript);

                eventLog1.WriteEntry("Partner gültig", EventLogEntryType.SuccessAudit);
            }
            #endregion

            #region ich bin da 

            if (!OwnUp & OwnUpSince < DateTime.Now.AddSeconds(-Settings1.Default.OwnInitWait))
            {
                OwnUp = true;

                DoScript(Settings1.Default.OwnUpScript);

                eventLog1.WriteEntry("Ich gültig", EventLogEntryType.SuccessAudit);

            }
            #endregion

            #region Nachricht versenden
            cli.Send( msg,msg.Length);

            #endregion 

            #region Nachricht empfangen

            byte []  buffer = new byte[255];
            
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

            int i  = server.ReceiveFrom(buffer,buffer.Length, SocketFlags.None,ref ep);

            if (i >0)
            {
                if (((IPEndPoint)ep).Address.ToString() == Settings1.Default.PartnerServer)
                {
                    lastRecv = DateTime.Now;

                    if (PartUpSince == DateTime.MaxValue)
                    {
                        PartUpSince = DateTime.Now;
                    }

                    warnSend = false;
                }
                else
                {
                    eventLog1.WriteEntry("Bekomme Packerl von " + ((IPEndPoint)ep).Address.ToString(), EventLogEntryType.Warning);
                }
            }
            
            #endregion
      
        }
    }
}
