using System;

namespace MauiCalculator.Models
{
    /// <summary>
    /// Représente un enregistrement d'opération dans l'historique des calculs
    /// </summary>
    public class CalculationRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Expression { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsError { get; set; } = false;
        public string? ErrorMessage { get; set; }

        public string FormattedTime => Timestamp.ToString("HH:mm:ss");

        public override string ToString()
        {
            return $"{Expression} = {Result}{(IsError ? $" ({ErrorMessage})" : "")}";
        }
    }
}
