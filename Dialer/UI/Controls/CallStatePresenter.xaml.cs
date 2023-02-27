using Internal.Windows.Calls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dialer.UI.Controls
{
    public sealed partial class CallStatePresenter : UserControl
    {
        public static readonly DependencyProperty PresentedCallProperty = DependencyProperty.RegisterAttached("PresentedCall", typeof(Call), typeof(CallStatePresenter), new PropertyMetadata(null));

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
                    }
                }
            }
        }

        public CallStatePresenter()
        {
            InitializeComponent();
        }

        private void PresentedCall_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, Bindings.Update);
        }
    }
}
