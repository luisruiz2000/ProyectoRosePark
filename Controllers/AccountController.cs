using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RosePark.Controllers
{

    public class AccountController : Controller
    {

        private readonly RoseParkDbContext _context;

        public AccountController(RoseParkDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("Login"); // Esto buscará en /Views/Account/Login.cshtml
        }
        [HttpPost]
public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
{
    // Si el modelo es inválido, retornar la vista de login
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Verificar las credenciales y realizar el login
    var user = _context.Usuarios.FirstOrDefault(u => u.CorreoUsuario == model.CorreoUsuario && u.ClaveUsuario == model.ClaveUsuario);
    if (user == null)
    {
        ModelState.AddModelError("", "Usuario o contraseña incorrectos");
        return View(model);
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.CorreoUsuario),
        new Claim("IdUsuario", user.IdUsuario.ToString()),
        new Claim(ClaimTypes.Role, user.IdRol.ToString())
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties
    {
        IsPersistent = model.RememberMe,
        RedirectUri = returnUrl ?? Url.Action("Index", "Home") // Redirigir al returnUrl o a la página principal
    };

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

    // Redirigir basado en el IdRol
    if (user.IdRol == 1)
    {
        // Redirigir a la vista normal para los clientes
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
    else
    {
        // Redirigir al dashboard para usuarios administrativos
        return RedirectToAction("Dashboard", "Admin");
    }
}




        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private Usuario GetUserByEmail(string email)
        {
            // Implementa la lógica para obtener el usuario de la base de datos
            return new Usuario(); // Reemplaza con el usuario real
        }

        private bool VerifyPassword(string storedPassword, string providedPassword)
        {
            // Implementa la lógica para verificar la contraseña
            return storedPassword == providedPassword; // Reemplaza con la verificación real
        }
    }
}
