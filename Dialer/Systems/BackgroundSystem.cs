using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls.Background;
using Windows.UI.Notifications;

namespace Dialer.Systems
{
    public sealed class BackgroundSystem
    {
        private const string CALL_BLOCKED = "CallBlocked";
        private const string CALL_HISTORY_CHANGED = "CallHistoryChanged";
        private const string CALL_ORIGIN_DATA_REQUEST = "CallOriginDataRequest";
        private const string LINE_STATE_CHANGED = "LineStateChanged";
        //private const string NEW_VOICEMAIL_MESSAGE = "NewVoicemailMessage";
        private const string TOAST_BACKGROUNG_ACTIVATED = "ToastBackgroungActivated";

        private bool Skip = false;

        public BackgroundSystem() { }

        private async Task ConfigureBackgroundTasks(bool force = false)
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            Dictionary<string, bool> taskRegistration = new()
            {
                { CALL_BLOCKED, false },
                { CALL_HISTORY_CHANGED, false },
                { CALL_ORIGIN_DATA_REQUEST, false },
                //{ NEW_VOICEMAIL_MESSAGE, false },
                { LINE_STATE_CHANGED, false },
                { TOAST_BACKGROUNG_ACTIVATED, false },
            };
            foreach (IBackgroundTaskRegistration registeredTask in BackgroundTaskRegistration.AllTasks.Select(x => x.Value))
            {
                switch (registeredTask.Name)
                {
                    case string taskName when taskRegistration.Keys.Contains(taskName):
                        if (force)
                        {
                            goto default;
                        }
                        else
                        {
                            taskRegistration[taskName] = true;
                        }
                        break;
                    default:
                        registeredTask.Unregister(false);
                        break;
                }
            }
            foreach (string taskName in taskRegistration.Keys)
            {
                if (!taskRegistration[taskName])
                {
                    BackgroundTaskBuilder taskBuilder = new();
                    taskBuilder.Name = taskName;
                    switch (taskName)
                    {
                        case CALL_BLOCKED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallBlocked, false));
                            break;
                        case CALL_HISTORY_CHANGED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallHistoryChanged, false));
                            break;
                        case CALL_ORIGIN_DATA_REQUEST:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallOriginDataRequest, false));
                            break;
                        /*case NEW_VOICEMAIL_MESSAGE:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.NewVoicemailMessage, false));
                            break;*/
                        case LINE_STATE_CHANGED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.LineChanged, false));
                            break;
                        case TOAST_BACKGROUNG_ACTIVATED:
                            taskBuilder.SetTrigger(new ToastNotificationActionTrigger());
                            break;
                        default:
                            throw new NotImplementedException($"Case for {taskName} task missed.");
                    }
                    taskBuilder.Register();
                }
            }
        }

        private void OnLateBackgroundActivation()
        {
            App.Current.NotificationSystem.RemoveSystemToastNotificationIfExist();
            App.Current.NotificationSystem.RefreshCallNotification(App.Current.CallSystem.CallManager.CurrentCalls);
        }

        public async Task Initialize()
        {
            await ConfigureBackgroundTasks();
        }

        public void OnBackgroundActivated(IBackgroundTaskInstance taskInstance)
        {
            switch (taskInstance.Task.Name)
            {
                case CALL_ORIGIN_DATA_REQUEST:
                    //PhoneCallOriginDataRequestTriggerDetails originDataRequest = args.TaskInstance.TriggerDetails as PhoneCallOriginDataRequestTriggerDetails;
                    //PhoneCallOrigin data = new PhoneCallOrigin();
                    //data.Category = "Category";
                    //data.CategoryDescription = "CategoryDescription";
                    //data.DisplayName = "DisplayName";
                    //data.Location = "Location";
                    //PhoneCallOriginManager.SetCallOrigin(originDataRequest.RequestId, data);
                    break;
                case LINE_STATE_CHANGED:
                    PhoneLineChangedTriggerDetails lineChangedDetails = taskInstance.TriggerDetails as PhoneLineChangedTriggerDetails;
                    if (!Skip && !App.Current.IsForeground)
                    {
                        Skip = true;
                        OnLateBackgroundActivation();
                    }
                    break;
                case TOAST_BACKGROUNG_ACTIVATED:
                    ToastNotificationActionTriggerDetail toastDetails = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    App.Current.OnToastNotificationActivated(ToastActivationType.Background, toastDetails.Argument);
                    break;
            }
        }
    }
}
