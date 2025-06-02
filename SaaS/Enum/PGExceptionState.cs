namespace SaaS.Enum;

public enum PGExceptionState
{
    UniqueViolation = 23505,
    ForeignKeyViolation = 23503,
    NotNullViolation = 23502,
    DuplicateObject = 42710,
}

public static class PGExceptionStateExtensions
{
    public static string StateCode(this PGExceptionState state)
    {
        return ((int)state).ToString();
    }
    public static string StateName(this PGExceptionState state)
    {
        return state switch
        {
            PGExceptionState.UniqueViolation => "Unique Violation",
            PGExceptionState.ForeignKeyViolation => "Foreign Key Violation",
            PGExceptionState.NotNullViolation => "Not Null Violation",
            PGExceptionState.DuplicateObject => "Duplicate Object",
            _ => "Unknown Error"
        };
    }
}