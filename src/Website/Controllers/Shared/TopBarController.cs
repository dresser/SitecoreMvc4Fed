using System.Web.Mvc;
using Website.Models;

namespace Website.Controllers.Shared
{
    public class TopBarController : Controller
    {
        public ActionResult TopBar()
        {
            string s = null;
            var model = new TopBarModel(s);
            return View(model);
        }
    }
}