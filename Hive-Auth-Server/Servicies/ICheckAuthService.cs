﻿namespace HiveAuthServer.Services;

public interface ICheckAuthService
{
    public Task<bool> CheckAuthToMemoryDbAsync(string email, string token);
}
