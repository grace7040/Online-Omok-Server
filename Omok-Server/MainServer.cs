using Omok_Server;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

namespace Omok_Server
{
    //네트워크 I/O 처리를 위한 서버 클래스 (비동기 처리)
    public class MainServer : AppServer<NetworkSession, OmokBinaryRequestInfo>, IHostedService
    {
        public ILog MainLogger;

        PacketProcessor _packetProcessor = new();
        PacketManager<MemoryPackBinaryPacketDataCreator> _packetManager = new();
        //MySqlProcessor _mySqlProcessor = new();
        RedisProcessor _redisProcessor = new();
        MySqlProcessor _mySqlProcessor = new();
        RoomManager _roomMgr = new();
        UserManager _userMgr = new();
        HeartBeatManager _heartBeatMgr = new();
        

        ServerOption _serverOption;     //appsettings의 서버 설정
        IServerConfig _networkConfig;   //SuperSocket의 서버 설정

        private readonly IHostApplicationLifetime _appLifetime;

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig)
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, OmokBinaryRequestInfo>())
        {
            _appLifetime = appLifetime;
            _serverOption = serverConfig.Value;

            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, OmokBinaryRequestInfo>(OnPacketReceived);

        }

        // 호스팅 서비스 시작 시 호출
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnAppStarted);
            _appLifetime.ApplicationStopped.Register(OnAppStopped);

            return Task.CompletedTask;
        }

        // 호스팅 서비스 종료 시 호출
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        void OnAppStarted()
        {
            //Config 초기화 및 서버 생성
            InitConfig();
            CreateServer();
            StartServer();
        }

        void OnAppStopped()
        {
            base.Stop();
        }

        void OnConnected(NetworkSession session)
        {
            MainLogger.Info($"OnConnected(): {session.SessionID} 접속 요청");
            var innerPacket = _packetManager.MakeInNTFConnectOrDisConnectClientPacket(true, session.SessionID);
            Distribute(innerPacket);
        }

        void OnClosed(NetworkSession session, CloseReason closeReason)
        {
            MainLogger.Info($"OnClosed(): {session.SessionID} 접속 해제");
            var innerPacket = _packetManager.MakeInNTFConnectOrDisConnectClientPacket(false, session.SessionID);
            Distribute(innerPacket);
        }

        void OnPacketReceived(NetworkSession session, OmokBinaryRequestInfo requestPacket)
        {
            //MainLogger.Info($"OnPacketReceived(): {session.SessionID} Sent Packet. Packet Body Length: {requestPacket.Data.Length}");
            requestPacket.SessionID = session.SessionID;

            Distribute(requestPacket);
        }

        void InitConfig()
        {
            _networkConfig = new ServerConfig
            {
                Port = _serverOption.Port,
                Ip = "Any",
                MaxConnectionNumber = _serverOption.MaxConnectionNumber,
                Mode = SocketMode.Tcp,
                Name = _serverOption.Name
            };
        }

        void CreateServer()
        {
            try
            {
                bool bResult = Setup(new RootConfig(), _networkConfig, logFactory: new NLogLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패");
                    return;
                }
                else
                {
                    //서버 생성시 로거 주입
                    MainLogger = base.Logger;
                }

                CreateAndInitComponents();

                Console.WriteLine($"서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }
        }

        void StartServer()
        {
            var result = base.Start();

            if (result)
            {
                MainLogger.Info("서버 시작");
            }
            else
            {
                MainLogger.Error("서버 시작 실패");
            }
        }

        void CreateAndInitComponents()
        {
            var maxUserCount = _serverOption.RoomMaxCount * _serverOption.RoomMaxUserCount;
            _userMgr.Init(maxUserCount, this.SendData, this.DistributeRedisDBWork, this.DistributeMySqlDBWork, MainLogger);
            _userMgr.CreateUsers();

            _roomMgr.Init(this.SendData, this.Distribute, this.DistributeMySqlDBWork, _userMgr.UpdateUsersGameData, MainLogger, _serverOption);
            _roomMgr.CreateRooms(_serverOption);

            _heartBeatMgr.Init(this.SendData, this.Distribute, MainLogger, _userMgr, _serverOption.CheckUserCount, maxUserCount, _serverOption.HeartBeatInterval);
            _heartBeatMgr.StartTimer();

            _packetProcessor.InitAndStartProcessing(_serverOption, _userMgr, _roomMgr, _heartBeatMgr, this.SendData, MainLogger);
            _mySqlProcessor.InitAndStartProcessing(_serverOption.DbThreadCount, _serverOption.DbConnectionString, this.Distribute, MainLogger);
            _redisProcessor.InitAndStartProcessing(_serverOption.RedisThreadCount, _serverOption.RedisConnectionString, this.Distribute, MainLogger);
        }

        public bool SendData(string sessionID, byte[] sendData)
        {
            var session = GetSessionByID(sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        void Distribute(OmokBinaryRequestInfo requestPacket)
        {
            _packetProcessor.InsertPakcet(requestPacket);
            
        }

        void DistributeMySqlDBWork(OmokBinaryRequestInfo requestPacket)
        {
            _mySqlProcessor.InsertPakcet(requestPacket);
        }

        void DistributeRedisDBWork(OmokBinaryRequestInfo requestPacket)
        {
            _redisProcessor.InsertPakcet(requestPacket);
        }

    }
}

public class NetworkSession : AppSession<NetworkSession, OmokBinaryRequestInfo>
{
}