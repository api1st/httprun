namespace HttpExecutor.Abstractions
{
    public enum LineType
    {
        Comment,
        RequestName,
        VariableDefinition,
        RequestVerb,
        RequestVerbMultiLine,
        RequestHeader,
        RequestBody,
        Divider,
        UserConfirmation
    }
}