using Microsoft.AspNetCore.Mvc;
using TXTextControl;
using TXTextControl.DocumentServer;

namespace tx_workflow_merge.Controllers
{
    public class DocumentController : Controller
    {
        [HttpPost]
        public IActionResult SaveTemplate([FromBody] string document, [FromQuery] string templateName)
        {
            if (string.IsNullOrEmpty(document) || string.IsNullOrEmpty(templateName))
            {
                return BadRequest("Invalid input: Document and template name must be provided.");
            }

            try
            {
                // Convert base64 string to byte array
                byte[] bytes = Convert.FromBase64String(document);

                // Ensure the directory exists
                string directoryPath = Path.Combine("Templates");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Safely combine the path and template name
                string filePath = Path.Combine(directoryPath, templateName);

                // Save the document to a file
                System.IO.File.WriteAllBytes(filePath, bytes);

                // Return JSON with the file name
                return Json(new { templateName = templateName });
            }
            catch (FormatException)
            {
                return BadRequest("Invalid base64 string.");
            }
            catch (IOException ex)
            {
                return StatusCode(500, "Error saving the file.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        public IActionResult LoadTemplate([FromQuery] string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return BadRequest("Template name must be provided.");
            }

            try
            {
                // Safely combine the path and template name
                string filePath = Path.Combine("Data", templateName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Template not found.");
                }

                // Read the document from the file
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);

                // Convert byte array to base64 string
                string document = Convert.ToBase64String(bytes);

                // Return the document as a JSON object
                return Json(new { document = document });
            }
            catch (IOException ex)
            {
                return StatusCode(500, "Error reading the file.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [HttpGet]
        public IActionResult MergeTemplate([FromQuery] string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return BadRequest("Template name must be provided.");
            }

            try
            {
                // Safely combine the path and template name
                string templatePath = Path.Combine("Templates", templateName);
                string dataPath = Path.Combine("Data", "data.json");

                if (!System.IO.File.Exists(templatePath))
                {
                    return NotFound("Template not found.");
                }

                if (!System.IO.File.Exists(dataPath))
                {
                    return NotFound("Data file not found.");
                }

                // Read the template and data files
                byte[] templateBytes = System.IO.File.ReadAllBytes(templatePath);
                string jsonData = System.IO.File.ReadAllText(dataPath);

                using (ServerTextControl tx = new ServerTextControl())
                {
                    // Load the template
                    tx.Create();
                    tx.Load(templateBytes, BinaryStreamType.InternalUnicodeFormat);

                    MailMerge merge = new MailMerge
                    {
                        TextComponent = tx,
                        FormFieldMergeType = FormFieldMergeType.Preselect
                    };

                    // Merge the template with JSON data
                    merge.MergeJsonData(jsonData);

                    // Save the merged document to a byte array
                    byte[] document;
                    tx.Save(out document, BinaryStreamType.InternalUnicodeFormat);

                    // Convert byte array to base64 string
                    string documentBase64 = Convert.ToBase64String(document);

                    // Return the document as a JSON object
                    return Json(new { document = documentBase64 });
                }
            }
            catch (IOException ex)
            {
                return StatusCode(500, "Error reading files or saving the document.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public IActionResult Finalize([FromBody] string document, [FromQuery] string templateName)
        {
            if (string.IsNullOrEmpty(document) || string.IsNullOrEmpty(templateName))
            {
                return BadRequest("Document and template name must be provided.");
            }

            try
            {
                using (ServerTextControl tx = new ServerTextControl())
                {
                    // Load the document
                    tx.Create();
                    byte[] bytes = Convert.FromBase64String(document);
                    tx.Load(bytes, BinaryStreamType.InternalUnicodeFormat);

                    // Convert form fields to JSON
                    string jsonData = FormFieldHelper.FormFieldsToJson(tx);

                    // Get filename from templateName without extension
                    string fileName = Path.GetFileNameWithoutExtension(templateName);
                    string dataPath = Path.Combine("Data", fileName + ".json");

                    // Ensure the directory exists
                    string directoryPath = Path.GetDirectoryName(dataPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Save JSON data to file
                    System.IO.File.WriteAllText(dataPath, jsonData);

                    // Flatten form fields
                    FormFieldHelper.FlattenFormFields(tx);

                    // Save the document to a byte array
                    byte[] finalDocument;
                    tx.Save(out finalDocument, BinaryStreamType.AdobePDF);

                    // Convert byte array to base64 string
                    string finalDocumentBase64 = Convert.ToBase64String(finalDocument);

                    // Return the document as a JSON object
                    return Json(new { document = finalDocumentBase64 });
                }
            }
            catch (FormatException)
            {
                return BadRequest("Invalid base64 string.");
            }
            catch (IOException ex)
            {
                return StatusCode(500, "Error processing the document.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

    }
}
