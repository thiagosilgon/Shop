using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;


namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
        {
            var users = await context
                .Users
                .AsNoTracking()
                .ToListAsync();
            return users;
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        //[Authorize(Roles ="manager")]
        public async Task<ActionResult<List<User>>> Post(
            [FromBody] User model,
            [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //Força o usuário a ser sempre "funcionario"
                model.Role = "empoyee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                //Esconde a senha
                model.Password="";
                return Ok(model);
            }
            catch(Exception)
            {
                return BadRequest(new {message = "Não foi possível criar a categoria"});
            }        
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromServices] DataContext context,
            [FromBody] User model)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();
        
            if(user == null)
                return NotFound(new { message = "Usuário ou senha inválida"});

            var token = TokenService.GenerateToken(user);
            //Esconde a senha
            user.Password="";
            return new
            {
                user = user,
                token = token
            };        
        }

        [HttpPut]
        [Route("id:int")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult<User>> Put(
            [FromServices] DataContext context,
            int id,
            [FromBody] User model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(id != model.Id)
                return NotFound(new { message = "Usuário não encontrado"});

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }
        }
    }
}