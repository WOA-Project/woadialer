using System;
using Internal.Windows.Calls;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WoADialer.UI.Controls
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
            this.InitializeComponent();
        }

        private async void PresentedCall_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => Bindings.Update());
        }
    }
}
