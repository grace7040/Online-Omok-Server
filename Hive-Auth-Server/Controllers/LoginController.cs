﻿using Microsoft.AspNetCore.Mvc;
using Hive_Auth_Server.DTOs;
using Hive_Auth_Server.Servicies;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IMemoryDb _memoryDb;
        IHiveDb _hiveDb;
        IHasher _hasher;
        ITokenCreator _tokenCreator;
        
        public LoginController(IMemoryDb memoryDb, IHiveDb hiveDb, IHasher hasher, ITokenCreator tokenCreater)
        {
            _memoryDb = memoryDb;
            _hiveDb = hiveDb;
            _hasher = hasher;
            _tokenCreator = tokenCreater;
        }


        [HttpPost("login")]
        public async Task<ResponseDTO> Login(ReqAccountDTO account)
        {
            var passwordFromRequest = _hasher.GetHashedString(account.Password);
            var passwordFromDb = _hiveDb.GetPasswordByEmailAsync(account.Email);

            if (await passwordFromDb != passwordFromRequest)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailWrongPassword };
            }

            //Token 랜덤 생성 및 Redis에 저장
            string token = _tokenCreator.CreateAuthToken();
            ErrorCode result = await _memoryDb.RegistUserAsync(account.Email, token, Expiries.LoginToken);
            if(result != ErrorCode.None)
            {
                return new ResponseDTO { Result = result };
            }
            return new ResUserAuthDTO { Result = ErrorCode.None, Token = token };
        }
    }
}
