using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using smartFactoryApp.Models;

namespace smartFactoryApp.ViewModels;

public partial class LegoBrickViewModel : ViewModelBase
{
    private readonly LegoBrick brick;

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private double angle;

    // Zeichenfarbe des Bricks
    public IBrush FillBrush => brick.Color switch
    {
        Parameters.LegoColor.red    => Brushes.Firebrick,
        Parameters.LegoColor.yellow => Brushes.Gold,
        Parameters.LegoColor.blue   => Brushes.DodgerBlue,
        Parameters.LegoColor.green  => Brushes.Green,
        _                           => Brushes.Gray
    };

    // Auf blauer/grüner Linie stehen Bricks senkrecht (12 x 20),
    // auf roter/gelber Linie liegen sie waagerecht (20 x 12)
    private bool IstSenkrecht => brick.Senkrecht;

    public double BrickWidth  => IstSenkrecht ? 12 : 20;
    public double BrickHeight => IstSenkrecht ? 20 : 12;

    // Mitte des Bricks auf die Position legen
    public Thickness BrickMargin => new Thickness(-BrickWidth / 2, -BrickHeight / 2, 0, 0);

    public LegoBrickViewModel(LegoBrick brick)
    {
        this.brick = brick;
        Refresh();
    }

    public void Refresh()
    {
        X = brick.Location.X;
        Y = brick.Location.Y;
        Angle = brick.Angle;
    }
}