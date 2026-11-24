using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Actor;
using IGOR.Interface.Communication.Axis;
using IGOR.Interface.Communication.Job;
using IGOR.Interface.Logging;
using IGOR.Service.Communication.TwinCat.Clients;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TwinCAT.Ads;

namespace IGOR.Service.Communication.TwinCat
{
    // IMPORTANT!! complex types with bools must have struclayout explicit with bool size of 2 bytes

    /// <summary>
    /// ISps com Implementation for TwinCat
    /// </summary>
    internal sealed class TwinCatCom : IPlcCom, IDisposable
    {
        private readonly PlcClient _plcClient;
        private readonly SysClient _sysClient;
        private readonly NcClient _ncClient;
        private readonly CancellationTokenSource _pollingCancelationSource = new();
        private readonly ILogger _log;
        private readonly IAppLogger _appLogger;

        public ObservableCollection<PlcJob> PendingPlcJobs { get; set; }
        private PlcJob _lastJob;
        private bool _jobsExecuting;
        public bool JobsExecuting
        {
            get => _jobsExecuting; private set
            {
                _jobsExecuting = value;
            }
        }




        /// <summary>
        /// ISps com Implementation for TwinCat
        /// </summary>
        public TwinCatCom(ILogger logger, IAppLogger appLogger)
        {
            _log = logger.ForContext<TwinCatCom>();
            _appLogger = appLogger;
            _plcClient = new PlcClient(logger);
            _sysClient = new SysClient(logger);
            _ncClient = new NcClient(logger);
            PendingPlcJobs = new ObservableCollection<PlcJob>();

            _plcClient.AdsStateChanged += (s, e) =>
            {
                var state = AdsStateToSpsState(e);
                PlcState = state;
                OnPlcStateChanged(state);
            };
            _sysClient.AdsStateChanged += (s, e) =>
            {
                var state = AdsStateToSpsState(e);
                SysState = state;
                OnSysStateChanged(state);
            };

            _ncClient.AdsStateChanged += (s, e) =>
            {
                var state = AdsStateToSpsState(e);
                NcState = state;
                OnNcStateChanged(state);
            };
            try
            {
                Actors = Actor.LoadActors();
                //Actor.GenerateSampleJson();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Fehler beim lesen der Actors.json");
            }

            //AttachJobObserver();

            // TEST
            PendingPlcJobs.CollectionChanged += TestCollectionChanged;
        }

        /// <summary>
        /// Debug Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    //_log.Debug($"Job added. \n{PendingPlcJobs.Count} Jobs pending."); 
                    Debug.WriteLine($"{PendingPlcJobs.Count} Jobs pending.");
                    foreach (PlcJob job in e.NewItems)
                    {
                        Debug.WriteLine($"\t[{job.JobID} | MoveType = {job.MoveType} | IsBlack = {job.IsBlack}]");
                    }

                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    //_log.Debug($"Job removed. \n{PendingPlcJobs.Count} Jobs pending.");
                    Debug.WriteLine($"{PendingPlcJobs.Count} Jobs pending.");
                }
            }
        }

        private void OnPlcConnected(EventArgs e)
        {
            PlcConnected?.Invoke(this, e);
        }

        private void AttachJobObserver()
        {
            _plcClient.TryObserve<PlcJob>("HMI.stLastJob", LastJobChanged);
        }

        private void DetachJobObserver()
        {
            _plcClient.RemoveObserver("HMI.stLastJob");
        }

        private void LastJobChanged(PlcJob lastJob)
        {
            // TODO: überprüfen, ob hier auch einfach .Remove(lastJob) funktioniert.
            PendingPlcJobs.Remove(PendingPlcJobs.Where(x => x.JobID == lastJob.JobID).FirstOrDefault());
            InvokePlcJobDone(lastJob);
        }


        private void AttachPlcLogger()
        {
            _plcClient.TryObserve<bool>("HMI.bErrorChange", ErrorDetected);
            ErrorDetected(true);
        }

        private void DetachPlcLogger()
        {
            _plcClient.RemoveObserver("HMI.bErrorChange");
        }

        private void ErrorDetected(bool bError)
        {
            if (!bError)
                return;
            _plcClient.TryWrite("HMI.bErrorChange", false);
            Application.Current.Dispatcher.Invoke(_appLogger.ClearPlcLog);
            _plcClient.TryRead<ushort>("HMI.nNumberErrors", out var numErrors);
            if (numErrors <= 0)
            {
                return;
            }

            PlcError?.Invoke(this, EventArgs.Empty);

            _plcClient.TryReadArray<PlcError>("HMI.aErrors", numErrors, out var errors);
            foreach (var error in errors)
            {
                //if (error.Source > 0)
                Application.Current.Dispatcher.Invoke(() => _appLogger.AddPlcLog(error));
            }
        }

        private void OnSysConnected(EventArgs e)
        {
            SysConnected?.Invoke(this, e);
        }

        private void OnNcConnected(EventArgs e)
        {
            NcConnected?.Invoke(this, e);
        }

        private void OnPlcStateChanged(PlcState state)
        {
            PlcStateChanged?.Invoke(this, state);
            if (PlcState == PlcState.Run)
            {
                AttachPlcLogger();
                AttachJobObserver();
            }
            else
            {
                DetachPlcLogger();
                DetachJobObserver();
            }
        }

        private void OnSysStateChanged(PlcState state)
        {
            SysStateChanged?.Invoke(this, state);
        }

        private void OnNcStateChanged(PlcState state)
        {
            NcStateChanged?.Invoke(this, state);
        }

        private PlcState AdsStateToSpsState(AdsState state)
        {
            switch (state)
            {
                case AdsState.Invalid:
                    return PlcState.Invalid;

                case AdsState.Idle:
                    return PlcState.Run;

                case AdsState.Init:
                    return PlcState.Init;

                case AdsState.Start:
                    return PlcState.Start;

                case AdsState.Run:
                    return PlcState.Run;

                case AdsState.Stop:
                    return PlcState.Stop;

                case AdsState.PowerFailure:
                    return PlcState.Error;

                case AdsState.Error:
                    return PlcState.Error;

                case AdsState.Config:
                    return PlcState.Config;

                case AdsState.Incompatible:
                    return PlcState.Invalid;

                case AdsState.Exception:
                    return PlcState.Error;

                default:
                    return PlcState.Invalid;
            }
        }

        private void InvokePlcJobDone(PlcJob job)
        {
            PlcJobDone?.Invoke(this, job);
        }

        private void OnAcknowledged()
        {
            // AppLog löschen 
            Application.Current.Dispatcher.Invoke(_appLogger.ClearAppLog);


            // Jobs löschen
            PendingPlcJobs.Clear();
        }

        #region Interface implementation

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event EventHandler SysConnected;
        public event EventHandler PlcConnected;
        public event EventHandler NcConnected;

        public event EventHandler<PlcState> SysStateChanged;
        public event EventHandler<PlcState> PlcStateChanged;
        public event EventHandler<PlcState> NcStateChanged;

        public event EventHandler<PlcJob> PlcJobDone;
        public event EventHandler PlcError;


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Connected => _plcClient.IsConnected;


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyCollection<IAxis> Axes => _ncClient.Axis;

        public IReadOnlyCollection<IActor> Actors { get; private set; }

        public PlcState PlcState { get; private set; }

        public PlcState NcState { get; private set; }

        public PlcState SysState { get; private set; }


        public void Connect()
        {
            if (_plcClient.Connect())
            {
                OnPlcConnected(EventArgs.Empty);
            }

            if (_sysClient.Connect())
            {
                OnSysConnected(EventArgs.Empty);
            }
            if (_ncClient.Connect())
            {
                OnNcConnected(EventArgs.Empty);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Connect(string address)
        {
            if (_plcClient.Connect(address))
            {
                OnPlcConnected(EventArgs.Empty);
            }

            if (_sysClient.Connect(address))
            {
                OnSysConnected(EventArgs.Empty);
            }


            if (_ncClient.Connect(address))
            {
                OnNcConnected(EventArgs.Empty);
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Disconnect()
        {
            _pollingCancelationSource.Cancel();
            _plcClient.Disconnect();
            _sysClient.Disconnect();
            _ncClient.Disconnect();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryRead<T>(string variable, out T value)
        {
            return _plcClient.TryRead(variable, out value);
        }

        public bool TryRead(string variable, Type variableType, out object value)
        {
            return _plcClient.TryRead(variable, variableType, out value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryWrite(string variable, object value)
        {
            if (variable == "HMI.stSoftKeyQuit.bIsPressed" && value is bool pressed && pressed)
                OnAcknowledged();

            return _plcClient.TryWrite(variable, value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool TryObserve<T>(string variable, Action<T> callback)
        {
            return _plcClient.TryObserve(variable, callback);
        }

        public void RemoveObserver(string variable)
        {
            _plcClient.RemoveObserver(variable);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            _plcClient.TryWrite("HMI.stSoftKeyCtrlOff.bIsPressed", true);
            Thread.Sleep(200);
            _plcClient.TryWrite("HMI.stSoftKeyCtrlOff.bIsPressed", false);
            //DetachJobObserver();
            Disconnect();
            _plcClient.Dispose();
            _ncClient.Dispose();
            _sysClient.Dispose();
        }


        public bool EnqueueJob(PlcJob job)
        {
            var valid = false;
            for (int i = 0; i < 40; i++)
            {
                if (!_plcClient.TryRead<bool>("HMI.bSetJob", out var setJob))
                {
                    valid = false;
                    break;
                }

                if (setJob)
                {
                    Thread.Sleep(50);
                    continue;
                }
                valid = TryWrite("HMI.stJob", job) && TryWrite("HMI.bSetJob", true);
                break;
            }
            if (!valid)
                _log.Error("Portalkomando konnte nicht übertragen werden");

            // Debug um zu testen, welche Jobs geschrieben werden
            Debug.WriteLine($"Job {job.JobID}: Start ({job.StartPosX} | {job.StartPosY}) -> End ({job.EndPosX} | {job.EndPosY})  {job.MoveType}");
            PendingPlcJobs.Add(job);

            return valid;
        }

        public bool EnqueueJobs(IEnumerable<PlcJob> jobs, bool sortJobs = false)
        {
            if (sortJobs)
                jobs = SortJobs(jobs);

            foreach (var job in jobs)
            {

                if (!EnqueueJob(job))
                    return false;
            }

            // warten auf NCI
            Thread.Sleep(500);

            _plcClient.TryWrite("HMI.bAllJobsSend", true);

            return true;
        }

        public async Task<bool> ExecuteJobs(IEnumerable<PlcJob> jobs, CancellationToken cancelJobsToken, bool sortJobs = false)
        {
            var enqueuedJobs = jobs.ToList();

            if (!EnqueueJobs(jobs, sortJobs))
            {
                return false;
            }

            JobsExecuting = true;

            var jobsExecuted = false;
            var aborted = false;

            PlcError += (s, args) =>
            {
                aborted = true;
            };
            try
            {

                await Task.Run(() =>
                {
                    // wait for portal to finish the jobs
                    while (!jobsExecuted && !aborted)
                    {
                        if (cancelJobsToken.IsCancellationRequested)
                        {
                            cancelJobsToken.ThrowIfCancellationRequested();
                        }

                        // wenn enqueuedJobs nicht mehr in PendingPlcJobs enthalten sind
                        if (!enqueuedJobs.Any(job => PendingPlcJobs.Contains(job)))
                        {
                            jobsExecuted = true;
                        }
                    }
                }, cancelJobsToken);

            }
            catch (OperationCanceledException)
            {
                // TODO: Quittieren, bzw. Jobs löschen
                jobsExecuted = false;
            }


            // TODO: muss das?
            PlcError -= (s, args) => aborted = true;


            if (jobs.Count() > 0 && jobsExecuted)
            {
                // wait for NCI
                await Task.Delay(1500);
            }

            JobsExecuting = false;

            return jobsExecuted;
        }

        private IEnumerable<PlcJob> SortJobs(IEnumerable<PlcJob> jobs)
        {
            List<PlcJob> result = new List<PlcJob>();

            var jobsByColor = from job in jobs
                              orderby job.EndPosX
                              group job by job.IsBlack into colorGroup
                              select colorGroup;
            foreach (var colorGroup in jobsByColor)
            {

                var colorJobs = colorGroup.ToList();
                PlcJob currentJob = colorJobs.FirstOrDefault();

                int jobCount = 1;

                while (colorJobs.Count > 0)
                {
                    if (!result.Contains(currentJob))
                        result.Add(currentJob);

                    colorJobs.Remove(currentJob);

                    double mindist = double.MaxValue;
                    PlcJob? nextJob = null;
                    foreach (PlcJob job in colorJobs)
                    {
                        double dist = GetDistanceBetween(new Point(currentJob.EndPosX, currentJob.EndPosY), new Point(job.EndPosX, job.EndPosY));

                        if (dist < mindist)
                        {
                            nextJob = job;
                            mindist = dist;
                        }
                    }

                    if (!nextJob.HasValue)
                        continue;
                    else
                    {
                        currentJob = nextJob.Value;
                        jobCount++;

                        if (jobCount > 4)
                        {
                            // nach 4 Steinen neu sortieren
                            colorJobs = colorJobs.OrderBy(x => x.StartPosX).ToList();
                            currentJob = colorJobs.FirstOrDefault();
                            jobCount = 1;
                        }
                    }
                }
            }
            return result;

        }

        public static double GetDistanceBetween(Point start, Point end)
        {
            var distanceVector = Point.Subtract(start, end);
            double distance = distanceVector.Length;
            return distance;
        }

        public bool SetLight(bool bActivateLight)
        {
            return TryWrite("HMI.bLightOn", bActivateLight);
        }

        #endregion

    }
}
