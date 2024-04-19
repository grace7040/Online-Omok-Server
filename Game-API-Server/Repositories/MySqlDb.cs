using MySqlConnector;
using SqlKata.Execution;

namespace Game_API_Server.Repositories
{
    public class MySqlDb : IGameDb
    {
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;

        public MySqlDb(IConfiguration configuration)
        {
            _configuration = configuration;

            var dbConnectString = _configuration.GetConnectionString("GameDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }

        public void Dispose()
        {
            ConnectionClose();
        }
        public async Task<ErrorCode> InsertAccountAsync(string email)
        {
            try
            {
                //존재하는 이메일인지    
                if (await IsUserEmailExistAsync(email))
                {
                    return ErrorCode.CreateAccountFailAlreadyExist;
                }

                var count = await _queryFactory.Query("user_game_data")
                                  .InsertAsync(new { 
                                      email = email,
                                      level = 1,
                                      exp = 0,
                                      win_count = 0,
                                      lose_count = 0
                                  });

                //DB 추가 실패시
                if (count != 1)
                {
                    return ErrorCode.CreateAccountFailInsertAccount;
                }
            }
            catch
            {
                return ErrorCode.CreateAccountFailException;
            }

            return ErrorCode.None;
        }

        public async Task<bool> IsUserEmailExistAsync(string email)
        {
            var count = (await _queryFactory.Query("user_game_data")
                                         .Select("level").Where("email", email)
                                         .GetAsync<int>()).FirstOrDefault();

            if (count != 0)
            {
                return true;
            }

            return false;
        }

        void ConnectionClose()
        {
            _dbConnection.Close();
        }
    }
}
