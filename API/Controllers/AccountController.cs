using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;
using API.Data;
using API.DTOs;
using API.Interfaces;

namespace API.Controllers;


public class AccountController(DataContext context, ITokenService tokenService): BaseApiController
{
    
    [HttpPost("register")] // account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
        using var hmac = new HMACSHA512();

        if(await UserExists(registerDto.Username)){
            return BadRequest("Username is already taken.");
        }
        var user= new AppUser{
            UserName = registerDto.Username,
            PasswordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDto{
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(x=>x.UserName.ToLower() == loginDto.Username.ToLower());

        if(user == null){
            return Unauthorized("Invalid username");
        }
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));   
        for(int i = 0;i< computedHash.Length; i++){
            if(computedHash[i] != user.PasswordHash[i]){
                return Unauthorized("Invalid password");
            }
        }

        return new UserDto{
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }
    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(u=> u.UserName.ToLower() == username.ToLower());
    }
}
