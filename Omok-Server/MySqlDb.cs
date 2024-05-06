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
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;

        public MySqlDb(string dbConnectString)
        { 

            //var dbConnectString = _configuration.GetConnectionString("GameDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }

        public async Task<UserGameData> LoadUserGameDataAsync(string id)
        {
            var datas = (await _queryFactory.Query("user_game_data")
                                         .Where("email", id)
                                         .GetAsync<UserGameData>()).FirstOrDefault();

            return datas;

        }

        void ConnectionClose()
        {
            _dbConnection.Close();
        }
    }
}
