using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiCalculator.Models;

namespace MauiCalculator.ViewModels
{
    /// <summary>
    /// ViewModel principal de la calculatrice mobile sous architecture MVVM (.NET MAUI)
    /// Gère les opérations de base, décimaux, mémoire, fonctions scientifiques et historique.
    /// </summary>
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private string _displayText = "0";
        private string _currentOperationExpression = "";
        private string _errorMessage = "";
        private bool _hasError = false;
        private string _angleModeText = "DEG";
        private bool _isPortrait = true;
        private bool _isLandscape = false;
        private bool _isSecondMode = false;
        private double? _previousValue = null;
        private string? _currentOperator = null;
        private bool _waitingForOperand = false;
        private double _memory = 0.0;

        public ObservableCollection<CalculationRecord> History { get; } = new();

        public CalculatorViewModel()
        {
            DigitCommand = new Command<string>(OnDigit);
            DecimalCommand = new Command(OnDecimal);
            ClearCommand = new Command(OnClear);
            DeleteLastCharCommand = new Command(OnDeleteLastChar);
            ToggleSignCommand = new Command(OnToggleSign);
            PercentageCommand = new Command(OnPercentage);
            OperatorCommand = new Command<string>(OnOperator);
            CalculateCommand = new Command(OnCalculate);
            ScientificCommand = new Command<string>(OnScientific);
            InsertConstantCommand = new Command<string>(OnInsertConstant);
            ToggleAngleModeCommand = new Command(OnToggleAngleMode);
            ToggleSecondModeCommand = new Command(() => IsSecondMode = !IsSecondMode);
            MemoryCommand = new Command<string>(OnMemory);
            RandomCommand = new Command(OnRandom);
            ToggleMenuCommand = new Command(OnToggleMenu);
            ToggleOrientationCommand = new Command(OnToggleOrientation);
        }

        #region Propriétés Notifiables (Binding)

        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        public string CurrentOperationExpression
        {
            get => _currentOperationExpression;
            set => SetProperty(ref _currentOperationExpression, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set {
                SetProperty(ref _errorMessage, value);
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public bool IsPortrait
        {
            get => _isPortrait;
            set => SetProperty(ref _isPortrait, value);
        }

        public bool IsLandscape
        {
            get => _isLandscape;
            set => SetProperty(ref _isLandscape, value);
        }

        public bool IsSecondMode
        {
            get => _isSecondMode;
            set => SetProperty(ref _isSecondMode, value);
        }

        public string AngleModeText
        {
            get => _angleModeText;
            set => SetProperty(ref _angleModeText, value);
        }

        public string ClearButtonText => (_displayText == "0" && string.IsNullOrEmpty(_currentOperationExpression)) ? "AC" : "C";

        public Color DisplayTextColor => HasError ? Color.FromArgb("#FF453A") : Color.FromArgb("#FFFFFF");

        public int ResultFontSize => DisplayText.Length > 9 ? (DisplayText.Length > 13 ? 34 : 46) : 64;

        #endregion

        #region Commandes ICommand

        public ICommand DigitCommand { get; }
        public ICommand DecimalCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand DeleteLastCharCommand { get; }
        public ICommand ToggleSignCommand { get; }
        public ICommand PercentageCommand { get; }
        public ICommand OperatorCommand { get; }
        public ICommand CalculateCommand { get; }
        public ICommand ScientificCommand { get; }
        public ICommand InsertConstantCommand { get; }
        public ICommand ToggleAngleModeCommand { get; }
        public ICommand ToggleSecondModeCommand { get; }
        public ICommand MemoryCommand { get; }
        public ICommand RandomCommand { get; }
        public ICommand ToggleMenuCommand { get; }
        public ICommand ToggleOrientationCommand { get; }

        #endregion

        #region Logique Opérationnelle

        private void OnDigit(string digit)
        {
            ClearError();
            if (_waitingForOperand || DisplayText == "0")
            {
                DisplayText = digit;
                _waitingForOperand = false;
            }
            else
            {
                if (DisplayText.Length < 16)
                    DisplayText += digit;
            }
            OnPropertyChanged(nameof(ClearButtonText));
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void OnDecimal()
        {
            ClearError();
            if (_waitingForOperand)
            {
                DisplayText = "0.";
                _waitingForOperand = false;
            }
            else if (!DisplayText.Contains("."))
            {
                DisplayText += ".";
            }
            OnPropertyChanged(nameof(ClearButtonText));
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void OnClear()
        {
            DisplayText = "0";
            CurrentOperationExpression = "";
            _previousValue = null;
            _currentOperator = null;
            _waitingForOperand = false;
            ClearError();
            OnPropertyChanged(nameof(ClearButtonText));
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void OnDeleteLastChar()
        {
            if (HasError)
            {
                OnClear();
                return;
            }
            if (_waitingForOperand) return;

            if (DisplayText.Length <= 1 || (DisplayText.Length == 2 && DisplayText.StartsWith("-")))
            {
                DisplayText = "0";
            }
            else
            {
                DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
            }
            OnPropertyChanged(nameof(ClearButtonText));
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void OnToggleSign()
        {
            if (HasError || DisplayText == "0") return;
            if (double.TryParse(DisplayText, out double val))
            {
                DisplayText = (-val).ToString("G12");
            }
        }

        private void OnPercentage()
        {
            if (HasError || !double.TryParse(DisplayText, out double val)) return;
            double res;
            if (_previousValue.HasValue && (_currentOperator == "+" || _currentOperator == "-"))
            {
                res = _previousValue.Value * (val / 100.0);
            }
            else
            {
                res = val / 100.0;
            }
            DisplayText = res.ToString("G12");
        }

        private void OnOperator(string op)
        {
            if (HasError || !double.TryParse(DisplayText, out double val)) return;

            if (_previousValue.HasValue && !_waitingForOperand && !string.IsNullOrEmpty(_currentOperator))
            {
                if (!ExecuteOperation(_previousValue.Value, val, _currentOperator, out double interim))
                    return;

                _previousValue = interim;
                DisplayText = interim.ToString("G12");
            }
            else
            {
                _previousValue = val;
            }

            _currentOperator = op;
            CurrentOperationExpression = $"{DisplayText} {op}";
            _waitingForOperand = true;
        }

        private void OnCalculate()
        {
            if (HasError || !_previousValue.HasValue || string.IsNullOrEmpty(_currentOperator))
                return;

            if (double.TryParse(DisplayText, out double currentVal))
            {
                string fullExpr = $"{CurrentOperationExpression} {DisplayText}";

                if (ExecuteOperation(_previousValue.Value, currentVal, _currentOperator, out double result))
                {
                    string formatted = result.ToString("G12");
                    DisplayText = formatted;
                    CurrentOperationExpression = $"{fullExpr} =";
                    _previousValue = null;
                    _currentOperator = null;
                    _waitingForOperand = true;

                    History.Insert(0, new CalculationRecord
                    {
                        Expression = fullExpr,
                        Result = formatted,
                        Timestamp = DateTime.Now
                    });
                }
                else
                {
                    // Erreur gérée (par exemple Division par zéro)
                    History.Insert(0, new CalculationRecord
                    {
                        Expression = fullExpr,
                        Result = "Erreur",
                        Timestamp = DateTime.Now,
                        IsError = true,
                        ErrorMessage = ErrorMessage
                    });
                }
                OnPropertyChanged(nameof(ResultFontSize));
            }
        }

        private bool ExecuteOperation(double a, double b, string op, out double result)
        {
            result = 0;
            switch (op)
            {
                case "+": result = a + b; return true;
                case "-": result = a - b; return true;
                case "×":
                case "*": result = a * b; return true;
                case "÷":
                case "/":
                    // GESTION EXPLICITE DE LA DIVISION PAR ZERO SANS CRASH
                    if (Math.Abs(b) < 1e-15)
                    {
                        TriggerError("Impossible de diviser par zéro");
                        return false;
                    }
                    result = a / b;
                    return true;
                case "xʸ":
                    if (a < 0 && Math.Abs(b - Math.Round(b)) > 1e-9)
                    {
                        TriggerError("Résultat non réel");
                        return false;
                    }
                    result = Math.Pow(a, b);
                    return true;
                case "ʸ√x":
                    if (b == 0) { TriggerError("Degré de racine indéfini"); return false; }
                    result = Math.Pow(a, 1.0 / b);
                    return true;
                default:
                    result = b;
                    return true;
            }
        }

        private void OnScientific(string func)
        {
            if (HasError || !double.TryParse(DisplayText, out double val)) return;

            double res = 0;
            string expr = "";

            switch (func)
            {
                case "sin":
                    double rSin = AngleModeText == "DEG" ? (val * Math.PI / 180.0) : val;
                    res = Math.Sin(rSin);
                    expr = $"sin({val})";
                    break;
                case "cos":
                    double rCos = AngleModeText == "DEG" ? (val * Math.PI / 180.0) : val;
                    res = Math.Cos(rCos);
                    expr = $"cos({val})";
                    break;
                case "tan":
                    if (AngleModeText == "DEG" && Math.Abs((val - 90) % 180) < 1e-6)
                    {
                        TriggerError("Tangente indéfinie");
                        return;
                    }
                    double rTan = AngleModeText == "DEG" ? (val * Math.PI / 180.0) : val;
                    res = Math.Tan(rTan);
                    expr = $"tan({val})";
                    break;
                case "ln":
                    if (val <= 0) { TriggerError("ln(x) nécessite x > 0"); return; }
                    res = Math.Log(val);
                    expr = $"ln({val})";
                    break;
                case "log":
                    if (val <= 0) { TriggerError("log10(x) nécessite x > 0"); return; }
                    res = Math.Log10(val);
                    expr = $"log({val})";
                    break;
                case "x²":
                    res = val * val;
                    expr = $"({val})²";
                    break;
                case "x³":
                    res = val * val * val;
                    expr = $"({val})³";
                    break;
                case "sqrt":
                    if (val < 0) { TriggerError("Racine d'un nombre négatif"); return; }
                    res = Math.Sqrt(val);
                    expr = $"√({val})";
                    break;
                case "1/x":
                    if (Math.Abs(val) < 1e-15) { TriggerError("Division par zéro"); return; }
                    res = 1.0 / val;
                    expr = $"1/({val})";
                    break;
                case "eˣ":
                    res = Math.Exp(val);
                    expr = $"e^({val})";
                    break;
                case "10ˣ":
                    res = Math.Pow(10, val);
                    expr = $"10^({val})";
                    break;
                case "n!":
                    if (val < 0 || val != Math.Floor(val)) { TriggerError("Entier positif requis"); return; }
                    if (val > 170) { TriggerError("Dépassement de capacité"); return; }
                    res = Factorial((int)val);
                    expr = $"{val}!";
                    break;
                case "sinh": res = Math.Sinh(val); expr = $"sinh({val})"; break;
                case "cosh": res = Math.Cosh(val); expr = $"cosh({val})"; break;
                case "tanh": res = Math.Tanh(val); expr = $"tanh({val})"; break;
                default: return;
            }

            string formatted = res.ToString("G12");
            DisplayText = formatted;
            CurrentOperationExpression = $"{expr} =";
            _waitingForOperand = true;

            History.Insert(0, new CalculationRecord
            {
                Expression = expr,
                Result = formatted,
                Timestamp = DateTime.Now
            });
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private double Factorial(int n)
        {
            if (n <= 1) return 1;
            double r = 1;
            for (int i = 2; i <= n; i++) r *= i;
            return r;
        }

        private void OnInsertConstant(string c)
        {
            ClearError();
            if (c == "pi") DisplayText = Math.PI.ToString("G12");
            else if (c == "e") DisplayText = Math.E.ToString("G12");
            else if (c == "phi") DisplayText = "1.61803398875";
            _waitingForOperand = false;
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void OnToggleAngleMode()
        {
            AngleModeText = AngleModeText == "DEG" ? "RAD" : "DEG";
        }

        private void OnMemory(string op)
        {
            double.TryParse(DisplayText, out double current);
            switch (op)
            {
                case "MC": _memory = 0; break;
                case "MR": DisplayText = _memory.ToString("G12"); _waitingForOperand = false; break;
                case "M+": _memory += current; break;
                case "M-": _memory -= current; break;
            }
        }

        private void OnRandom()
        {
            ClearError();
            DisplayText = new Random().NextDouble().ToString("F4");
            _waitingForOperand = false;
        }

        private void OnToggleOrientation()
        {
            IsLandscape = !IsLandscape;
            IsPortrait = !IsLandscape;
        }

        private void TriggerError(string msg)
        {
            DisplayText = "Erreur";
            ErrorMessage = msg;
            HasError = true;
            _previousValue = null;
            _currentOperator = null;
            _waitingForOperand = true;
            OnPropertyChanged(nameof(DisplayTextColor));
            OnPropertyChanged(nameof(ResultFontSize));
        }

        private void ClearError()
        {
            if (HasError)
            {
                HasError = false;
                ErrorMessage = "";
                OnPropertyChanged(nameof(DisplayTextColor));
                OnPropertyChanged(nameof(ResultFontSize));
            }
        }

        private async void OnToggleMenu()
        {
            string action = await Application.Current.MainPage.DisplayActionSheet(
                "Menu Calculatrice", "Fermer", null,
                "Basculer Vue Paysage / Portrait",
                "Historique des calculs",
                $"Unité Angle : {(AngleModeText == "DEG" ? "RAD" : "DEG")}",
                "Tester Division par Zéro (10 ÷ 0)");

            if (action == "Basculer Vue Paysage / Portrait")
            {
                OnToggleOrientation();
            }
            else if (action == "Historique des calculs")
            {
                string hist = History.Count == 0 ? "Aucun historique disponible" :
                    string.Join(Environment.NewLine, History);
                await Application.Current.MainPage.DisplayAlert("Historique", hist, "Fermer");
            }
            else if (action.StartsWith("Unité Angle"))
            {
                OnToggleAngleMode();
            }
            else if (action.StartsWith("Tester Division"))
            {
                OnClear();
                DisplayText = "10";
                OnOperator("÷");
                DisplayText = "0";
                OnCalculate();
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
