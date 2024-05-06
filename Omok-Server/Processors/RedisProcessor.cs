using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Omok_Server
{
    public class RedisProcessor
    {
        ILog _mainLogger;
        bool _isThreadRunning = false;
        List<System.Threading.Thread> _processThreadList = new();

        BufferBlock<OmokBinaryRequestInfo> _dbPktBuffer = new();
        RedisHandler _dbWorkHandler = new();
        string _connectionString;

        Dictionary<int, Func<OmokBinaryRequestInfo, RedisDb, Task<OmokBinaryRequestInfo>>> _dbWorkHandlerMap = new();

        Action<OmokBinaryRequestInfo> DistributeAction;

        public void InitAndStartProcessing(int threadCount, string connectionString, Action<OmokBinaryRequestInfo> distributeAction, ILog logger) 
        {
            _mainLogger = logger;
            _mainLogger.Info("DB Init Start");
            DistributeAction = distributeAction;
            _connectionString = connectionString;

            _isThreadRunning = true;

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

        async void Process() 
        {
            // ::TODO:: DB 커넥션 생성
            var db = new RedisDb(_connectionString);
            while (_isThreadRunning)
            {
                try
                {
                    var packet = _dbPktBuffer.Receive();

                    var header = new MemoryPackPacketHeadInfo();
                    header.Read(packet.Data);


                    if (_dbWorkHandlerMap.ContainsKey(header.Id))
                    {
                        var result = await _dbWorkHandlerMap[header.Id](packet, db);
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
