using MySqlConnector;
using SqlKata.Execution;

namespace Hive_Auth_Server.Repositories
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
        public async Task<ErrorCode> InsertAccountAsync(string email, string password)
        {
            try
            {
                //존재하는 이메일인지    
                if (await IsUserEmailExistAsync(email))
                {
                    return ErrorCode.CreateAccountFailAlreadyExist;
                }

                var count = await _queryFactory.Query("user_account_data")
                                  .InsertAsync(new { email = email, password = password });

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

        public async Task<string> GetPasswordByEmailAsync(string email)
        {
            if(await IsUserEmailExistAsync(email))
            {
                var password = (await _queryFactory.Query("user_account_data")
                                         .Select("password").Where("email", email)
                                         .GetAsync<string>()).FirstOrDefault();
                return password;
            }
            return string.Empty;
        }


        async Task<bool> IsUserEmailExistAsync(string email)
        {
            var count = (await _queryFactory.Query("user_account_data")
                                         .Select("uid").Where("email", email)
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
