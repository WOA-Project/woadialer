using Internal.Windows.Calls;
using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dialer.UI.Controls
{
    public sealed partial class SmallCallPresenter : UserControl
    {
        public static readonly DependencyProperty PresentedCallProperty = DependencyProperty.RegisterAttached("PresentedCall", typeof(Call), typeof(CallStatePresenter), new PropertyMetadata(null));

        private Timer Timer;

        public Call PresentedCall
        {
            get => (Call)GetValue(PresentedCallProperty);
            set
            {
                if (PresentedCall != value)
                {
                    if (PresentedCall != null)
                    {
                        PresentedCall.StateChanged -= PresentedCall_StateChanged;
                    }
                    SetValue(PresentedCallProperty, value);
                    if (value != null)
                    {
                        value.StateChanged += PresentedCall_StateChanged;
                        PresentedCall_StateChanged(value, new CallStateChangedEventArgs(value.State, value.State, value.StateReason));
                    }
                }
            }
        }

        public SmallCallPresenter()
        {
            this.InitializeComponent();
        }

        private void PresentedCall_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            switch (args.NewState)
            {
                case CallState.Disconnected:
                case CallState.Count:
                case CallState.Indeterminate:
                    Timer?.Dispose();
                    break;
                case CallState.ActiveTalking:
                    Timer = new Timer(TimerCallback, null, TimeSpan.Zero, new TimeSpan(0, 0, 1));
                    break;
            }
        }

        private async void TimerCallback(object state)
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => Bindings.Update());
        }
    }
}
