using IGOR.Interface.Communication.Actor;
using IGOR.Interface.Communication.Axis;
using IGOR.Interface.Communication.Job;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace IGOR.Interface.Communication
{
    /// <summary>
    /// Basic interface for sps communication
    /// </summary>
    public interface IPlcCom : INotifyPropertyChanged, IDisposable
    {

        event EventHandler PlcConnected;
        event EventHandler SysConnected;
        event EventHandler NcConnected;

        public event EventHandler<PlcState> SysStateChanged;
        public event EventHandler<PlcState> PlcStateChanged;
        public event EventHandler<PlcState> NcStateChanged;
        
        public event EventHandler<PlcJob> PlcJobDone;
        public event EventHandler PlcError;

        ObservableCollection<PlcJob> PendingPlcJobs { get; }
        public bool JobsExecuting { get; }

        /// <summary>
        /// Get the current sps state
        /// </summary>

        PlcState SysState { get; }

        PlcState PlcState { get; }

        PlcState NcState { get; }


        /// <summary>
        /// Collection of Nc Axes
        /// </summary>
        IReadOnlyCollection<IAxis> Axes { get; }

        IReadOnlyCollection<IActor> Actors { get; }

        /// <summary>
        /// Indicates wether the sps is connected or not
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Connect to the sps 
        /// </summary>
        /// <param name="address">the ip-address of the sps</param>
        void Connect(string address);


        void Connect();

        /// <summary>
        /// Disconnect from the sps
        /// </summary>
        void Disconnect();

        /// <summary>
        /// writes a value to the sps
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        /// <returns>true if sucess otherwise false</returns>
        bool TryWrite(string variable, object value);


        /// <summary>
        /// Reads a value from the sps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        /// <returns> true if sucess otherwise false</returns>
        bool TryRead<T>(string variable, out T value);

        bool TryRead(string variable, Type variableType , out object value);

        bool TryObserve<T>(string variable, Action<T> callback);

        void RemoveObserver(string variable);

        bool EnqueueJob(PlcJob job);

        bool EnqueueJobs(IEnumerable<PlcJob> jobs, bool sortJobs = false);

        Task<bool> ExecuteJobs(IEnumerable<PlcJob> jobs, CancellationToken cancelJobs, bool sortJobs = false);

        bool SetLight(bool bActivateLight);
    }
}
