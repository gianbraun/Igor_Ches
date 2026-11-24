using IGOR.Interface.Communication.Axis;
using IGOR.Service.Communication.TwinCat.Clients;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat.Axis
{
    public class TcAxis : IAxis
    {
        private NcClient _ncClient;
        private readonly ILogger _log;

        public string Name { get; }
        public uint Id { get; }

        public IAxisState State { get; } = new TcAxisState();
        public IAxisPos Pos { get; } = new TcAxisPos();
        public IAxisRelease Release { get; } = new TcAxisRelease();
        public IAxisParam Params { get; } = new TcAxisParam();

        public event PropertyChangedEventHandler PropertyChanged;        

        public TcAxis(string name, uint id, NcClient ncClient, ILogger log)
        {
            Name = name;
            Id = id;
            _ncClient = ncClient;
            _log = log.ForContext<TcAxis>();
            ReadAxisData();
            AttachContiniousChange();
        }


        public void Move(double destination, double speed, PositioningType positioningType)
        {
            var buff = new byte[24];
            using var stream = new MemoryStream(buff);
            var writer = new BinaryWriter(stream);
            writer.Write((uint)positioningType);
            writer.Write(0);
            writer.Write(destination);
            writer.Write(speed);
            _ncClient.TryWrite(0x4200 + Id, 0x20, buff);
        }

        public void Stop()
        {
            _ncClient.TryWrite(0x4200 + Id, 0x2);
        }

        private void ReadAxisData()
        {
            try
            {
                ReadAxisPos((TcAxisPos)Pos);
                ReadAxisRelease((TcAxisRelease)Release);
                ReadAxisState((TcAxisState)State);
                ReadAxisParams((TcAxisParam)Params);
            }
            catch (Exception ex)
{
                _log.Warning(ex, "Achsdaten für {achsName} können nicht gelesen werden", Name);
            }
        }

        private void AttachContiniousChange()
        {
            var posCancellation = new CancellationTokenSource();
            var releaseCancellation = new CancellationTokenSource();
            var stateCancellation = new CancellationTokenSource();

            Pos.ContiniusRefreshChanged += (s, e) =>
            {
                if (Pos.ContiniusRefresh)
                {
                    posCancellation = new CancellationTokenSource();
                    StartAxisPosPolling(posCancellation.Token);
                }

                else
                {
                    posCancellation.Cancel();
                }
            };

            Release.ContiniusRefreshChanged += (s, e) =>
            {
                if (Release.ContiniusRefresh)
                {
                    releaseCancellation = new CancellationTokenSource();
                    StartAxisReleasePolling(releaseCancellation.Token);
                }
                else
                {
                    releaseCancellation.Cancel();
                }
            };

            State.ContiniusRefreshChanged += (s, e) =>
            {
                if (State.ContiniusRefresh)
                {
                    StartAxisStatePolling(stateCancellation.Token);
                    stateCancellation = new CancellationTokenSource();
                }
                else
                {
                    stateCancellation.Cancel();
                }
            };
        }

        private void ReadAxisRelease(TcAxisRelease release)
        {
            release.FeedFW = _ncClient.TryRead<bool>(0x4300 + Id, 0x3, out var feedFw);
            release.FeedBW = _ncClient.Read<bool>(0x4300 + Id, 0x4);
            release.ControllerRelease = _ncClient.Read<bool>(0x4300 + Id, 0x2);
            release.Override = _ncClient.Read<uint>(0x4300 + Id, 0x21) / 10000d;
        }

        private void ReadAxisPos(TcAxisPos pos)
        {
            pos.ActualPos = _ncClient.Read<double>(0x4300 + Id, 0xBA);
            pos.TargetPos = _ncClient.Read<double>(0x4300 + Id, 0xBF);
        }

        private void ReadAxisState(TcAxisState state)
        {
            state.MoveFW = _ncClient.Read<bool>(0x4300 + Id, 0x8A);
            state.MoveBW = _ncClient.Read<bool>(0x4300 + Id, 0x8B);
            state.NotMoving = _ncClient.Read<bool>(0x4300 + Id, 0x8C);
            state.InDestinationPos = _ncClient.Read<bool>(0x4300 + Id, 0x8F);
            state.HasJob = _ncClient.Read<bool>(0x4300 + Id, 0x9B);
            state.Ready = _ncClient.Read<bool>(0x4300 + Id, 0x82);
            state.Referenzed = _ncClient.Read<bool>(0x4300 + Id, 0x83);
        }

        private void ReadAxisParams(TcAxisParam param)
        {
            param.HandFast = _ncClient.Read<double>(0x4000 + Id, 0x9);
            param.HandSlow = _ncClient.Read<double>(0x4000 + Id, 0x8);
            param.MinPos = _ncClient.Read<double>(0x5000 + Id, 0xD);
            param.MaxPos = _ncClient.Read<double>(0x5000 + Id, 0xE);
            var posMonitoringDWord = _ncClient.Read<uint>(0x4300 + Id, 0xB9);
            param.SoftlimitNegAktive = (posMonitoringDWord & (1 << 18)) != 0;
            param.SoftlimitPosAktive = (posMonitoringDWord & (1 << 19)) != 0;
        }

      
        private void StartAxisStatePolling(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    ReadAxisState((TcAxisState)State);
                    Thread.Sleep(100);
                }
            }, token);
        }

        private void StartAxisPosPolling(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    ReadAxisPos((TcAxisPos)Pos);
                    Thread.Sleep(100);
                }

            }, token);
        }

        private void StartAxisReleasePolling(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    ReadAxisRelease((TcAxisRelease)Release);
                    Thread.Sleep(100);
                }
            }, token);
        }
    }
}
