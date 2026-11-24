using IGOR.Common;
using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Actor;
using IGOR.Interface.Communication.Axis;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Resources.Extensions;
using System.Text.Json;

namespace IGOR.Display.ManualOperations.ViewModels
{
    public class ManualOperationsViewModel : BindableBase
    {
        private readonly IPlcCom _plcCom;
        private readonly IRegionManager _regionManager;
        private readonly ILogger _log;

        public IReadOnlyCollection<IAxis> Axes => _plcCom.Axes;

        public IReadOnlyCollection<IActor> Actors => _plcCom.Actors;

        public IAxis SelectedAxis { get; set; }

        public IActor SelectedActor { get; set; }

        public ManualOperationsViewModel(IPlcCom plcCom, IRegionManager regionManager, ILogger logger)
        {
            _plcCom = plcCom;
            _regionManager = regionManager;
            _log = logger.ForContext<ManualOperationsViewModel>();
        }

        private void OnSelectedAxisChanged(object _, object after)
        {
            if (after == null)
                return;

            SelectedActor = null;

            var axis = (IAxis)after;
            var parameters = new NavigationParameters()
            {
                {"Axis", after }
            };
            _regionManager.RequestNavigate(RegionNames.DetailRegion, "AxisDetailView", parameters);
        }

        private void OnSelectedActorChanged(object _, object after)
        {
            if(after == null)
                return;

            SelectedAxis = null;
            var axis = (IActor)after;
            var parameters = new NavigationParameters()
            {
                {"Actor", after }
            };
            _regionManager.RequestNavigate(RegionNames.DetailRegion, "ActorsDetialView", parameters);
        }
    }
}
