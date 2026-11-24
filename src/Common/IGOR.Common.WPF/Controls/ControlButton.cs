using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using IGOR.Common.WPF.Extensions;

namespace IGOR.Common.WPF.Controls
{
    public class ControlButton : ButtonBase, IDisposable
    {

        protected Ellipse PART_Ellipse;
        private CancellationTokenSource CancelAnimmationSource;
        protected SolidColorBrush DarkendColor;






        public ICommand PressedChangedCommand
        {
            get { return (ICommand)GetValue(PressedChangedCommandProperty); }
            set { SetValue(PressedChangedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressedChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedChangedCommandProperty =
            DependencyProperty.Register("PressedChangedCommand", typeof(ICommand), typeof(ControlButton), new PropertyMetadata(null));





        public SolidColorBrush ButtonColor
        {
            get { return (SolidColorBrush)GetValue(ButtonColorProperty); }
            set { SetValue(ButtonColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonColorProperty =
            DependencyProperty.Register("ButtonColor", typeof(SolidColorBrush), typeof(ControlButton), new PropertyMetadata(new SolidColorBrush(Colors.White), OnButtonColorChanged));


        public ControlButtonState State
        {
            get { return (ControlButtonState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ControlButtonState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ControlButtonState), typeof(ControlButton), new PropertyMetadata(ControlButtonState.Off, OnButtonStateChanged));


        public TimeSpan BlinkDuration
        {
            get { return (TimeSpan)GetValue(BlinkDurationProperty); }
            set { SetValue(BlinkDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlinkDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlinkDurationProperty =
            DependencyProperty.Register("BlinkDuration", typeof(TimeSpan), typeof(ControlButton), new PropertyMetadata(TimeSpan.FromSeconds(1)));


        private static void OnButtonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ControlButton;
            if (e.NewValue is not ControlButtonState state || button.PART_Ellipse is null)
                return;



            switch (state)
            {
                case ControlButtonState.Off:
                    button.StopBlinking();
                    button.PART_Ellipse.Fill = button.DarkendColor;
                    break;
                case ControlButtonState.On:
                    button.StopBlinking();
                    button.PART_Ellipse.Fill = button.ButtonColor;
                    break;
                case ControlButtonState.Blinking:
                    button.StartBlinking(button.BlinkDuration);
                    break;
                default:
                    break;
            }
        }

        private static void OnButtonColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ControlButton;
            if (e.NewValue is not SolidColorBrush brush)
                return;

            button.DarkendColor = new SolidColorBrush(brush.Color.ChangeColorBrightness(-0.8f));
        }

        static ControlButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlButton), new FrameworkPropertyMetadata(typeof(ControlButton)));
        }
        public ControlButton()
        {
        }
        public override void OnApplyTemplate()
        {
            PART_Ellipse = GetTemplateChild("PART_Ellipse") as Ellipse;
            PART_Ellipse.Fill = new SolidColorBrush(Colors.Gray);
            OnButtonStateChanged(this, new DependencyPropertyChangedEventArgs(StateProperty, null, State));
            base.OnApplyTemplate();
        }

        public void StartBlinking(TimeSpan duration)
        {
            CancelAnimmationSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                var flip = false;
                while (true)
                {


                    CancelAnimmationSource.Token.ThrowIfCancellationRequested();
                    Application.Current.Dispatcher.Invoke(() => PART_Ellipse.Fill = flip ? ButtonColor : DarkendColor);
                    Thread.Sleep(duration);
                    flip = !flip;

                }
            });
        }

        public void StopBlinking()
        {
            CancelAnimmationSource?.Cancel();
        }

        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            PressedChangedCommand.Execute(e.NewValue);
            base.OnIsPressedChanged(e);
        }

        public void Dispose()
        {
            StopBlinking();
        }
    }

    public enum ControlButtonState
    {
        Off,
        On,
        Blinking
    }
}
