namespace AccountManagement.Application.Common.Responses
{
    public class ApiError
    {
        public Dictionary<string, string[]>? FieldErrors { get; set; }
        public List<string>? GeneralErrors { get; set; }
    }
}
