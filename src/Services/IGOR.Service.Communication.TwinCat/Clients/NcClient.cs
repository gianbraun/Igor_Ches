using IGOR.Interface.Communication.Axis;
using IGOR.Service.Communication.TwinCat.Axis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using TwinCAT.Ads;

namespace IGOR.Service.Communication.TwinCat.Clients
{
    /// <summary>
    /// TwinCat Nc Client
    /// </summary>
    public class NcClient : TcClient
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override int Port => 500;

        protected override string ClientName => "NC";


        public IReadOnlyList<IAxis> Axis { get; private set; }

        /// <summary>
        /// TwinCat Nc Client
        /// </summary>
        public NcClient(ILogger logger) : base(logger.ForContext<NcClient>())
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnAdsStateChanged(AdsState state)
        {
            base.OnAdsStateChanged(state);
            try
            {
                if (IsConnected && State == state)
                    GetAxis();
            }
            catch (Exception ex)
            {
                log.Warning(ex, "Fehler beim Auslesen der Achsen");
            }


        }

        private void GetAxis()
        {
            var axes = new List<TcAxis>();
            TryRead<uint>(0x1100, 0x3, out var count);
            var ids = ReadArray<uint>(0x1100, 0x33, count);

            foreach (var id in ids)
            {
                var name = ReadString(0x4000 + id, 0x2, 31, Encoding.ASCII);
                var axis = new TcAxis(name, id, this, log);
                axes.Add(axis);
            }
            Axis = axes;
        }

    }
}
