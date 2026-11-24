using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat
{
    public class NotificationItem
    {
        public Type DataType { get; }

        public object Target { get; private set; }

        public void UpdateTarget(object value)
        {
            Target = value;
        }
    }
}
