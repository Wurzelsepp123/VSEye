using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SPOCK.Receivers
{
    public interface IEyeMovementListener
    {
        void RaiseEyeEvents(Point Position, UInt64 Timestamp, double Reliability);
    }
}