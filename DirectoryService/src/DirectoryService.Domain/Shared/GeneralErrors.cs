namespace DirectoryService.Domain.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null, string? invalidField = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} недействительно", invalidField);
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("length.is.invalid", $"Поле{label}обязательно");
    }

    public static Error IncorrectValueLength(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("incorrect.value.length", $"Неподходящая длина поля{label}");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
    }


    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка");
    }
}