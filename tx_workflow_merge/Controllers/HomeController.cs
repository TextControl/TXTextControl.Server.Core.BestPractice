using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tx_workflow_merge.Models;

namespace tx_workflow_merge.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Template()
		{
            // read json file 
            string json = System.IO.File.ReadAllText("data/data.json");
            ViewBag.Json = json;

            // generate unique filename
            var templateName = Guid.NewGuid().ToString() + ".tx";
			ViewBag.TemplateName = templateName;

            return View();
		}

		public IActionResult FillIn()
		{
			List<Template> templates = new List<Template>();

            // get all template files
            string[] files = Directory.GetFiles("Templates", "*.tx");

            // iterate through all template files
            foreach (string file in files)
            {
                    // add the template to the list
                templates.Add(new Template() { TemplateName = Path.GetFileName(file) });
            }

            // pass the list to the view
            return View(templates);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
