using Avalonia.Controls;
using Avalonia.Interactivity;
using smartFactoryApp.ViewModels;

namespace smartFactoryApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel =>
        DataContext as MainViewModel;

    // ==========================================
    // MOVEMENT
    // ==========================================

    private void MoveLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveLeft();
    }

    private void MoveRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveRight();
    }

    private void MoveUp_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUp();
    }

    private void MoveDown_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDown();
    }

    private void MoveUpLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpLeft();
    }

    private void MoveUpRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpRight();
    }

    private void MoveDownLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownLeft();
    }

    private void MoveDownRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownRight();
    }

    // ==========================================
    // MOVE / STEP MODE
    // ==========================================

    private void MoveMode_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SetMoveMode();
    }

    private void StepMode_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SetStepMode();
    }

    private void StopRobot_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.StopRobot();
    }

    // ==========================================
    // ROTATION
    // ==========================================

    private void RotateLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RotateLeft();
    }

    private void RotateRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RotateRight();
    }

    // ==========================================
    // PRODUCTION LINES
    // ==========================================

    // Linien mit zufälligen Bricks füllen
    private void SetupProductionLines_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SetupProductionLines();
    }

    // Automatische Fahrt des Roboters
    private void GoToPickUp_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.GoToPickUp();
    }

    private void GoToDelivery_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.GoToDelivery();
    }

    private void StartTimer_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.StartTimer();
    }

    private void StopTimer_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.StopTimer();
    }

    private void UndockButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Undock();
    }
}