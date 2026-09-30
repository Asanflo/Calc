using System;
using Microsoft.Maui.Controls;
using MauiCalculator.ViewModels;

namespace MauiCalculator
{
    public partial class MainPage : ContentPage
    {
        public MainPage(CalculatorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new CalculatorViewModel();
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (BindingContext is CalculatorViewModel vm)
            {
                // Détection de l'orientation écran : Paysage vs Portrait
                bool isLandscape = Width > Height;
                vm.IsLandscape = isLandscape;
                vm.IsPortrait = !isLandscape;
            }
        }
    }
}
