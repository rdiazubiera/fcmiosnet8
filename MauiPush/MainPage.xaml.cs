using Plugin.Firebase.CloudMessaging;

namespace MauiPush;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        string token = null;
        try
        {
            token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
        catch (Exception ex)
        {
            token = ex.Message;
        }
        Console.WriteLine($"FCM token: {token}");
        await DisplayAlert("Token:", token, "Cancelar");

        await Clipboard.Default.SetTextAsync(token ?? "Texto pegado");
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}


