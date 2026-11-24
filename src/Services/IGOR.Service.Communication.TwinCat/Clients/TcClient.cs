using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace IGOR.Service.Communication.TwinCat.Clients
{
    public abstract class TcClient : IDisposable
    {
        protected readonly ILogger log;
        protected virtual string ClientName { get; }

        private Dictionary<string, Action<object>> _observedVariables = new Dictionary<string, Action<object>>();
        private Dictionary<string, uint> _observedVariableHandles = new Dictionary<string, uint>();
        private Dictionary<uint, Action<object>> _observedIndexes = new Dictionary<uint, Action<object>>();

        private CancellationTokenSource _statePollingCancelationSource = new CancellationTokenSource();

        public event EventHandler<AdsState> AdsStateChanged;
        public event EventHandler ClientConnected;

        public AdsState State { get; private set; }

        public bool IsConnected => _adsClient.IsConnected;

        private readonly AdsClient _adsClient = new AdsClient();

        [DoNotNotify]
        protected abstract int Port { get; }



        public TcClient(ILogger loggerWithContext)
        {
            log = loggerWithContext;
            _adsClient.AdsNotificationEx += AdsVariableChanged;
        }

        private void AdsVariableChanged(object sender, AdsNotificationExEventArgs e)
        {
            if (e.UserData is uint index)
            {
                _observedIndexes[index].Invoke(e.Value);
            }
            else if (e.UserData is string variable)
            {
                _observedVariables[variable].Invoke(e.Value);
            }

        }

        public virtual bool Connect()
        {
            try
            {
                _adsClient.Timeout = 1000;
                _adsClient.Connect(Port);
                _adsClient.ReadState();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Lokal Verbindung zur {clientName} kann nicht hergestellt werden ({port})", ClientName, Port);
                return false;
            }
            if (IsConnected)
            {
                StartStatePolling(_statePollingCancelationSource.Token);
                OnConnected();
                return true;
            }
            log.Error("Lokale Verbindung zur {clientName} kann nicht hergestellt werden ({port})", ClientName, Port);
            return false;
        }

        /// <summary>
        /// Connects the Client
        /// </summary>
        /// <param name="amsAddress">address of the client</param>
        public virtual bool Connect(string amsAddress)
        {
            try
            {
                _adsClient.Connect(new AmsNetId(amsAddress), Port);
                _adsClient.ReadState();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Verbindung zur {clientName} kann nicht hergestellt werden ({amsNetId}:{port})", ClientName, amsAddress, Port);
                return false;
            }
            if (IsConnected)
            {
                StartStatePolling(_statePollingCancelationSource.Token);
                OnConnected();
                return true;
            }
            log.Error("Verbindung zur {clientName} kann nicht hergestellt werden ({amsNetId}:{port})", ClientName, amsAddress, Port);
            return false;
        }


        /// <summary>
        /// Disconnects the Client
        /// </summary>
        public virtual void Disconnect()
        {
            if (IsConnected)
            {
                _adsClient.Disconnect();
                OnDisconnected();
            }

        }

        protected virtual void OnConnected()
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {

        }


        public void StartStatePolling(CancellationToken cancellationToken)
        {
            if (!IsConnected)
                return;

            var task = Task.Run(() =>
            {
                while (true)
                {
                    _adsClient.TryReadState(out var state);
                    if (State != state.AdsState)
                    {
                        State = state.AdsState;
                        OnAdsStateChanged(State);
                    }

                    Thread.Sleep(500);
                }
            }, cancellationToken);
        }

        protected T Read<T>(string variable)
        {
            return (T)Read(variable, typeof(T));
        }

        internal T Read<T>(uint indexGroup, uint indexOffset)
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)(_adsClient.ReadAny<short>(indexGroup, indexOffset) > 0);
            }
            return _adsClient.ReadAny<T>(indexGroup, indexOffset);
        }

        internal object Read(string variable, Type type)
        {
            var handel = _adsClient.CreateVariableHandle(variable);
            var val = _adsClient.ReadAny(handel, type);
            _adsClient.DeleteVariableHandle(handel);
            return val;

        }

        public bool TryReadArray<T>(string varibale, uint numElements, out T[] value)
        {
            value = new T[numElements];
            var size = Marshal.SizeOf<T>();
            var buff = new byte[size * numElements];
            var handle = _adsClient.CreateVariableHandle(varibale);
            try
            {
                _adsClient.Read(handle, buff);

            }
            catch (Exception ex)
            {
                log.Error(ex, "Array \"{0}\" kann nicht aus SPS gelesen werden", varibale);
                return false;
            }

            unsafe
            {
                fixed (byte* p = buff)
                {
                    var localPtr = p;
                    for (int i = 0; i < numElements; i++)
                    {
                        value[i] = Marshal.PtrToStructure<T>((IntPtr)localPtr);
                        localPtr += size;
                    }
                }
            }

            return true;
        }

        protected T[] ReadArray<T>(uint indexGroup, uint indexOffset, uint numElements)
        {
            T[] result = new T[numElements];
            var size = Marshal.SizeOf<T>();
            var buff = new byte[size * numElements];
            _adsClient.Read(indexGroup, indexOffset, buff);
            unsafe
            {
                fixed (byte* p = buff)
                {
                    var localPtr = p;
                    for (int i = 0; i < numElements; i++)
                    {
                        result[i] = Marshal.PtrToStructure<T>((IntPtr)localPtr);
                        localPtr += size;
                    }
                }
            }

            return result;
        }

        protected string ReadString(uint indexGroup, uint indexOffset, uint size, Encoding encoding)
        {
            var buff = new byte[size];
            _adsClient.Read(indexGroup, indexOffset, buff);
            var result = encoding.GetString(buff);
            return result.Replace("\0", string.Empty);
        }

        public bool TryRead<T>(uint indexGroup, uint indexOffset, out T value)
        {
            value = default;
            try
            {
                value = Read<T>(indexGroup, indexOffset);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryRead<T>(string variable, out T value)
        {
            value = default;
            try
            {
                value = Read<T>(variable);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Plc Variable {0} konnte nicht gelesen werden", variable);
                return false;
            }

            return true;
        }

        public bool TryRead(string variable, Type variableType, out object value)
        {
            value = default;
            try
            {
                value = Read(variable, variableType);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Plc Variable {0} konnte nicht gelesen werden", variable);
                return false;
            }

            return true;
        }

        private void Write(string variable, object value)
        {
            var handle = _adsClient.CreateVariableHandle(variable);
            _adsClient.WriteAny(handle, value);
            _adsClient.DeleteVariableHandle(handle);
        }

        private void Write(uint indexGroup, uint offsetGroup, Memory<byte> buffer)
        {
            _adsClient.Write(indexGroup, offsetGroup, buffer);
        }

        private void Write(uint indexGroup, uint indexOffset)
        {
            _adsClient.Write(indexGroup, indexOffset);
        }

        internal bool TryWrite(uint indexGroup, uint indexOffset)
        {
            try
            {
                Write(indexGroup, indexOffset);
            }
            catch (Exception ex)
            {
                log.Error(ex, "SPS indexgrouppe {0} an indexOffset {1} konnte nicht gelesen werden", indexGroup, indexOffset);
                return false;
            }
            return true;
        }

        internal bool TryWrite(uint indexGroup, uint offsetGroup, Memory<byte> buffer)
        {
            try
            {
                _adsClient.Write(indexGroup, offsetGroup, buffer);
            }
            catch (Exception ex)
            {
                log.Error(ex, "SPS indexgrouppe {0} an indexOffset {1} konnte nicht gelesen werden", indexGroup, offsetGroup );
                return false;
            }
            return true;
        }

        public bool TryWrite(string variable, object value)
        {
            try
            {
                Write(variable, value);
            }
            catch (Exception ex)
            {
                log.Error(ex, "SPS Variable {0} konnte nicht beschrieben werden", variable);
                return false;
            }
            return true;
        }

        public bool TryObserve<T>(string variable, Action<T> callback)
        {
            try
            {
                _observedVariables.Add(variable, c => callback((T)c));
                var handle = _adsClient.AddDeviceNotificationEx(variable,
                new NotificationSettings(AdsTransMode.OnChange, 100, 100), variable, typeof(T));
                _observedVariableHandles.Add(variable, handle);
            }
            catch (Exception ex)
            {
                _observedVariables.Remove(variable);
                log.Error(ex, "SPS Variable {0} konnte nicht gelesen werden", variable);
                return false;
            }
            return true;
        }

        public void Observe<T>(uint index, uint offset, Action<T> callback)
        {
            _observedIndexes.Add(index + offset, c => callback((T)c));
            _adsClient.AddDeviceNotificationEx(index, offset,
                new NotificationSettings(AdsTransMode.OnChange, 100, 100), index + offset, typeof(T));
        }

        public void RemoveObserver(string variable)
        {
            if (!_observedVariableHandles.TryGetValue(variable, out var handle))
                return;

            _adsClient.DeleteDeviceNotification(handle);
            _observedVariableHandles.Remove(variable);
            _observedVariables.Remove(variable);
        }



        protected virtual void OnAdsStateChanged(AdsState state)
        {
            AdsStateChanged?.Invoke(this, state);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _adsClient.Disconnect();
                _adsClient.Dispose();
            }
        }

        #endregion
    }
}
