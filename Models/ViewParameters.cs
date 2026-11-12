using System.Drawing;

namespace smartFactoryApp.Models
{
    public static class ViewParameters
    {
        public static readonly Point RobotPosition =
            new Point(230, 250);

        public static readonly Point RedProductionLinePosition =
            new Point(390, 450);

        public static readonly Point YellowProductionLinePosition =
            new Point(60, 450);

        public static readonly Point BlueProductionLinePosition =
            new Point(100, 130);

        public static readonly Point GreenProductionLinePosition =
            new Point(420, 130);


        public const int ProductionLineWidth = 160;
        public const int ProductionLineHeight = 60;


        public static readonly Rectangle RedProductionLineRect =
            new Rectangle(RedProductionLinePosition,new Size(ProductionLineWidth, ProductionLineHeight )
            );


        public static readonly Rectangle YellowProductionLineRect =
            new Rectangle(
                YellowProductionLinePosition,
                new Size(
                    ProductionLineWidth,
                    ProductionLineHeight
                )
            );


        public static readonly Rectangle BlueProductionLineRect =
            new Rectangle(
                BlueProductionLinePosition,
                new Size(
                    ProductionLineHeight,
                    ProductionLineWidth
                )
            );


        public static readonly Rectangle GreenProductionLineRect =
            new Rectangle(
                GreenProductionLinePosition,
                new Size(
                    ProductionLineHeight,
                    ProductionLineWidth
                )
            );


        // Delivery Line
        public static readonly Rectangle FactoryZonesRect =
            new Rectangle(
                200,
                40,
                180,
                60
            );


        // Recycling / Multicolor Förderband
        public static readonly Rectangle MultiColorRect =
            new Rectangle(
                200,
                330,
                180,
                60
            );


        public static readonly Rectangle Borders =
            new Rectangle(
                37,
                37,
                490,
                460
            );


        public static readonly Rectangle BrickBorders =
            new Rectangle(
                38,
                38,
                525,
                525
            );


        public const int RobotWidth = 30;
        public const int RobotHeight = 66;
    }
}