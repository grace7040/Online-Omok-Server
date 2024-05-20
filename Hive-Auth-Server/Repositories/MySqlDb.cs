using MySqlConnector;
using SqlKata.Execution;

namespace HiveAuthServer.Repositories
{
    public class MySqlDb : IHiveDb
    {
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;

        public MySqlDb(IConfiguration configuration)
        {
            _configuration = configuration;

            var dbConnectString = _configuration.GetConnectionString("HiveDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }

        public void Dispose()
        {
            ConnectionClose();
        }
        public async Task<ErrorCode> InsertAccountAsync(string Id, string password)
        {
            try
            {
                //존재하는 이메일인지    
                if (await IsUserEmailExistAsync(Id))
                {
                    return ErrorCode.CreateAccountFailAlreadyExist;
                }

                var count = await _queryFactory.Query("user_account_data")
                                  .InsertAsync(new { id = Id, password = password });

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

        public async Task<string> GetPasswordByIdAsync(string id)
        {
            if(await IsUserEmailExistAsync(id))
            {
                var password = (await _queryFactory.Query("user_account_data")
                                         .Select("password").Where("id", id)
                                         .GetAsync<string>()).FirstOrDefault();
                return password;
            }
            return string.Empty;
        }


        async Task<bool> IsUserEmailExistAsync(string id)
        {
            var count = (await _queryFactory.Query("user_account_data")
                                         .Select("uid").Where("id", id)
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
