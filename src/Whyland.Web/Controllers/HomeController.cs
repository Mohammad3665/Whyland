namespace Whyland.Web.Controllers;

/// <summary>
/// The public landing page (anonymous). Serves as the post-login redirect target until the
/// real home page is built from the template's <c>index.html</c>.
/// </summary>
public class HomeController : Controller
{
    /// <summary>Renders the public home page.</summary>
    [HttpGet]
    public IActionResult Index() => View();
}
