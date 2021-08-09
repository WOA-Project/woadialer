using Dialer.Helpers;
using Dialer.UI.Converters;
using Internal.Windows.Calls;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace Dialer.Systems
{
    public sealed class NotificationSystem
    {
        private const string WINDOWS_SYSTEM_TOAST_CALLING = "Windows.SystemToast.Calling";
        private const string CALL_NOTIFICATION_UI = "CallNotificationUI";
        private static readonly IReadOnlyDictionary<string, byte> ACTION_PRIORITY = new Dictionary<string, byte>
        {
            { ANSWER, 0 },
            { HOLD_AND_ANSWER, 0 },
            { END_AND_ANSWER, 1 },
            { END, 1 },
            { HOLD, 2 },
            { UNHOLD, 2 },
            { REJECT, 2 },
            { SWAP, 2 },
            { TEXT_REPLY, 3 },
            { COMBINE, 3 },
            { PRIVATE, 3 },
            { IGNORE, 4 }
        };
        private ToastNotificationHistory ToastNotificationHistory = ToastNotificationManager.History;

        public const string ACTION = "Action";
        public const string USED_CALLS = "UsedCalls";
        public const string USED_CALLS_STATES = "UsedCallsStates";

        #region Available actions
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        public const string ANSWER = "Answer";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        public const string END = "End";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        public const string PRIVATE = "Private";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        public const string HOLD = "Hold";
        /// <summary>
        /// Requires <see cref="HOLD_CALL_ID"/>
        /// </summary>
        public const string UNHOLD = "Unhold";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        public const string REJECT = "Reject";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        public const string IGNORE = "Ignore";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/> and <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        public const string HOLD_AND_ANSWER = "Hold_Answer";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/> and <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        public const string END_AND_ANSWER = "End_Answer";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        public const string TEXT_REPLY = "TextReply";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/> and <see cref="HOLD_CALL_ID"/>
        /// </summary>
        public const string SWAP = "Swap";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/> and <see cref="HOLD_CALL_ID"/>
        /// </summary>
        public const string COMBINE = "Combine";
        public const string SHOW_CALL_UI = "ShowCallUI";
        public const string SHOW_INCOMING_CALL_UI = "ShowIncomingCallUI";
        #endregion

        #region Possible parameters
        public const string ACTIVE_CALL_ID = "ActiveCallID";
        public const string INCOMING_CALL_ID = "IncomingCallID";
        public const string HOLD_CALL_ID = "HoldCallID";
        #endregion

        public UserNotificationListener NotificationListener { get; private set; }
        public TileUpdater TileUpdater { get; private set; }
        public ToastNotificationManagerForUser ToastNotificationManagerForUser { get; private set; }
        public ToastNotifier ToastNotifier { get; private set; }
        public ToastCollectionManager ToastCollectionManager { get; private set; }

        public NotificationSystem() { }

        private List<ToastButton> CreateButtonsForCall(Call call)
        {
            List<ToastButton> buttons = new List<ToastButton>();
            switch (call.State)
            {
                case CallState.ActiveTalking:
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_Hold\\Text"), $"{ACTION}={HOLD}&{ACTIVE_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_End\\Text"), $"{ACTION}={END}&{ACTIVE_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    break;
                case CallState.Dialing:
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_End\\Text"), $"{ACTION}={END}&{ACTIVE_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    break;
                case CallState.Incoming:
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_TextReply\\Text"), $"{ACTION}={TEXT_REPLY}&{INCOMING_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Foreground });
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_Reject\\Text"), $"{ACTION}={REJECT}&{INCOMING_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_Answer\\Text"), $"{ACTION}={ANSWER}&{INCOMING_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Foreground });
                    break;
                case CallState.OnHold:
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_Unhold\\Text"), $"{ACTION}={UNHOLD}&{ACTIVE_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    buttons.Add(new ToastButton(App.Current.ResourceLoader.GetString("Button_End\\Text"), $"{ACTION}={END}&{ACTIVE_CALL_ID}={call.ID}") { ActivationType = ToastActivationType.Background });
                    break;
            }
            return buttons;
        }

        private IEnumerable<ToastButton> MergeButtons(IEnumerable<ToastButton> actions)
        {
            List<ToastButton> result = new List<ToastButton>(5);
            Dictionary<ToastButton, WwwFormUrlDecoder> queries = actions.ToDictionary(x => x, x => new WwwFormUrlDecoder(x.Arguments), ToastButtonEqualityComparer.EqualityComparer);
            List<ToastButton> _actions = actions.OrderBy(x => ACTION_PRIORITY[queries[x].GetFirstValueByName(ACTION)]).ToList();
            ToastButton answer = _actions.FirstOrDefault(x => queries[x].GetFirstValueByName(ACTION) == ANSWER);
            ToastButton hold = _actions.FirstOrDefault(x => queries[x].GetFirstValueByName(ACTION) == HOLD);
            ToastButton end = _actions.FirstOrDefault(x => queries[x].GetFirstValueByName(ACTION) == END);
            if (answer != null)
            {
                if (hold != null || end != null)
                {
                    _actions.Remove(answer);
                }
                if (end != null)
                {
                    _actions.RemoveAll(x => queries[x].GetFirstValueByName(ACTION) == END);
                    end = new ToastButton(App.Current.ResourceLoader.GetString("Button_End_And_Answer\\Text"), $"{ACTION}={END_AND_ANSWER}&{ACTIVE_CALL_ID}={queries[end].GetFirstValueByName(ACTIVE_CALL_ID)}&{INCOMING_CALL_ID}={queries[answer].GetFirstValueByName(INCOMING_CALL_ID)}") { ActivationType = ToastActivationType.Foreground };
                    queries.Add(end, new WwwFormUrlDecoder(end.Arguments));
                    _actions.Insert(0, end);
                }
                if (hold != null)
                {
                    _actions.RemoveAll(x => queries[x].GetFirstValueByName(ACTION) == HOLD);
                    hold = new ToastButton(App.Current.ResourceLoader.GetString("Button_Hold_And_Answer\\Text"), $"{ACTION}={HOLD_AND_ANSWER}&{ACTIVE_CALL_ID}={queries[hold].GetFirstValueByName(ACTIVE_CALL_ID)}&{INCOMING_CALL_ID}={queries[answer].GetFirstValueByName(INCOMING_CALL_ID)}") { ActivationType = ToastActivationType.Foreground };
                    queries.Add(hold, new WwwFormUrlDecoder(hold.Arguments));
                    _actions.Insert(0, hold);
                }
            }
            for (int i0 = 0; i0 < 5 && i0 < _actions.Count; i0++)
            {
                result.Add(_actions[i0]);
            }
            return result.OrderByDescending(x => ACTION_PRIORITY[queries[x].GetFirstValueByName(ACTION)]);
        }

        private List<object> CreateVisualForCall(Call call)
        {
            StringBuilder description = new StringBuilder();
            description.Append(CallToCallStateTextString.Convert(call));
            if (call.Phone != null)
            {
                description.Append(" - ");
                description.Append(call.Phone.Kind);
            }
            if (call.Line != null)
            {
                description.Append(" - ");
                description.Append(string.IsNullOrEmpty(call.Line.DisplayName) ? call.Line.NetworkName : call.Line.DisplayName);
            }
            return new List<object>()
            {
                new AdaptiveText() { Text = call.Contact?.DisplayName ?? "Dialer" },
                new AdaptiveText() { Text = description.ToString() },
                new AdaptiveImage()
                {
                    HintCrop = AdaptiveImageCrop.Circle,
                    HintAlign = AdaptiveImageAlign.Center,
                    Source = "https://unsplash.it/100?image=1000",
                }
            };
        }

        private void NotificationListener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            switch (args.ChangeKind)
            {
                case UserNotificationChangedKind.Added:
                    UserNotification notification = sender.GetNotification(args.UserNotificationId);
                    try
                    {
                        if (notification != null && notification.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING)
                        {
                            sender.RemoveNotification(notification.Id);
                        }
                    }
                    catch
                    {

                    }
                    break;
            }
        }

        public async void Initializate()
        {
            NotificationListener = UserNotificationListener.Current;
            if (await NotificationListener.RequestAccessAsync() == UserNotificationListenerAccessStatus.Allowed)
            {
                try
                {
                    // Crashes windows notification platform
                    //NotificationListener.NotificationChanged += NotificationListener_NotificationChanged;
                }
                catch (Exception) { Debug.WriteLine("NotificationListener.NotificationChanged.EventAdd failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }
            }

            try
            {
                TileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            }
            catch (Exception) { Debug.WriteLine("TileUpdateManager.CreateTileUpdaterForApplication failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }

            try
            {
                ToastNotificationManagerForUser = ToastNotificationManager.GetDefault();
                try
                {
                    ToastCollectionManager = ToastNotificationManagerForUser.GetToastCollectionManager();
                }
                catch (Exception) { Debug.WriteLine("ToastNotificationManagerForUser.GetToastCollectionManager failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }
            }
            catch (Exception) { Debug.WriteLine("ToastNotificationManager.GetDefault failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }

            try
            {
                ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            }
            catch (Exception) { Debug.WriteLine("ToastNotificationManager.CreateToastNotifier failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }
        }

        private void RemoveCallToastNotifications(IEnumerable<ToastNotification> notifications)
        {
            foreach (ToastNotification notification in notifications)
            {
                switch (notification.Tag)
                {
                    case CALL_NOTIFICATION_UI:
                        ToastNotificationManager.History.Remove(notification.Tag);
                        break;
                }
            }
        }

        public ToastNotification CreateMissedCallToastNotification(Call call)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = $"{App.Current.ResourceLoader.GetString("Notification\\MissedCall\\Headline")} - {call.Contact?.DisplayName}" },
                            new AdaptiveText() { Text = call.Phone?.Number }
                        }
                    }
                },
                Scenario = ToastScenario.Default
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml());
            notification.Group = App.Current.ResourceLoader.GetString("Notification\\MissedCall\\GroupHeadline");
            return notification;
        }

        public ToastNotification CreateCallNotification(IEnumerable<Call> currentCalls)
        {
            List<Call> calls = currentCalls.Where(x => x.State != CallState.Disconnected && x.State != CallState.Indeterminate).OrderBy(x => x.State).ToList();
            ToastBindingGeneric content = new ToastBindingGeneric();
            ToastActionsCustom actions = new ToastActionsCustom();
            switch (calls.Count)
            {
                case 0:
                    return null;
                case 1:
                    Call singleCall = calls.First();
                    foreach (IToastBindingGenericChild child in CreateVisualForCall(singleCall))
                    {
                        content.Children.Add(child);
                    }
                    foreach (IToastButton button in CreateButtonsForCall(singleCall))
                    {
                        actions.Buttons.Add(button);
                    }
                    break;
                default:
                    content.Children.Add(new AdaptiveText() { Text = "Dialer - Active calls" });
                    for (int i0 = 0; i0 < calls.Count / 2; i0++)
                    {
                        AdaptiveGroup group = new AdaptiveGroup();
                        for (int i1 = i0 * 2; i1 < i0 * 2 + 2 && i1 < calls.Count; i1++)
                        {
                            AdaptiveSubgroup subgroup = new AdaptiveSubgroup();
                            foreach (IAdaptiveSubgroupChild child in CreateVisualForCall(calls[i1]))
                            {
                                subgroup.Children.Add(child);
                            }
                            group.Children.Add(subgroup);
                        }
                        content.Children.Add(group);
                    }
                    foreach (IToastButton button in MergeButtons(calls.Select(x => CreateButtonsForCall(x)).SelectMany(x => x)))
                    {
                        actions.Buttons.Add(button);
                    }
                    break;
            }
            Call incomingCall = calls.FirstOrDefault(x => x.State == CallState.Incoming);
            bool hasRingTone = !string.IsNullOrEmpty(incomingCall?.Contact?.RingToneToken);
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = content
                },
                Actions = actions,
                Audio = new ToastAudio()
                {
                    Silent = calls.Any(x => x.State != CallState.Incoming),
                    Loop = true,
                    Src = hasRingTone ? new Uri(incomingCall.Contact.RingToneToken) : null
                },
                Launch = $"{ACTION}={SHOW_CALL_UI}",
                Scenario = ToastScenario.IncomingCall
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = CALL_NOTIFICATION_UI,
                ExpiresOnReboot = true,
                Priority = ToastNotificationPriority.High,
                Data = new NotificationData()
                {
                    Values =
                    {
                        { USED_CALLS, calls.Aggregate(new StringBuilder(), (x, y) => x.Append(y.ID).Append(';'), x => x.ToString()) },
                        { USED_CALLS_STATES, calls.Aggregate(new StringBuilder(), (x, y) => x.Append((uint)y.State).Append(';'), x => x.ToString()) }
                    }
                }
            };
            return notification;
        }

        public async void RemoveSystemToastNotificationIfExist()
        {
            IReadOnlyList<UserNotification> notifications = await NotificationListener.GetNotificationsAsync(NotificationKinds.Toast);
            UserNotification notification = notifications.FirstOrDefault(x =>
            {
                try
                {
                    return x.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING;
                }
                //That may happens when notifications come from non-UWP app, like MobileShell
                catch
                {
                    return false;
                }
            });
            if (notification != null)
            {
                NotificationListener.RemoveNotification(notification.Id);
            }
        }

        public void RemoveCallToastNotifications()
        {
            try
            {
                RemoveCallToastNotifications(ToastNotificationHistory.GetHistory());
            }
            catch (Exception) { Debug.WriteLine("RemoveCallToastNotifications failed.\nWarning: Windows Notification Platform might be unavailable, have you crashed something?"); }
        }

        public void RefreshCallNotification(IEnumerable<Call> currentCalls)
        {
            IReadOnlyList<ToastNotification> notifications = ToastNotificationHistory.GetHistory();
            bool badState = notifications.Count == 0 || notifications.Any(x =>
            {
                if (x.Data != null)
                {
                    List<uint> ids = x.Data.Values[USED_CALLS].Split(';').Where(y => !string.IsNullOrEmpty(y)).Select(y => uint.Parse(y)).ToList();
                    List<CallState> states = x.Data.Values[USED_CALLS_STATES].Split(';').Where(y => !string.IsNullOrEmpty(y)).Select(y => (CallState)Enum.Parse(typeof(CallState), y)).ToList();
                    List<Tuple<uint, CallState>> prev = ids.Join(states, y => ids.IndexOf(y), y => states.IndexOf(y), (x, y) => new Tuple<uint, CallState>(x, y)).ToList();
                    return !prev.All(y => currentCalls.Any(z => z.ID == y.Item1 && z.State == y.Item2));
                }
                else
                {
                    return false;
                }
            });
            if (badState)
            {
                RemoveCallToastNotifications(notifications);
                ToastNotification notification = CreateCallNotification(currentCalls);
                if (notification != null)
                {
                    ToastNotifier.Show(notification);
                }
            }
        }
    }
}
