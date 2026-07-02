using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Shapes;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Views;

public class PhotoTimeCarousel : ContentView
{
    public PhotoTimeCarousel()
    {
        var cv = new CarouselView
        {
            HorizontalOptions = LayoutOptions.Center,
            PeekAreaInsets = new Thickness(50, 0),
            Loop = true
        };
        //Observable Collection
        cv.SetBinding(CarouselView.ItemsSourceProperty, "Schedules");
        //The selected item from the Observable Collection
        cv.SetBinding(CarouselView.CurrentItemProperty, "SelectedSchedule");
        //The command to update the selected item
        //cv.SetBinding(CarouselView.CurrentItemChangedCommandProperty, "OnScheduleChangeCommand");
        ////Provide the parameter that the method requires
        //cv.SetBinding(CarouselView.CurrentItemChangedCommandParameterProperty, "this");
        cv.ItemTemplate = new DataTemplate(() =>
                {
                    var dayLabel = new Label { FontSize = 20, HorizontalOptions = LayoutOptions.Center };
                    dayLabel.SetBinding(Label.TextProperty, "SelectedSchedule.PhotoDateTime", stringFormat: "{0:DDDD, MMMM D}");
                    var timeOfDayLabel = new Label { FontSize = 16, HorizontalOptions = LayoutOptions.Center };
                    timeOfDayLabel.SetBinding(Label.TextProperty, "SelectedSchedule.TimeOfDay");
                    var hourLabel = new Label { FontSize = 14, HorizontalOptions = LayoutOptions.Center };
                    hourLabel.SetBinding(Label.TextProperty, "SelectedSchedule.PhotoDateTime", stringFormat: "{0: h tt}");
                    var iconImage = new Image { WidthRequest = 50, HeightRequest = 50, HorizontalOptions = LayoutOptions.Center };
                    iconImage.SetBinding(Image.SourceProperty, "SelectedSchedule.ForecastIcon");
                    var summaryLabel = new Label { FontSize = 14, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Justify, LineBreakMode = LineBreakMode.WordWrap };
                    summaryLabel.SetBinding(Label.TextProperty, "SelectedSchedule.ForecastDetail");


                    var contentBorder = new Border
                    {
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = 12
                        },
                        StrokeThickness = 4,
                        Stroke = Brush.Green,
                        Padding = new Thickness(20),
                        Margin = new Thickness(20)
                    };

                    return new VerticalStackLayout
                    {
                        Children =
                        {
                            contentBorder,
                            new VerticalStackLayout
                            {
                                Children =
                                {
                                    dayLabel,
                                    timeOfDayLabel,
                                    hourLabel,
                                    iconImage,
                                    summaryLabel
                                }
                            }
                        }
                    };
                });
    }
}
