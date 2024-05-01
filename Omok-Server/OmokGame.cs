using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class OmokGame
    {
        const int _boardSize = 19;

        const int _turnOverTime = 6000;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        Dictionary<string, StoneColor> _userStoneColorDict;

        int[,] _omokBoard = new int[_boardSize, _boardSize];

        int _previousX  = -1;
        int _previousY  = -1;
        int _currentX  = -1;
        int _currentY  = -1;

        StoneColor _currStoneColor;

        int _turnOverCnt;

        public bool IsGameEnd { get; private set; } = false;
        public bool IsDoubleThree { get; private set; } = false;

        Action<StoneColor> GameEndAction;
        Func<string, byte[], bool> SendFunc;

        public OmokGame(Dictionary<string, StoneColor> userStoneColorDict, Func<string, byte[], bool> sendFunc, Action<StoneColor> gameEndAction)
        {
            _userStoneColorDict = userStoneColorDict;
            SendFunc = sendFunc;
            GameEndAction = gameEndAction;

            StartGame();
        }

        public void StartGame()
        {
            Array.Clear(_omokBoard, 0, _boardSize * _boardSize);

            _previousX = _previousY = -1;
            _currentX = _currentY = -1;
            _currStoneColor = StoneColor.Black;
            _turnOverCnt = 0;
            IsDoubleThree = false;
            IsGameEnd = false;

            string firstTurnPlayer = GetSessionByStoneColor(StoneColor.Black);
            NotifyPutStoneToClient(firstTurnPlayer, null);
            StartTimer();
        }

        string GetSessionByStoneColor(StoneColor stoneColor)
        {
            return _userStoneColorDict.FirstOrDefault(entry =>
                       EqualityComparer<StoneColor>.Default.Equals(entry.Value, stoneColor)).Key;
        }

        public void PutStone(int x, int y)
        {
            CheckDoubleThree(x, y);
            if (IsDoubleThree)
            {
                ResponsePutStone(GetSessionByStoneColor(_currStoneColor), ErrorCode.PutStoneFailInvalidPosition);
                IsDoubleThree = false;
                return;
            }
            if (_omokBoard[x, y] != (int)StoneColor.None)
            {
                ResponsePutStone(GetSessionByStoneColor(_currStoneColor), ErrorCode.PutStoneFailInvalidPosition);
                return;
            }

            _omokBoard[x, y] = (int)_currStoneColor;
            _turnOverCnt = 0;

            _previousX = _currentX;
            _previousY = _currentY;

            _currentX = x;
            _currentY = y;

            ResponsePutStone(GetSessionByStoneColor(_currStoneColor), ErrorCode.None);
            ChangeTurn();
            CheckOmokComplete(x, y);
            
        }

        void ChangeTurn()
        {
            if (IsGameEnd) return;

            if (_currStoneColor == StoneColor.Black)
                _currStoneColor = StoneColor.White;
            else if (_currStoneColor == StoneColor.White)
                _currStoneColor = StoneColor.Black;

            NotifyPutStoneToClient(GetSessionByStoneColor(_currStoneColor), new Tuple<int,int> (_currentX, _currentY));
            StartTimer();
        }

        public bool IsUserTurn(string sessionId)
        {
            return _currStoneColor == _userStoneColorDict[sessionId];
        }

        public void CheckOmokComplete(int x, int y)
        {
            bool omokComplete = false;
            if (CheckOmokHorizontal(x, y) == 5)        // 같은 돌 개수가 5개면 (6목이상이면 게임 계속) 
            {
                omokComplete = true;
            }

            else if (CheckOmokVertical(x, y) == 5)
            {
                omokComplete = true;
            }

            else if (CheckOmokDiagonal(x, y) == 5)
            {
                omokComplete = true;
            }

            else if (CheckOmokInverseDiagonal(x, y) == 5)
            {
                omokComplete = true;
            }

            if (omokComplete)
            {
                EndGame((StoneColor)_omokBoard[x, y]);
            }

        }

        void NotifyPutStoneToClient(string sessionID, Tuple<int, int>? position)
        {
            var notifyPutStone = new PKTNtfPutStone()
            {
                Position = position
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyPutStone, PacketId.NtfPutStone);

            SendFunc(sessionID, sendPacket);
        }

        public void ResponsePutStone(string sessionID, ErrorCode errorCode)
        {
            var resPutStone = new PKTResPutStone()
            {
                Result = (short)errorCode
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resPutStone, PacketId.ResPutStone);
            SendFunc(sessionID, sendPacket);
        }


        async void StartTimer()
        {
            if (IsGameEnd) return;

            int x = _currentX;
            int y = _currentY;
            await Task.Delay(_turnOverTime);

            
            if(x == _currentX && y == _currentY && !IsGameEnd)
            {
                NotifyTurnOver(GetSessionByStoneColor(_currStoneColor));
                if(++_turnOverCnt >= 6)
                {
                    //게임 종료
                    EndGame(StoneColor.None);
                }
            }
        }

        void EndGame(StoneColor stoneColor)
        {
            IsGameEnd = true;
            GameEndAction(stoneColor);
        }

        void NotifyTurnOver(string sessionID)
        {
            var notifyTurnOver = new PKTNtfTurnOver();
            var sendPacket = _packetMgr.GetBinaryPacketData(notifyTurnOver, PacketId.NtfTurnOver);
            SendFunc(sessionID, sendPacket);

            ChangeTurn();
        }

        int CheckOmokHorizontal(int x, int y)      // ㅡ 확인
        {
            int sameStoneCnt = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && _omokBoard[x + i, y] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && _omokBoard[x - i, y] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            return sameStoneCnt;
        }

        int CheckOmokVertical(int x, int y)      // | 확인
        {
            int sameStoneCnt = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (y + i <= 18 && _omokBoard[x, y + i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (y - i >= 0 && _omokBoard[x, y - i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            return sameStoneCnt;
        }

        int CheckOmokDiagonal(int x, int y)      // / 확인
        {
            int sameStoneCnt = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && y - i >= 0 && _omokBoard[x + i, y - i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && y + i <= 18 && _omokBoard[x - i, y + i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            return sameStoneCnt;
        }

        int CheckOmokInverseDiagonal(int x, int y)     // ＼ 확인
        {
            int sameStoneCnt = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && y + i <= 18 && _omokBoard[x + i, y + i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && y - i >= 0 && _omokBoard[x - i, y - i] == _omokBoard[x, y])
                    sameStoneCnt++;

                else
                    break;
            }

            return sameStoneCnt;
        }

        void CheckDoubleThree(int x, int y)     // 33확인
        {
            int doubleThreeCnt = 0;

            doubleThreeCnt += CheckHorizontalDoubleThree(x, y);
            doubleThreeCnt += CheckVerticalDoubleThree(x, y);
            doubleThreeCnt += CheckDiagonalDoubleThree(x, y);
            doubleThreeCnt += CheckInverseDiagonalDoubleThree(x, y);

            if (doubleThreeCnt >= 2)
                IsDoubleThree = true;

            else
                IsDoubleThree = false;
        }

        int CheckHorizontalDoubleThree(int x, int y)    // 가로 (ㅡ) 확인
        {
            int threeStoneCnt = 1;
            int i, j;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 → 확인
            {
                if (x + i > 18)
                    break;

                else if (_omokBoard[x + i, y] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x + i, y] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ← 확인
            {
                if (x - j < 0)
                    break;

                else if (_omokBoard[x - j, y] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x - j, y] != (int)StoneColor.None)
                    break;
            }

            if (threeStoneCnt == 3 && x + i < 18 && x - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((_omokBoard[x + i, y] != (int)StoneColor.None && _omokBoard[x + i - 1, y] != (int)StoneColor.None) || (_omokBoard[x - j, y] != (int)StoneColor.None && _omokBoard[x - j + 1, y] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        private int CheckVerticalDoubleThree(int x, int y)    // 세로 (|) 확인
        {
            int threeStoneCnt = 1;
            int i, j;

            threeStoneCnt = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↓ 확인
            {
                if (y + i > 18)
                    break;

                else if (_omokBoard[x, y + i] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x, y + i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↑ 확인
            {
                if (y - j < 0)
                    break;

                else if (_omokBoard[x, y - j] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x, y - j] != (int)StoneColor.None)
                    break;
            }

            if (threeStoneCnt == 3 && y + i < 18 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((_omokBoard[x, y + i] != (int)StoneColor.None && _omokBoard[x, y + i - 1] != (int)StoneColor.None) || (_omokBoard[x, y - j] != (int)StoneColor.None && _omokBoard[x, y - j + 1] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        int CheckDiagonalDoubleThree(int x, int y)    // 사선 (/) 확인
        {
            int threeStoneCnt = 1;
            int i, j;

            threeStoneCnt = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↗ 확인
            {
                if (x + i > 18 || y - i < 0)
                    break;

                else if (_omokBoard[x + i, y - i] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x + i, y - i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↙ 확인
            {
                if (x - j < 0 || y + j > 18)
                    break;

                else if (_omokBoard[x - j, y + j] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x - j, y + j] != (int)StoneColor.None)
                    break;
            }

            if (threeStoneCnt == 3 && x + i < 18 && y - i > 0 && x - j > 0 && y + j < 18)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((_omokBoard[x + i, y - i] != (int)StoneColor.None && _omokBoard[x + i - 1, y - i + 1] != (int)StoneColor.None) || (_omokBoard[x - j, y + j] != (int)StoneColor.None && _omokBoard[x - j + 1, y + j - 1] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        int CheckInverseDiagonalDoubleThree(int x, int y)    // 역사선 (＼) 확인
        {
            int threeStoneCnt = 1;
            int i, j;

            threeStoneCnt = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↘ 확인
            {
                if (x + i > 18 || y + i > 18)
                    break;

                else if (_omokBoard[x + i, y + i] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x + i, y + i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↖ 확인
            {
                if (x - j < 0 || y - j < 0)
                    break;

                else if (_omokBoard[x - j, y - j] == _omokBoard[x, y])
                    threeStoneCnt++;

                else if (_omokBoard[x - j, y - j] != (int)StoneColor.None)
                    break;
            }

            if (threeStoneCnt == 3 && x + i < 18 && y + i < 18 && x - j > 0 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((_omokBoard[x + i, y + i] != (int)StoneColor.None && _omokBoard[x + i - 1, y + i - 1] != (int)StoneColor.None) || (_omokBoard[x - j, y - j] != (int)StoneColor.None && _omokBoard[x - j + 1, y - j + 1] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }
    }
}

