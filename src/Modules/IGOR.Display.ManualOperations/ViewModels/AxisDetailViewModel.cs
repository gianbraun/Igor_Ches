using IGOR.Interface.Communication.Axis;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace IGOR.Display.ManualOperations.ViewModels
{
    public class AxisDetailViewModel : BindableBase, INavigationAware
    {
        public IAxis Axis { get; set; }

        public Color ActualPosColor { get; set; } = (Color)Application.Current.Resources["ForegroundDarkColor"];

        public double TargetVel { get; set; } = 0;

        public double TargetPos { get; set; }

        public bool CanMoveNeg { get; private set; }

        public bool CanMovePos { get; private set; }

        public DelegateCommand JogPosFastActiveCommand => new DelegateCommand(() => Axis.Move(0, Axis.Params.HandFast, PositioningType.EndlessPos), CheckCanMovePositive);
        public DelegateCommand JogPosActiveCommand => new DelegateCommand(() => Axis.Move(0, Axis.Params.HandSlow, PositioningType.EndlessPos), CheckCanMovePositive);
        public DelegateCommand JogNegFastActiveCommand => new DelegateCommand(() => Axis.Move(0, Axis.Params.HandFast, PositioningType.EndlessNeg), CheckCanMoveNegative);
        public DelegateCommand JogNegActiveCommand => new DelegateCommand(() => Axis.Move(0, Axis.Params.HandSlow, PositioningType.EndlessNeg), CheckCanMoveNegative);

        public DelegateCommand StartJobCommand => new DelegateCommand(() => Axis.Move(TargetPos, TargetVel, PositioningType.Absolute), CheckCanJobStart);


        public DelegateCommand AxisStopCommand => new DelegateCommand(() => Axis.Stop());



        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Axis.Pos.ContiniusRefresh = false;
            Axis.State.ContiniusRefresh = false;
            Axis.Release.ContiniusRefresh = false;
            Axis.Pos.PropertyChanged -= AxisPosPropertiesChanged;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.FirstOrDefault().Value is not IAxis axis)
                return;
            Axis = axis;
            Axis.Pos.PropertyChanged += AxisPosPropertiesChanged;
            Axis.Release.PropertyChanged += AxisReleasePropertyChanged;
            CheckCanMovePositive();
            CheckCanMoveNegative();
            Axis.Pos.ContiniusRefresh = true;
            Axis.State.ContiniusRefresh = true;
            Axis.Release.ContiniusRefresh = true;
        }

        private void AxisPosPropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Axis.Pos.ActualPos))
            {
                CheckCanMoveNegative();
                CheckCanMovePositive();
            }
        }

        private void AxisReleasePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckCanMoveNegative();
            CheckCanMovePositive();
            StartJobCommand.RaiseCanExecuteChanged();
        }

        private bool CheckCanMoveNegative()
        {
            var softLimit = !Axis.Params.SoftlimitNegAktive || Axis.Pos.ActualPos > Axis.Params.MinPos;
            var result = softLimit && Axis.Release.FeedBW && Axis.Release.ControllerRelease && Axis.Release.Override > 0;
            CanMoveNeg = result;
            return result;
        }

        private bool CheckCanMovePositive()
        {
            var softLimit = !Axis.Params.SoftlimitPosAktive || Axis.Pos.ActualPos < Axis.Params.MaxPos;
            var result = softLimit && Axis.Release.FeedFW && Axis.Release.ControllerRelease && Axis.Release.Override > 0;
            CanMovePos = result;
            return result;
        }

        private bool CheckCanJobStart()
        {
            var softLimitPos = !Axis.Params.SoftlimitPosAktive || TargetPos <= Axis.Params.MaxPos;
            var softLimitNeg = !Axis.Params.SoftlimitNegAktive || TargetPos >= Axis.Params.MinPos;
            bool feedFw = Axis.Release.FeedFW || TargetPos < Axis.Pos.ActualPos;
            bool feedBw = Axis.Release.FeedBW || TargetPos > Axis.Pos.ActualPos;
            var result = softLimitNeg && softLimitPos && Axis.Release.ControllerRelease && feedFw && feedBw;
            return result;
        }

        private void OnTargetPosChanged(object befor, object after)
        {
            StartJobCommand.RaiseCanExecuteChanged();
        }

    }
}
