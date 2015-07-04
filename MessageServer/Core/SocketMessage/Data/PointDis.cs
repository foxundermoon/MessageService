using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.SocketMessage.Data
{
    public class PointDis : IComparable<PointDis>
    {
        public bool CanUse { get; set; }
        public int Index { get; set; }
        public double Distance { get; set; }
        public Int32 CompareTo(PointDis other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}
