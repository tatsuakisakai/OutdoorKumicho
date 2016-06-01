using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

// To add offline sync support, add the NuGet package Microsoft.WindowsAzure.MobileServices.SQLiteStore
// to your project. Then, uncomment the lines marked // offline sync
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898

namespace outdoorkumicho
{
    public sealed partial class MainPage : Page
    {
        #region MemberProperties
        private MobileServiceUser user;
        private bool IsLogin = false;
        private MobileServiceCollection<KumichoActivity, KumichoActivity> activities;
        private MobileServiceCollection<ActivityAttendees, ActivityAttendees> attendeeslist;
        private IMobileServiceTable<KumichoActivity> activityTable = App.MobileService.GetTable<KumichoActivity>();
        private IMobileServiceTable<ActivityAttendees> attendees = App.MobileService.GetTable<ActivityAttendees>();
        private bool IsShowAttendList = false;
        private App myapp;
        #endregion
        public MainPage()
        {
            this.InitializeComponent();
            myapp = (App)App.Current;
        }
        #region EventHandlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
             Reload_Click(this, null);
        }
        private async void ActivitySelectChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateActivitySelection(e);
        }
      
        private void OntappedDescriptionLabel(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DescriptionFrame.Visibility = Visibility.Collapsed;

        }

        private async void Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload.IsEnabled = false;
            await RefreshActivities();
            Reload.IsEnabled = true;

        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (await AuthenticateAsync())
            {
                Login.Visibility = Visibility.Collapsed;
                AttendEvent.IsEnabled = true;
                AttendEvent.Visibility = Visibility.Visible;
                await RefreshActivities();
                IsLogin = true;
            }
        }

        private void Regist_Click(object sender, RoutedEventArgs e)
        {
            DescriptionFrame.Visibility = Visibility.Collapsed;
            RegisterForm.Visibility = Visibility.Visible;
        }
        private async void ConfirmRegister(object sender, RoutedEventArgs e)
        {
            await RegistAttendee();
        }

        private void OnCancelRegist(object sender, RoutedEventArgs e)
        {
            ClearRegisterForm();
        }

        private async void OnAttendChecked(object sender, RoutedEventArgs e)
        {
            await UpdateAttend(sender);
        }
        private async void OnCancelEvent(object sender, RoutedEventArgs e)
        {
            await EventCancel();
        }

        private async void OnCommitEvent(object sender, RoutedEventArgs e)
        {
            await EventCommit();
        }

        private async void ActiveEvent_Click(object sender, RoutedEventArgs e)
        {
            IsShowAttendList = false;
            await RefreshActivities();
            Cancel.Visibility = Visibility.Collapsed;
            Regist.Visibility = Visibility.Visible;
            ActiveEvent.FontWeight = FontWeights.ExtraBold;
            AttendEvent.FontWeight = FontWeights.Normal;

        }

        private async void AttendEvent_Click(object sender, RoutedEventArgs e)
        {

            IsShowAttendList = true;
            await RefreshActivities();
            Cancel.Visibility = Visibility.Visible;
            Regist.Visibility = Visibility.Collapsed;
            ActiveEvent.FontWeight = FontWeights.Normal;
            AttendEvent.FontWeight = FontWeights.ExtraBold;

        }
        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await CancelRegistration();
        }
        #endregion

        #region Tasks
        private async Task RefreshActivities()
        {
            if(IsShowAttendList)
            {
               await  LoadRegistedActivity(user.UserId);
            }
            else
            { 
                MobileServiceInvalidOperationException exception = null;
                try
                {
                    activities = await activityTable
                            .Where(activity => activity.IsCanceled == false)
                            .OrderBy(c => c.Schedule)
                            .ToCollectionAsync();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    await new MessageDialog(exception.Message,
                        "Error loading items").ShowAsync();
                }
                else
                {
                   ActivityList.ItemsSource = activities;
                }
            }
        }
        private async Task<bool> AuthenticateAsync()
        {
            bool success = false;
            try
            {
                user = await App.MobileService
                    .LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                success = true;
            }
            catch (InvalidOperationException)
            {
            }

            IsLogin = true;
            return success;
        }

        private async Task RegistAttendee()
        {
            string message = "参加を申込みますか?";
            var confirmdialog = new MessageDialog(message);
            confirmdialog.Commands.Add(new UICommand("Yes"));
            confirmdialog.Commands.Add(new UICommand("No"));
            var result = await confirmdialog.ShowAsync();
            if (result.Label == "Yes")
            {
                try
                {
                    KumichoActivity selitem 
                        = (KumichoActivity)ActivityList.SelectedItem;
                    attendeeslist = await attendees
                        .Where(attendee => 
                        attendee.EventID == selitem.EventID 
                        && attendee.TwitterID == user.UserId)
                        .ToCollectionAsync();
                    if (attendeeslist.Count == 0)
                    {
                        ActivityAttendees newregister = new ActivityAttendees
                        {
                            EventID = selitem.EventID,
                            TwitterID = user.UserId,
                            FirstName = FirstName.Text,
                            FamilyName = FamilyName.Text,
                            IsCanceled = false,
                            IsAttended = false
                        };
                        await attendees.InsertAsync(newregister);
                        selitem.ActualAttendees++;
                        await activityTable.UpdateAsync(selitem);

                        message 
                            = string.Format("【{0}】への参加申込を受付けました。",
                            selitem.Title);
                        //参加申込済リストを読み込み通知のタグを更新
                        await LoadRegistedActivity(user.UserId);
                        ClearRegisterForm();
                    }
                    else
                    {

                        message = "このイベントは参加申込済またはキャンセル済です。";
                        ClearRegisterForm();
                    }

                }
                catch
                {
                    message = "申込時にエラーが発生しました。時間をおいて再度申込してください。";
                }
                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            }
        }

        private async Task CancelRegistration()
        {
            var dialog = new MessageDialog("参加を取り消しますか？");
            dialog.Commands.Add(new UICommand("OK"));
            dialog.Commands.Add(new UICommand("Cancel"));
            var result = await dialog.ShowAsync();
            string message;
            if (result.Label == "OK")
            {
                try
                {
                    KumichoActivity selitem = ActivityList.SelectedItem as KumichoActivity;

                    attendeeslist = await attendees
                        .Where(attendee => attendee.EventID == selitem.EventID && attendee.TwitterID == user.UserId)
                        .ToCollectionAsync();
                    if (attendeeslist.Count != 0)
                    {
                        ActivityAttendees cancelregistration = attendeeslist[0];
                        cancelregistration.IsCanceled = true;
                        await attendees.UpdateAsync(cancelregistration);
                        selitem.ActualAttendees--;
                        await activityTable.UpdateAsync(selitem);
                        message = "参加を取り消しました。";
                    }
                    else
                    {
                        message = "キャンセルできませんでした。";
                    }
                }
                catch (Exception ex)
                {
                    message = "キャンセル時に問題が発生しました。時間をおいて再度実行してください。";
                }

                dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();

            }
            await RefreshActivities();
        }

        private async Task UpdateActivitySelection(SelectionChangedEventArgs e)
        {
            if (IsShowAttendList)
            {
                if(ActivityList.SelectedIndex!= -1 && IsLogin)
                {
                    Cancel.IsEnabled = true;
                }
                else if(IsLogin)
                {
                    Cancel.IsEnabled = false;
                }

            }
            else
            {
                await ShowActivities(e);
            }
        }

        private async Task ShowActivities(SelectionChangedEventArgs e)
        {
            try
            {
                KumichoActivity selitem = (KumichoActivity)e.AddedItems[0];
                SelectedDescription.Text = selitem.Description;
                if (ActivityList.SelectedIndex != -1)
                {
                    if (IsLogin && (selitem.ActualAttendees < selitem.MaxAttendees))
                    {
                        Regist.IsEnabled = true;
                    }
                    if (selitem.ActualAttendees >= selitem.MaxAttendees)
                    {
                        string basetext = SelectedDescription.Text;
                        SelectedDescription.Text = "【満員につき締切】 " + basetext;
                    }
                    DescriptionFrame.Visibility = Visibility.Visible;
                }
                else
                {

                    DescriptionFrame.Visibility = Visibility.Collapsed;
                    if (IsLogin)
                    {
                        Regist.IsEnabled = false;
                        CommitEvent.IsEnabled = false;
                        CancelEvent.IsEnabled = false;
                    }
                }

                if (ActivityList.SelectedIndex == -1 || IsLogin == false) return;

                CommitEvent.IsEnabled = (!(selitem.IsComitted))
                    && (selitem.ActualAttendees >= selitem.MinAttendees);
                CancelEvent.IsEnabled = true;

                await LoadAttendeeList(selitem);

            }
            catch (Exception)
            {

            }
        }

        private async Task LoadAttendeeList(KumichoActivity selitem)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {

                attendeeslist = await attendees
                    .Where(attendee => attendee.EventID == selitem.EventID)
                    .ToCollectionAsync();

            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message,
                    "Error loading items").ShowAsync();
            }
            else
            {
                AttendeeList.ItemsSource = attendeeslist;
            }

        }

        private async Task UpdateAttend(object sender)
        {
            CheckBox cb = (CheckBox)sender;
            ActivityAttendees attend = cb.DataContext as ActivityAttendees;
            await attendees.UpdateAsync(attend);
            cb.IsEnabled = false;
        }

        private async Task EventCommit()
        {
            try
            {
                var Confirmdialog = new MessageDialog("イベント開催を通知しますか？");
                Confirmdialog.Commands.Add(new UICommand("OK"));
                Confirmdialog.Commands.Add(new UICommand("Cancel"));
                var result = await Confirmdialog.ShowAsync();
                if (result.Label == "OK")
                {
                    KumichoActivity selitem 
                        = (KumichoActivity)ActivityList.SelectedItem;
                    Dictionary<string, string> methodparams 
                        = new Dictionary<string, string>
                        { { "id", selitem.EventID } };
                    await App.MobileService.InvokeApiAsync("ConfirmEvent",
                        HttpMethod.Get, methodparams);
                    var dialog = new MessageDialog("イベント開催を通知しました。");
                    dialog.Commands.Add(new UICommand("OK"));
                    await dialog.ShowAsync();
                }

            }
            catch (Exception) { }

        }

        private async Task EventCancel()
        {
            try
            {
                var Confirmdialog = new MessageDialog("イベントを中止しますか？");
                Confirmdialog.Commands.Add(new UICommand("OK"));
                Confirmdialog.Commands.Add(new UICommand("Cancel"));
                var result = await Confirmdialog.ShowAsync();
                if (result.Label == "OK")
                {
                    KumichoActivity selitem 
                        = (KumichoActivity)ActivityList.SelectedItem;
                    Dictionary<string, string> methodparams 
                        = new Dictionary<string, string>
                        { { "id", selitem.EventID } };
                    await App.MobileService.InvokeApiAsync("CancelEvent",
                        HttpMethod.Get, methodparams);
                    var dialog = new MessageDialog("イベント中止を通知しました。");
                    dialog.Commands.Add(new UICommand("OK"));
                    await dialog.ShowAsync();
                }
            }
            catch (Exception) { }

        }

        private async Task LoadRegistedActivity(string userid)
        {
            try
            {
                attendeeslist 
                    = await attendees.Where(c => c.TwitterID == userid)
                    .ToCollectionAsync();
                var myactivitieslist 
                    = await activityTable.Where(d => d.IsCanceled == false)
                    .ToCollectionAsync();
                var selectedActivities 
                    = myactivitieslist.Join(attendeeslist, c => c.EventID
                    , d => d.EventID, (c, d) =>
                new KumichoActivity
                {
                    Id = c.Id,
                    EventID = c.EventID,
                    Title = c.Title,
                    PictureURL = c.PictureURL,
                    Schedule = c.Schedule,
                    Description = c.Description,
                    Area = c.Area,
                    ActivityType = c.ActivityType,
                    ActivityLevel = c.ActivityLevel,
                    MaxAttendees = c.MaxAttendees,
                    MinAttendees = c.MinAttendees,
                    ActualAttendees = c.ActualAttendees,
                    IsCanceled = d.IsCanceled,
                    IsComitted = c.IsComitted
                }).Where(e => e.IsCanceled == false);
                myapp.NotificationTags.Clear();

                foreach (var activity in selectedActivities)
                {
                    myapp.NotificationTags.Add(activity.EventID);
                }
                await myapp.RegisterNotification();
                if(IsShowAttendList)
                    ActivityList.ItemsSource = selectedActivities;
            }
            catch
            {
            }
        }


        private void ClearRegisterForm()
        {
            FirstName.Text = "";
            FamilyName.Text = "";
            RegisterForm.Visibility = Visibility.Collapsed;
        }




        #endregion


    }
}
