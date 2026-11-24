using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat.Clients
{
    public class PlcClient : TcClient
    {
        protected override int Port => 851;

        protected override string ClientName => "Plc";

        public PlcClient(ILogger logger) : base(logger.ForContext<PlcClient>())
        {

        }
    }
}
