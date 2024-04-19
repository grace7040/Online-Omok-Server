﻿public interface IGameDb
{
    public Task<ErrorCode> InsertAccountAsync(string email);

    public Task<bool> IsUserEmailExistAsync(string email);
}