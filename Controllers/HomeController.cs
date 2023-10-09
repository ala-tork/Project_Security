using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using security.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace security.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.Password))
                {
                    HttpContext.Session.SetString("UserId", user.IdUser.ToString());
                    HttpContext.Session.SetString("UserFirstName", user.FirstName);
                    HttpContext.Session.SetString("UserLastName", user.LastName);
                    HttpContext.Session.SetString("UserPhone", user.PhoneNumber);
                    HttpContext.Session.SetString("UserRole", user.UserRole.ToString());

                    return RedirectToAction(nameof(ReclamationList));
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Index");
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                model.Password = HashPassword(model.Password);
                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("");
            }

            return View("Register", model);
        }




        public IActionResult ReclamationForm()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = _context.Users.FirstOrDefault(u => u.IdUser == Guid.Parse(userId));

            var reclamation = new Reclamation
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return View(reclamation);
        }



        [HttpPost]
        public async Task<IActionResult> CreateReclamation(Reclamation reclamation)
        {
            if (ModelState.IsValid)
            {
                var userIdString = HttpContext.Session.GetString("UserId");

                if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
                {
                    reclamation.IdUser = userId;


                    string encryptionKey = "12345678901234567890123456789012";
                    reclamation.Message = Encrypt(reclamation.Message, encryptionKey);

                    _context.Reclamations.Add(reclamation);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("ReclamationList");
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

            return View("ReclamationForm", reclamation);
        }

        

        public IActionResult ReclamationList()
        {
            var UserRole = HttpContext.Session.GetString("UserRole");
            string encryptionKey = "12345678901234567890123456789012";

            if (UserRole == "DEVELOPER" || UserRole== "SCRUM_MASTER")
            {
                var userId = HttpContext.Session.GetString("UserId");
                var UserReclamations = _context.Reclamations.Where(r => r.IdUser == Guid.Parse(userId))
                    .ToList();

                foreach (var reclamation in UserReclamations)
                {
                    reclamation.Message = Decrypt(reclamation.Message, encryptionKey);
                }

                return View(UserReclamations);
            }else if(UserRole == "MANAGER")
            {
                var ManagerReclamations = _context.Reclamations.ToList();
                return View(ManagerReclamations);
            }

            var reclamations = _context.Reclamations.ToList();

            foreach (var reclamation in reclamations)
            {
                reclamation.Message = Decrypt(reclamation.Message, encryptionKey);
            }

            return View(reclamations);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        // chiffrement de message 
        private string Encrypt(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                MemoryStream msEncrypt = new MemoryStream();
                using (msEncrypt)
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

        
        private string Decrypt(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; 

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }



        // crypt password
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}