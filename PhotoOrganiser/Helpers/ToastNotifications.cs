using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace ForensicX.Helpers
{
    internal class ToastNotifications
    {
        public void SendUpdatableToastWithProgress(double progress, string sourceDevice)
        {
            // Define a tag (and optionally a group) to uniquely identify the notification, in order update the notification data later;
            string tag = "diskImaging";
            string group = "diskImagingGroup";

            // Construct the toast content with data bound fields
            var content = new ToastContentBuilder()
                .AddText($"Disk imaging")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = $"{sourceDevice}",
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .GetToastContent();

            // Generate the toast notification
            var toast = new ToastNotification(content.GetXml());

            // Assign the tag and group
            toast.Tag = tag;
            toast.Group = group;

            // Assign initial NotificationData values
            // Values must be of type string
            toast.Data = new NotificationData();
            toast.Data.Values["progressValue"] = (progress / 100.0).ToString("F2");
            toast.Data.Values["progressValueString"] = $"{progress}%";
            toast.Data.Values["progressStatus"] = "Imaging in progress...";

            // Provide sequence number to prevent out-of-order updates, or assign 0 to indicate "always update"
            toast.Data.SequenceNumber = 0;

            // Show the toast notification to the user
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void UpdateProgress(double progress)
        {
            // Construct a NotificationData object;
            string tag = "diskImaging";
            string group = "diskImagingGroup";

            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData
            {
                SequenceNumber = 0
            };

            // Assign new values
            // Note that you only need to assign values that changed. In this example
            // we don't assign progressStatus since we don't need to change it
            data.Values["progressValue"] = (progress / 100.0).ToString("F2");
            data.Values["progressValueString"] = $"{progress}%";

            // Update the existing notification's data by using tag/group
            ToastNotificationManager.CreateToastNotifier().Update(data, tag, group);
        }
    }
}
