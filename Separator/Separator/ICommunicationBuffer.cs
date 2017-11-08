using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Separator
{
    public interface ICommunicationBuffer
    {
        void ReceiveData(byte[] Data);
        void SendCommand();

        PortHandler Handler { get; }
    }
}
