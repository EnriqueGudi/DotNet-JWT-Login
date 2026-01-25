using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using peluqueria.Data;
using peluqueria.Models;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace peluqueria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Obtener todos los usuarios
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsersModel>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }


        //Loguearse y generar JWT
        [HttpGet("login/{email},{password}")]
        public async Task<ActionResult<UsersModel>> GetUsuario(string email, string password)
        {
            var sql = "SELECT id, nombre, email,password FROM usuarios WHERE email = {0}";
            var usuario = await _context.Usuarios.FromSqlRaw(sql, email).FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Ok(new
                {
                    type = "warning",
                    mensaje = "Correo o contraseña incorrectos"
                });
            }

            // Verificar si la contraseña es correcta
            Boolean valido = false;
            if (usuario.Password.Length>30)
            {
                valido = BCrypt.Net.BCrypt.Verify(password, usuario.Password);
                if (!valido)
                {
                    return Ok(new
                    {
                        type = "warning",
                        mensaje = "Correo o contraseña incorrectos"
                    });
                }
            }
            else
            {
                if (usuario.Password == password)
                {
                    valido = true;
                }
            }

            if (valido)
            {
                // Crear el JWT
                var claims = new[]
                {
                    new Claim("Nombre", usuario.Nombre),
                    new Claim("Correo", usuario.Email)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );

                var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { Token = jwtToken, 
                                nombre = usuario.Nombre,
                                id_usuario = usuario.Id,
                                type="success"
                });


            }
            else
            {
                return Ok(new
                {
                    type = "warning",
                    mensaje = "Correo o contraseña incorrectos"
                });
            }
            
        }

        
    }
}
