using System.Text;
using TXTextControl;

public class FormFieldHelper
{
    public static void FlattenFormFields(ServerTextControl textControl)
    {
        int fieldCount = textControl.FormFields.Count;

        for (int i = 0; i < fieldCount; i++)
        {
            TextFieldCollectionBase.TextFieldEnumerator fieldEnum =
              textControl.FormFields.GetEnumerator();
            fieldEnum.MoveNext();

            FormField curField = (FormField)fieldEnum.Current;
            textControl.FormFields.Remove(curField, true);
        }
    }

    public static string FormFieldsToJson(ServerTextControl textControl)
    {
        int fieldCount = textControl.FormFields.Count;
        StringBuilder sb = new StringBuilder();
        sb.Append("[");

        TextFieldCollectionBase.TextFieldEnumerator fieldEnum =
             textControl.FormFields.GetEnumerator();

        for (int i = 0; i < fieldCount; i++)
        {
            fieldEnum.MoveNext();

            FormField curField = (FormField)fieldEnum.Current;
            sb.Append("{");
            sb.Append("\"name\": \"" + curField.Name + "\",");
            sb.Append("\"value\": \"" + curField.Text + "\"");
            sb.Append("}");

            if (i < fieldCount - 1)
                sb.Append(",");
        }

        sb.Append("]");

        return sb.ToString();
    }
}

