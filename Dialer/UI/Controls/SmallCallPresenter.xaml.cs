using Internal.Windows.Calls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;

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
            InitializeComponent();
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

        private void TimerCallback(object state)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, Bindings.Update);
        }
    }
}
