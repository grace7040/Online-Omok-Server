﻿using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SqlKata.Execution;

namespace OmokServer;

public class MySqlDb
{
    QueryFactory _queryFactory;
    MySqlConnection _dbConnection;

    public MySqlDb(string dbConnectString)
    { 
        _dbConnection = new MySqlConnection(dbConnectString);
        _dbConnection.Open();

        var compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConnection, compiler);
    }

    public UserGameData GetUserGameData(string id)
    {
        var datas = (_queryFactory.Query("user_game_data")
                                     .Where("id", id)
                                     .Get<UserGameData>()).FirstOrDefault();

        return datas;

    }

    public ErrorCode UpdateUserGameData(string id, int winCount, int loseCount, int level, int exp)
    {
        var datas = _queryFactory.Query("user_game_data")
                                     .Where("id", id)
                                     .Update(new { win_count = winCount,
                                                      lose_count = loseCount,
                                                      level = level,
                                                      exp = exp });

        if(datas == 0)
        {
            return ErrorCode.UpdateUserGameDataFail;
        }
                                     

        return ErrorCode.None;

    }

    void ConnectionClose()
    {
        _dbConnection.Close();
    }
}
