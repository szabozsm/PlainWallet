using Maui.ColorPicker;

namespace PlainWallet.Views.Controls;

public partial class ColorPicker : ContentView
{
    public ColorPicker()
    {
        InitializeComponent();
        AddColorsToPalette();

        subColorPicker.BindingContext = this;
        subColorPicker.SetBinding(Maui.ColorPicker.ColorPicker.PickedColorProperty, new Binding(nameof(SelectedColor), source: this));
    }

    private IEnumerable<Color> GetPredefinedColors()
    {

        return [    // Grayscale
                    //Colors.Black,
                    //Colors.DarkGray,
        Colors.Gray,
    Colors.LightGray,
    //Colors.Silver,
    Colors.White,

    // Reds
    Colors.DarkRed,
    Colors.Red,
    Colors.IndianRed,
    Colors.Firebrick,
    Colors.LightCoral,

    // Oranges
    Colors.DarkOrange,
    Colors.Orange,
    Colors.Coral,
    Colors.Tomato,
    Colors.OrangeRed,

    // Yellows
    Colors.Goldenrod,
    Colors.Gold,
    Colors.Yellow,
    Colors.Khaki,

    // Greens
    Colors.DarkGreen,
    Colors.ForestGreen,
    Colors.Green,
    Colors.LimeGreen,
    Colors.Lime,
    Colors.SeaGreen,

    // Cyans / Teals
    Colors.Teal,
    Colors.Turquoise,
    Colors.Cyan,
    Colors.Aqua,

    // Blues
    Colors.Navy,
    Colors.DarkBlue,
    Colors.Blue,
    Colors.DodgerBlue,
    Colors.SkyBlue,
    Colors.SteelBlue,
    Colors.SlateBlue,

    // Purples / Pinks
    Colors.Indigo,
    Colors.Purple,
    Colors.Violet,
    Colors.Magenta,
    Colors.Pink,

    // Browns
    Colors.SaddleBrown,
    Colors.Brown,
    Colors.Chocolate];

        // return [  Colors.Black,
        //     Colors.White,
        //     Colors.Gray,
        //     Colors.DarkGray,
        //     Colors.LightGray,

        //     Colors.Red,
        //     Colors.Orange,
        //     Colors.Yellow,
        //     Colors.Gold,

        //     Colors.Green,
        //     Colors.Lime,
        //     Colors.Olive,
        //     Colors.Teal,

        //     Colors.Blue,
        //     Colors.Navy,
        //     Colors.Cyan,

        //     Colors.Purple,
        //     Colors.Indigo,
        //     Colors.Pink,
        //     Colors.Brown];

    }

    private void AddColorsToPalette()
    {

        foreach (var color in GetPredefinedColors())
        {
            var colorButton = new Button
            {
                BackgroundColor = color,
                HorizontalOptions = LayoutOptions.Center,
                CornerRadius = 4,
                Margin = new Thickness(1)
            };
            colorButton.Clicked += OnSelectColorClicked;
            colorPalette.Children.Add(colorButton);
        }

    }

    public static readonly BindableProperty SelectedColorProperty =
      BindableProperty.Create(
          nameof(SelectedColor),
          typeof(Color),
          typeof(ColorPicker),
          Colors.White);

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private void OnSelectColorClicked(object sender, EventArgs e)
    {
        subColorPicker.PickedColor = ((Button)sender).BackgroundColor;
    }

}