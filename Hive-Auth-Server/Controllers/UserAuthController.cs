﻿using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using System.Security.Principal;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class UserAuthController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;

        public UserAuthController(IConfiguration configuration, IMemoryDb memoryDb) {
            _configuration = configuration;
            _memoryDb = memoryDb;
        }


         /* :: TODO :: 내부 기능들 서비스 단위로 분리하기. */
        //GameAPIServer의 LoginController가 참조함
        [HttpPost("checkuserauth")]
        public async Task<IActionResult> CheckUserAuth(ReqUserAuthDTO auth)
        {
            ErrorCode result = await _memoryDb.CheckUserAuthAsync(auth.Email, auth.Token);

            if(result != ErrorCode.None)
            {
                // :: TODO :: 로깅 추가
                return BadRequest();
            }
            return Ok();
        }
    }
}
