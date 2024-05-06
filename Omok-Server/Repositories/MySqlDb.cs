using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
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

        public async Task<UserGameData> GetUserGameDataAsync(string id)
        {
            var datas = (await _queryFactory.Query("user_game_data")
                                         .Where("email", id)
                                         .GetAsync<UserGameData>()).FirstOrDefault();

            return datas;

        }

        public async Task<ErrorCode> UpdateUserGameDataAsync(string id, int winCount, int loseCount, int level, int exp)
        {
            var datas = await _queryFactory.Query("user_game_data")
                                         .Where("email", id)
                                         .UpdateAsync(new { win_count = winCount,
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
}
