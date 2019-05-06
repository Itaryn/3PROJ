using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KittyCoins.Models
{
    public class EventArgsMessage : EventArgs
    {
        public string Message { get; set; }
        public DateTime MessageTime { get; set; }

        public EventArgsMessage(string message)
        {
            Message = message;
            MessageTime = DateTime.UtcNow;
        }
    }
    public class EventArgsObject : EventArgs
    {
        public object Object { get; set; }
        public DateTime MessageTime { get; set; }

        public EventArgsObject(object obj)
        {
            Object = obj;
            MessageTime = DateTime.UtcNow;
        }
    }
}
