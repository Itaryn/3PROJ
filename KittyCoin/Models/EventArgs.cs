using System;

namespace KittyCoin.Models
{
    /// <summary>
    /// Class to expand EventArgs with string and datetime
    /// </summary>
    /// <see cref="EventArgs"/>
    public class EventArgsMessage : EventArgs
    {
        /// <summary>
        /// The Message string
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The Creation DateTime of the EventArgs
        /// </summary>
        public DateTime MessageTime { get; set; }

        /// <summary>
        /// Constructor who take a message in argument and set the MessageTime to actual UTC
        /// </summary>
        /// <param name="message"></param>
        public EventArgsMessage(string message)
        {
            Message = message;
            MessageTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Class to expand EventArgs with object and datetime
    /// </summary>
    /// <see cref="EventArgs"/>
    public class EventArgsObject : EventArgs
    {
        /// <summary>
        /// The Object contained in the message
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        /// The Creation DateTime of the EventArgs
        /// </summary>
        public DateTime MessageTime { get; set; }

        /// <summary>
        /// Constructor who take an object in argument and set the MessageTime to actual UTC
        /// </summary>
        /// <param name="obj"></param>
        public EventArgsObject(object obj)
        {
            Object = obj;
            MessageTime = DateTime.UtcNow;
        }
    }
}
