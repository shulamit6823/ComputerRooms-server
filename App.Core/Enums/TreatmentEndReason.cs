namespace App.Core.Enums;

/// <summary>למה הסתיים שלב טיפול בהיסטוריית הטיפול</summary>
public enum TreatmentEndReason
{
    /// <summary>אחראית אחרת לקחה את הפנייה</summary>
    TakenByAnother,
    /// <summary>הטיפול הסתיים</summary>
    Completed,
    /// <summary>הפנייה הוחזרה לממתינה לטיפול</summary>
    ReturnedToPending,
    /// <summary>האחראית נמחקה מהמערכת</summary>
    ResponsibleDeleted
}
