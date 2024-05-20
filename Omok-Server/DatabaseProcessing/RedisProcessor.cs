using SuperSocket.SocketBase.Logging;
using System.Threading.Tasks.Dataflow;

namespace OmokServer
{
    public class RedisProcessor
    {
        ILog _mainLogger;
        bool _isThreadRunning = false;
        List<System.Threading.Thread> _processThreadList = new();

        BufferBlock<OmokBinaryRequestInfo> _dbPktBuffer = new();
        RedisHandler _dbWorkHandler = new();
        Dictionary<int, Func<OmokBinaryRequestInfo, RedisDb, OmokBinaryRequestInfo>> _dbWorkHandlerMap = new();

        string _connectionString;
        string _userRoomKey;

        Action<OmokBinaryRequestInfo> DistributeAction;

        public void InitAndStartProcessing(ILog logger, RedisOption redisOption, Action<OmokBinaryRequestInfo> distributeAction) 
        {
            _mainLogger = logger;
            
            _connectionString = redisOption.RedisConnectionString;
            _userRoomKey = redisOption.UserRoomKey;
            DistributeAction = distributeAction;

            _isThreadRunning = true;

            var threadCount = redisOption.RedisThreadCount;
            for (int i = 0; i < threadCount; i++)
            {
                var processThread = new System.Threading.Thread(this.Process);
                processThread.Start();

                _processThreadList.Add(processThread);
            }
            RegistDbHandlerMap();
        }

        void RegistDbHandlerMap()
        {
            _dbWorkHandler.RegistDbHandlerMap(_dbWorkHandlerMap);
        }

        public void InsertPakcet(OmokBinaryRequestInfo requstPacket)
        {
            _dbPktBuffer.Post(requstPacket);
        }

        async void Process() 
        {
            var db = new RedisDb(_connectionString, _userRoomKey);
            while (_isThreadRunning)
            {
                try
                {
                    var packet = _dbPktBuffer.Receive();

                    var header = new MemoryPackPacketHeadInfo();
                    header.Read(packet.Data);


                    if (_dbWorkHandlerMap.ContainsKey(header.Id))
                    {
                        var result = _dbWorkHandlerMap[header.Id](packet, db);
                        DistributeAction(result);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("세션 번호 {0}, DBWorkID {1}", packet.SessionID, header.Id);
                    }
                }
                catch (Exception ex)
                {
                    _mainLogger.Error(ex.ToString());
                }
            }
        }
    }
}
