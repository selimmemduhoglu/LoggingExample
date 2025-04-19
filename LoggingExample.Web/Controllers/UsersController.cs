using Microsoft.AspNetCore.Mvc;
using LoggingExample.Web.Models;
using LoggingExample.Web.Models.Exceptions;

namespace LoggingExample.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        
        // Örnek kullanıcı listesi
        private static readonly List<UserDto> _users = new()
        {
            new UserDto { Id = 1, Username = "johndoe", Email = "john@example.com", Age = 30, BirthDate = DateTime.Parse("1993-05-15") },
            new UserDto { Id = 2, Username = "janedoe", Email = "jane@example.com", Age = 25, BirthDate = DateTime.Parse("1998-10-22") }
        };

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            _logger.LogInformation("Tüm kullanıcılar listeleniyor. Toplam {Count} kullanıcı", _users.Count);
            
            var response = ApiResponse<List<UserDto>>.CreateSuccess(_users, "Kullanıcılar başarıyla listelendi");
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            _logger.LogInformation("Kullanıcı aranıyor: {UserId}", id);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserId}", id);
                throw new NotFoundException("Kullanıcı", id);
            }
            
            _logger.LogInformation("Kullanıcı bulundu: {UserId}", id);
            var response = ApiResponse<UserDto>.CreateSuccess(user, "Kullanıcı başarıyla bulundu");
            return Ok(response);
        }

        [HttpPost]
        public IActionResult CreateUser(UserDto model)
        {
            _logger.LogInformation("Yeni kullanıcı oluşturuluyor: {Username}", model.Username);
            
            // Bu model validasyonu ValidateModelAttribute tarafından otomatik olarak yapılacak
            
            // Email zaten kullanılıyor mu?
            if (_users.Any(u => u.Email == model.Email))
            {
                throw new BusinessException("E-posta adresi zaten kullanılıyor");
            }
            
            // Kullanıcıyı ekle
            model.Id = _users.Count + 1;
            _users.Add(model);
            
            _logger.LogInformation("Yeni kullanıcı oluşturuldu: {UserId}, {Username}", model.Id, model.Username);
            
            var response = ApiResponse<UserDto>.CreateSuccess(model, "Kullanıcı başarıyla oluşturuldu");
            return Created($"/api/users/{model.Id}", response);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, UserDto model)
        {
            _logger.LogInformation("Kullanıcı güncelleniyor: {UserId}", id);
            
            // Kullanıcı var mı?
            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                throw new NotFoundException("Kullanıcı", id);
            }
            
            // E-posta değişmiş mi ve kullanılıyor mu?
            if (model.Email != existingUser.Email && _users.Any(u => u.Email == model.Email))
            {
                throw new BusinessException("E-posta adresi zaten kullanılıyor");
            }
            
            // Güncelle
            existingUser.Username = model.Username;
            existingUser.Email = model.Email;
            existingUser.Age = model.Age;
            existingUser.BirthDate = model.BirthDate;
            
            _logger.LogInformation("Kullanıcı güncellendi: {UserId}", id);
            
            var response = ApiResponse<UserDto>.CreateSuccess(existingUser, "Kullanıcı başarıyla güncellendi");
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            _logger.LogInformation("Kullanıcı siliniyor: {UserId}", id);
            
            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                throw new NotFoundException("Kullanıcı", id);
            }
            
            _users.Remove(existingUser);
            
            _logger.LogInformation("Kullanıcı silindi: {UserId}", id);
            
            return NoContent();
        }
    }
} 