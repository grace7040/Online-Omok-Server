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
        //SendFunc, 플레이어 리스트, 패킷 매니저
        const int boardSize = 19;

        public int[,] OmokBoard = new int[boardSize, boardSize];

        public bool IsGameEnd { get; private set; } = false;
        public bool isDoubleThree { get; private set; } = false;

        public int previousX { get; private set; } = -1;
        public int previousY { get; private set; } = -1;

        public int currentX { get; private set; } = -1;
        public int currentY { get; private set; } = -1;

        StoneColor currStoneColor;

        Action<StoneColor> GameEndAction;

        Func<string, byte[], bool> SendFunc;

        Dictionary<string, StoneColor> _userStoneColorDict;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        public OmokGame(Dictionary<string, StoneColor> userStoneColorDict, Func<string, byte[], bool> sendFunc, Action<StoneColor> gameEndAction)
        {
            _userStoneColorDict = userStoneColorDict;
            SendFunc = sendFunc;
            GameEndAction = gameEndAction;

            StartGame();
        }

        public void StartGame()
        {
            Array.Clear(OmokBoard, 0, boardSize * boardSize);

            previousX = previousY = -1;
            currentX = currentY = -1;
            isDoubleThree = false;
            currStoneColor = StoneColor.Black;
            IsGameEnd = false;

            string firstTurnPlayer = GetSessionByStoneColor(StoneColor.Black);
            NotifyPutStoneToClient(firstTurnPlayer, null);
        }

        string GetSessionByStoneColor(StoneColor stoneColor)
        {
            return _userStoneColorDict.FirstOrDefault(entry =>
                       EqualityComparer<StoneColor>.Default.Equals(entry.Value, stoneColor)).Key;
        }
        public int GetOmokBoard(int x, int y)
        {
            return OmokBoard[x, y];
        }

        public void PutStone(int x, int y)
        {
            CheckDoubleThree(x, y);
            if (isDoubleThree)
            {
                ResponsePutStone(GetSessionByStoneColor(currStoneColor), ErrorCode.PUT_STONE_FAIL_INVALID_POSITION);
                isDoubleThree = false;
                return;
            }
            if (OmokBoard[x, y] != (int)StoneColor.None)
            {
                ResponsePutStone(GetSessionByStoneColor(currStoneColor), ErrorCode.PUT_STONE_FAIL_INVALID_POSITION);
                return;
            }

            OmokBoard[x, y] = (int)currStoneColor;

            previousX = currentX;
            previousY = currentY;

            currentX = x;
            currentY = y;

            ResponsePutStone(GetSessionByStoneColor(currStoneColor), ErrorCode.NONE);
            ChangeTurn();
            오목확인(x, y);
        }

        void ChangeTurn()
        {
            if (currStoneColor == StoneColor.Black)
                currStoneColor = StoneColor.White;
            else if (currStoneColor == StoneColor.White)
                currStoneColor = StoneColor.Black;

            NotifyPutStoneToClient(GetSessionByStoneColor(currStoneColor), new Tuple<int,int> (currentX, currentY));
        }

        public bool IsUserTurn(string sessionId)
        {
            return currStoneColor == _userStoneColorDict[sessionId];
        }

        public void 오목확인(int x, int y)
        {
            if (가로확인(x, y) == 5)        // 같은 돌 개수가 5개면 (6목이상이면 게임 계속) 
            {
                //승리효과음.Play();
                //MessageBox.Show((StoneColor)OmokBoard[x, y] + " 승");
                IsGameEnd = true;
            }

            else if (세로확인(x, y) == 5)
            {
                //승리효과음.Play();
                //MessageBox.Show((StoneColor)OmokBoard[x, y] + " 승");
                IsGameEnd = true;
            }

            else if (사선확인(x, y) == 5)
            {
                //승리효과음.Play();
                //MessageBox.Show((StoneColor)OmokBoard[x, y] + " 승");
                IsGameEnd = true;
            }

            else if (역사선확인(x, y) == 5)
            {
                //승리효과음.Play();
                //MessageBox.Show((StoneColor)OmokBoard[x, y] + " 승");
                IsGameEnd = true;
            }

            if (IsGameEnd)
            {
                //승자 돌 색깔: OmokBoard[x,y]
                GameEndAction((StoneColor)OmokBoard[x, y]);
            }

        }

        void NotifyPutStoneToClient(string sessionID, Tuple<int, int>? position)
        {
            var notifyPutStone = new PKTNtfPutStone()
            {
                Position = position
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyPutStone, PacketId.NTF_PUT_STONE);

            SendFunc(sessionID, sendPacket);
        }

        public void ResponsePutStone(string sessionID, ErrorCode errorCode)
        {
            var resPutStone = new PKTResPutStone()
            {
                Result = (short)errorCode
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resPutStone, PacketId.RES_PUT_STONE);
            SendFunc(sessionID, sendPacket);
        }


        void NotifyTurnOver(string sessionID)
        {
            //돌 놓을 때 턴 확인하도록
            //이 패킷 받은 경우 턴 = false
            //NotifyPutStone 패킷 받은 경우 턴 = true
            //이 함수는 서버에서 타이머 종료시 호출한다.
        }

        int 가로확인(int x, int y)      // ㅡ 확인
        {
            int 같은돌개수 = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && OmokBoard[x + i, y] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && OmokBoard[x - i, y] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            return 같은돌개수;
        }

        int 세로확인(int x, int y)      // | 확인
        {
            int 같은돌개수 = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (y + i <= 18 && OmokBoard[x, y + i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (y - i >= 0 && OmokBoard[x, y - i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            return 같은돌개수;
        }

        int 사선확인(int x, int y)      // / 확인
        {
            int 같은돌개수 = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && y - i >= 0 && OmokBoard[x + i, y - i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && y + i <= 18 && OmokBoard[x - i, y + i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            return 같은돌개수;
        }

        int 역사선확인(int x, int y)     // ＼ 확인
        {
            int 같은돌개수 = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (x + i <= 18 && y + i <= 18 && OmokBoard[x + i, y + i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (x - i >= 0 && y - i >= 0 && OmokBoard[x - i, y - i] == OmokBoard[x, y])
                    같은돌개수++;

                else
                    break;
            }

            return 같은돌개수;
        }

        void CheckDoubleThree(int x, int y)     // 33확인
        {
            int 삼삼확인 = 0;

            삼삼확인 += 가로삼삼확인(x, y);
            삼삼확인 += 세로삼삼확인(x, y);
            삼삼확인 += 사선삼삼확인(x, y);
            삼삼확인 += 역사선삼삼확인(x, y);

            if (삼삼확인 >= 2)
                isDoubleThree = true;

            else
                isDoubleThree = false;
        }

        int 가로삼삼확인(int x, int y)    // 가로 (ㅡ) 확인
        {
            int 돌3개확인 = 1;
            int i, j;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 → 확인
            {
                if (x + i > 18)
                    break;

                else if (OmokBoard[x + i, y] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x + i, y] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ← 확인
            {
                if (x - j < 0)
                    break;

                else if (OmokBoard[x - j, y] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x - j, y] != (int)StoneColor.None)
                    break;
            }

            if (돌3개확인 == 3 && x + i < 18 && x - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((OmokBoard[x + i, y] != (int)StoneColor.None && OmokBoard[x + i - 1, y] != (int)StoneColor.None) || (OmokBoard[x - j, y] != (int)StoneColor.None && OmokBoard[x - j + 1, y] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        private int 세로삼삼확인(int x, int y)    // 세로 (|) 확인
        {
            int 돌3개확인 = 1;
            int i, j;

            돌3개확인 = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↓ 확인
            {
                if (y + i > 18)
                    break;

                else if (OmokBoard[x, y + i] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x, y + i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↑ 확인
            {
                if (y - j < 0)
                    break;

                else if (OmokBoard[x, y - j] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x, y - j] != (int)StoneColor.None)
                    break;
            }

            if (돌3개확인 == 3 && y + i < 18 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((OmokBoard[x, y + i] != (int)StoneColor.None && OmokBoard[x, y + i - 1] != (int)StoneColor.None) || (OmokBoard[x, y - j] != (int)StoneColor.None && OmokBoard[x, y - j + 1] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        int 사선삼삼확인(int x, int y)    // 사선 (/) 확인
        {
            int 돌3개확인 = 1;
            int i, j;

            돌3개확인 = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↗ 확인
            {
                if (x + i > 18 || y - i < 0)
                    break;

                else if (OmokBoard[x + i, y - i] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x + i, y - i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↙ 확인
            {
                if (x - j < 0 || y + j > 18)
                    break;

                else if (OmokBoard[x - j, y + j] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x - j, y + j] != (int)StoneColor.None)
                    break;
            }

            if (돌3개확인 == 3 && x + i < 18 && y - i > 0 && x - j > 0 && y + j < 18)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((OmokBoard[x + i, y - i] != (int)StoneColor.None && OmokBoard[x + i - 1, y - i + 1] != (int)StoneColor.None) || (OmokBoard[x - j, y + j] != (int)StoneColor.None && OmokBoard[x - j + 1, y + j - 1] != (int)StoneColor.None))
                {
                    return 0;
                }

                else
                    return 1;
            }

            return 0;
        }

        int 역사선삼삼확인(int x, int y)    // 역사선 (＼) 확인
        {
            int 돌3개확인 = 1;
            int i, j;

            돌3개확인 = 1;

            for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↘ 확인
            {
                if (x + i > 18 || y + i > 18)
                    break;

                else if (OmokBoard[x + i, y + i] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x + i, y + i] != (int)StoneColor.None)
                    break;
            }

            for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↖ 확인
            {
                if (x - j < 0 || y - j < 0)
                    break;

                else if (OmokBoard[x - j, y - j] == OmokBoard[x, y])
                    돌3개확인++;

                else if (OmokBoard[x - j, y - j] != (int)StoneColor.None)
                    break;
            }

            if (돌3개확인 == 3 && x + i < 18 && y + i < 18 && x - j > 0 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
            {
                if ((OmokBoard[x + i, y + i] != (int)StoneColor.None && OmokBoard[x + i - 1, y + i - 1] != (int)StoneColor.None) || (OmokBoard[x - j, y - j] != (int)StoneColor.None && OmokBoard[x - j + 1, y - j + 1] != (int)StoneColor.None))
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

