using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ActionResponse
{
    public bool Success { get; set; }
    public object SucessValue { get; set; }
    public List<ErrorItem> ErrorMessage { get; set; }

    public ActionResponse()
    {
        Clear();
    }

    public void Clear()
    {
        Success = true;
        SucessValue = new object();
        ErrorMessage = new List<ErrorItem>();
    }
    public void AddError(string message) => AddError(message, "", "");
    public void AddError(string message, string field) => AddError(message, field, "");
    public void AddError(string message, string field, string fieldValue)
    {
        Success = false;
        ErrorMessage.Add(new ErrorItem
        {
            Field = field,
            FieldValue = fieldValue,
            Message = message
        });
    }
}

public class ErrorItem
{
    public string Message { get; set; }
    public string Field { get; set; }
    public string FieldValue { get; set; }
}
