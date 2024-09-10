using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using RosePark.Models.ViewModels;


using Microsoft.AspNetCore.Mvc.Rendering;

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

        [HttpGet]
        public IActionResult Register()
        {
            // Se puede cargar ViewBag.Roles aquí si es necesario, pero IdRol está predefinido a 1
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Registrar los errores del modelo para debug
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                return View(model);
            }

            try
            {
                var usuario = new Usuario
                {
                    CorreoUsuario = model.CorreoUsuario,
                    ClaveUsuario = model.ClaveUsuario,
                    IdRol = model.IdRol,
                    IdPersonasNavigation = new Persona
                    {
                        Nombres = model.NombrePersona,
                        Apellidos = model.ApellidosPersona,
                        TipoDocumento = model.TipoDocumentoPersona,
                        NroDocumento = model.NroDocumentoPersona,
                        Edad = model.EdadPersona,
                        Celular = model.CelularPersona,
                        FechaNacimiento = model.FechaNacimientoPersona
                    }
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear claims para el usuario
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, usuario.CorreoUsuario),
                    new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Autenticación del usuario
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Redirigir al Index de Home
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Mostrar el error en consola y enviar mensaje a la vista
                Console.WriteLine($"Error al guardar el usuario: {ex.Message}");
                ModelState.AddModelError("", "No se pudo guardar el usuario. Inténtalo de nuevo.");
                return View(model);
            }
        }
    }
}
