#nullable enable

using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace WoADialer.UI.ViewModel
{
    public sealed class CallViewModel : ViewModelCore
    {
        private Timer? Timer;

        public Call Call { get; }
        public CallState State => Call.State;
        public CallStateReason StateReason => Call.StateReason;
        public TimeSpan? Length => (Call.EndTime ?? DateTimeOffset.Now) - Call.StartTime;

        public CallViewModel(CoreDispatcher dispatcher, Call call) : base(dispatcher)
        {
            Call = call;
            Call.StateChanged += Call_StateChanged;
            Call.StartTimeChanged += Call_StartTimeChanged;
            Call.EndTimeChanged += Call_EndTimeChanged;
            if (Call.StartTime != null && Call.EndTime == null)
            {
                InitializateTimer();
            }
        }

        private void Call_EndTimeChanged(Call sender, CallTimeChangedEventArgs args)
        {
            Timer?.Dispose();
            OnPropertyChanged(nameof(Length));
        }

        private void Call_StartTimeChanged(Call sender, CallTimeChangedEventArgs args)
        {
            if (Call.StartTime != null && Call.EndTime == null)
            {
                InitializateTimer();
            }
            OnPropertyChanged(nameof(Length));
        }

        private void Call_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StateReason));
        }

        private void InitializateTimer()
        {
            Timer = new Timer(Timer_Callback, null, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 1));
        }

        private void Timer_Callback(object state)
        {
            OnPropertyChanged(nameof(Length));
        }
    }
}
