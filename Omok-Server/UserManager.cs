using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class UserManager
    {
        int _maxUserCount;

        Dictionary<string, User> _userMap = new Dictionary<string, User>();

        public void Init(int maxUserCount)
        {
            _maxUserCount = maxUserCount;
        }


        //로그인 시
        public ErrorCode AddUser(string userID, string sessionID)
        {
            if (IsFullUserCount())
            {
                return ErrorCode.LOGIN_FULL_USER_COUNT;
            }

            if (_userMap.ContainsKey(sessionID))
            {
                return ErrorCode.ADD_USER_DUPLICATION;
            }

            var user = new User();
            user.Set(true, sessionID, userID);
            _userMap.Add(sessionID, user);

            return ErrorCode.NONE;
        }

        public ErrorCode RemoveUser(string sessionID)
        {
            if (_userMap.Remove(sessionID) == false)
            {
                return ErrorCode.REMOVE_USER_SEARCH_FAILURE_USER_ID;
            }

            return ErrorCode.NONE;
        }

        public User GetUserBySessionId(string sessionID)
        {
            User user = null;
            _userMap.TryGetValue(sessionID, out user);
            return user;
        }

        bool IsFullUserCount()
        {
            return _maxUserCount <= _userMap.Count();
        }
    }
}
