using smartFactoryApp.Models;

namespace smartFactoryApp.ViewModels;

public class ProductionLineViewModel : ViewModelBase
{
    private readonly ProductionLine productionLine;

    public double X => productionLine.Location.X;
    public double Y => productionLine.Location.Y;

    public double Width => ViewParameters.ProductionLineWidth;
    public double Height => ViewParameters.ProductionLineHeight;

    public Parameters.LegoColor Color => productionLine.Color;

    internal ProductionLineViewModel(ProductionLine productionLine)
    {
        this.productionLine = productionLine;
    }
}