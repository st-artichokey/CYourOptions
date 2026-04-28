#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CYourOptions.Library.Models;
using CYourOptions.Library.Services;
using CYourOptions.Library.Stories;

namespace CYourOptions.App.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly DecisionEngine _engine;
    private readonly SaveManager _saveManager;

    public GameViewModel()
    {
        _engine = new DecisionEngine(TheProductionIncident.GetNodes());
        _saveManager = new SaveManager();

        MakeChoiceCommand = new Command<int>(MakeChoice);
        GoBackCommand = new Command(GoBack, () => CanGoBack);
        RestartCommand = new Command(Restart);

        _engine.Start(TheProductionIncident.StartNodeId);
        UpdateProperties();
    }

    public string Title { get; private set; } = "";
    public string Text { get; private set; } = "";
    public bool IsEndNode { get; private set; }
    public bool CanGoBack { get; private set; }
    public bool ShowChoices => !IsEndNode;
    public ObservableCollection<ChoiceItem> Choices { get; } = [];

    public ICommand MakeChoiceCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand RestartCommand { get; }

    private void MakeChoice(int index)
    {
        _engine.MakeChoice(index);
        UpdateProperties();
    }

    private void GoBack()
    {
        _engine.GoBack();
        UpdateProperties();
    }

    private void Restart()
    {
        _engine.Start(TheProductionIncident.StartNodeId);
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        var node = _engine.CurrentNode!;

        Title = node.Title;
        Text = node.Text;
        IsEndNode = node.IsEndNode;
        CanGoBack = _engine.CanGoBack;

        Choices.Clear();
        for (int i = 0; i < node.Choices.Count; i++)
        {
            Choices.Add(new ChoiceItem(i, node.Choices[i].Label, MakeChoiceCommand));
        }

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(IsEndNode));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(ShowChoices));
        ((Command)GoBackCommand).ChangeCanExecute();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class ChoiceItem
{
    public int Index { get; }
    public string Label { get; }
    public ICommand SelectCommand { get; }

    public ChoiceItem(int index, string label, ICommand selectCommand)
    {
        Index = index;
        Label = label;
        SelectCommand = selectCommand;
    }
}
