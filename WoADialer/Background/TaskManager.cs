using Internal.Windows.Calls;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls.Background;
using Windows.UI.Notifications;
using WoADialer.Model;

namespace WoADialer.Background
{
    public static class TaskManager
    {
        public const string LINE_STATE_CHANGED = "LineStateChanged";

        public static async Task RegisterBackgroudTasks()
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            //I know that stupid
            if (BackgroundTaskRegistration.AllTasks.Count != 1)
            {
                foreach (var task3 in BackgroundTaskRegistration.AllTasks)
                {
                    task3.Value.Unregister(true);
                }

                var builder = new BackgroundTaskBuilder();

                builder.Name = LINE_STATE_CHANGED;
                builder.SetTrigger(new PhoneTrigger(PhoneTriggerType.LineChanged, false));
                BackgroundTaskRegistration task = builder.Register();
            }
        }

        public static void ShowToast(string msg)
        {
            ToastVisual toast = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = "Notification Test"
                        },

                        new AdaptiveText()
                        {
                            Text = msg
                        }
                    }
                }
            };
            ToastContent toastContent = new ToastContent()
            {
                Visual = toast,
                Scenario = ToastScenario.Default
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml());
            MainEntities.ToastNotifier.Show(notification);
        }

        public static ToastNotification ShowCallToast(Call call)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
            {
                new AdaptiveText()
                {
                    Text = "Incoming Call"
                },
                new AdaptiveText()
                {
                    Text = new BindableString($"{call.Number}")
                }
            }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
        {
            new ToastButton("Text reply", $"action=textReply&callId={call.ID}")
            {
                ActivationType = ToastActivationType.Foreground
            },
            new ToastButton("Reminder", $"action=reminder&callId={call.ID}")
            {
                ActivationType = ToastActivationType.Background
            },
            new ToastButton("Ignore", $"action=ignore&callId={call.ID}")
            {
                ActivationType = ToastActivationType.Background
            },
            new ToastButton("Answer", $"action=answer&callId={call.ID}")
            {
                ActivationType = ToastActivationType.Foreground
            }
        }
                },
                Launch = $"action=answer&callId={call.ID}",
                Scenario = ToastScenario.IncomingCall
            };

            ToastNotification notification = new ToastNotification(toastContent.GetXml());
            MainEntities.ToastNotifier.Show(notification);
            return notification;
        }
    }
}
