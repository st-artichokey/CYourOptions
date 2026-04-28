using CYourOptions.App.ViewModels;

namespace CYourOptions.App.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel();
    }
}
