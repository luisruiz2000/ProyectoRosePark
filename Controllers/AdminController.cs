using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RosePark.Controllers
{

    public class AdminController : Controller
    {

        private readonly RoseParkDbContext _context;

        public AdminController(RoseParkDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View(); // Esto buscará en /Views/Admin/Dashboard.cshtml
        }
    }
}