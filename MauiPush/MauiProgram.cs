using Microsoft.Extensions.Logging;

using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Bundled.Shared;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using Application = Microsoft.Maui.Controls.Application;
#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
#else
using Plugin.Firebase.Bundled.Platforms.Android;
using Android.Content;

using Android.App;
using AndroidX.Core.App;
#endif


namespace MauiPush;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
             .RegisterFirebaseServices()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }



    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((app, launchOptions) =>
            {
                CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                return false;
            }));

            // events.AddiOS(iOS => iOS.WillFinishLaunching((app, launchOptions) => { CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings()); return false; }));
#else
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings())));
            //CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
#endif
        });
        CrossFirebaseCloudMessaging.Current.TokenChanged += Current_TokenChanged;
        CrossFirebaseCloudMessaging.Current.NotificationReceived += Current_NotificationReceived;
        CrossFirebaseCloudMessaging.Current.NotificationTapped += Current_NotificationTapped;

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(isAuthEnabled: true,
        isCloudMessagingEnabled: true, isAnalyticsEnabled: true);
    }

    private static async void Current_NotificationTapped(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationTappedEventArgs e)
    {

        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)//pendiente android hace crash
        { // await Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushAsync(new Notifications());
            //todo ir a notifiaciones
            await Application.Current.MainPage.DisplayAlert("Alert", "Notifiacion tocada.", "OK");
        }
        else
        {

            if (Preferences.Get(PrefKeys.IN_FORGROUND, "false") == "true")
            {
                try
                {
                    //todo await Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushAsync(new Notifications());
                    await Application.Current.MainPage.DisplayAlert("Alert", "Notifiacion tocada.", "OK");
                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                //color
                //  var sbs = new StatusBarServices();


                // var col = (Color)Microsoft.Maui.Controls.Application.Current.Resources["BottomBarBG"];
                //sbs.SetStatusBarColor(col);
                //TODO GUARDAR EN PREFERENCIAS Y AL LLEGAR A HOME NAVEGAR
                Preferences.Set(PrefKeys.PENDING_NOTIF, "true");

            }
        }

    }

    private static void Current_NotificationReceived(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationReceivedEventArgs e)
    {
        var notification = e.Notification;
        var data = e.Notification.Data;


#if ANDROID

        //  FirebaseCloudMessagingImplementation.ChannelId = MainActivity.Channel_ID;

        var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        var intent = new Intent(context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop);
        intent.AddFlags(ActivityFlags.SingleTop);


        foreach (var key in data.Keys)
        {
            string value = data[key];
            intent.PutExtra(key, value);
        }

        var pendingIntent = PendingIntent.GetActivity(context,
            MainActivity.NotificationID, intent, PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);

        var notificationBuilder = new NotificationCompat.Builder(context, MainActivity.Channel_ID)
            .SetContentTitle(notification.Title)
            .SetSmallIcon(Resource.Drawable.logo_evergo)
            .SetContentText(data.ToString())
            .SetChannelId(MainActivity.Channel_ID)
            .SetContentIntent(pendingIntent)
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.Max);

        var notificationManager = NotificationManagerCompat.From(context);
        notificationManager.Notify(MainActivity.NotificationID, notificationBuilder.Build());



        //     FirebaseCloudMessagingImplementation.NotificationBuilderProvider = MainActivity.CreateNotificationBuilder;
        //var notification = new NotificationCompat.Builder(this, "MyChannel")
        //  .SetContentTitle("Notification Title")
        //  .SetContentText("Notification Description")
        //  .SetSmallIcon(Resource.Drawable.appiconfg)
        //  .SetContentIntent(pendingIntent)
        //  .SetOngoing(true)
        //  .SetShowWhen(false)
        //  .Build();

        //StartForeground(10000, notification);
#endif

    }

    private static void Current_TokenChanged(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMTokenChangedEventArgs e)
    {
        var act = Preferences.Get(PrefKeys.DEVICE_TOKEN, string.Empty);
        var token = e.Token;
        Console.WriteLine($"FCM token: {e.Token}");
        if (token != act)
        {
            Preferences.Set(PrefKeys.DEVICE_TOKEN_OLD, act);
            Preferences.Set(PrefKeys.DEVICE_TOKEN, token);
            Preferences.Set(PrefKeys.TOKEND_SENDED, "false");
        }

    }





}

