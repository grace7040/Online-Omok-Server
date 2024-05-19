using SuperSocket.SocketBase.Logging;
using System.Threading.Tasks.Dataflow;

namespace Omok_Server
{
    public class MySqlProcessor
    {
        ILog _mainLogger;
        bool _isThreadRunning = false;
        List<System.Threading.Thread> _processThreadList = new();

        BufferBlock<OmokBinaryRequestInfo> _dbPktBuffer = new();
        MySqlHandler _dbWorkHandler = new();
        Dictionary<int, Func<OmokBinaryRequestInfo, MySqlDb, OmokBinaryRequestInfo>> _dbWorkHandlerMap = new();

        string _connectionString;

        Action<OmokBinaryRequestInfo> DistributeAction;

        public void InitAndStartProcessing(ILog logger, DbOption dbOption, Action<OmokBinaryRequestInfo> distributeAction) 
        {
            _mainLogger = logger;
            _mainLogger.Info("DB Init Start");
            DistributeAction = distributeAction;
            _connectionString = dbOption.DbConnectionString;

            _isThreadRunning = true;
                
            var threadCount = dbOption.DbThreadCount;
            for (int i = 0; i < threadCount; i++)
            {
                var processThread = new System.Threading.Thread(this.Process);
                processThread.Start();

                _processThreadList.Add(processThread);
            }
            RegistDbHandler();
            _mainLogger.Info("DB Init Success");
        }

        void RegistDbHandler()
        {
            _dbWorkHandler.RegistDbHandler(_dbWorkHandlerMap);
        }

        public void InsertPakcet(OmokBinaryRequestInfo requstPacket)
        {
            _dbPktBuffer.Post(requstPacket);
        }

        void Process() 
        {
            var db = new MySqlDb(_connectionString);
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
