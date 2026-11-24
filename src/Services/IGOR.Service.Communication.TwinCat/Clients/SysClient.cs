using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat.Clients
{

    /// <summary>
    /// TwinCat System Client
    /// </summary>
    public class SysClient : TcClient
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override int Port => 10000;

        protected override string ClientName => "Plc System";


        /// <summary>
        /// TwinCatSystem client
        /// </summary>
        public SysClient(ILogger logger) : base(logger.ForContext<SysClient>())
        {
        }
    }
}
