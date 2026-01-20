using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseSystem.Common.Helper
{
    public class IdHelper
    {
        private long _lastTimestamp = -1L;
        private long _sequence = 0L;
        private readonly long _workerId;
        private readonly long _datacenterId;

        public IdHelper(long workerId, long datacenterId)
        {
            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        public long NextId()
        {
            lock (this)
            {
                var timestamp = CurrentTimeMillis();

                if (timestamp < _lastTimestamp)
                {
                    throw new Exception("时钟回拨异常");
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & 4095; // 12位序列号
                    if (_sequence == 0)
                    {
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - 1288834974657L) << 22)
                       | (_datacenterId << 17)
                       | (_workerId << 12)
                       | _sequence;
            }
        }

        private long CurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private long WaitNextMillis(long lastTimestamp)
        {
            var timestamp = CurrentTimeMillis();
            while (timestamp <= lastTimestamp)
            {
                timestamp = CurrentTimeMillis();
            }
            return timestamp;
        }
    }
}
