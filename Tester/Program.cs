﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            DomainConnectDDNSUpdate.DomainConnectDDNSWorker worker = new DomainConnectDDNSUpdate.DomainConnectDDNSWorker(null);
            //worker.DoWork();
            worker.RefreshToken();
        }
    }
}
