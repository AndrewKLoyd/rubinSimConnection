using NetMQ.Sockets;
using NetMQ;
using Google.Protobuf;
using System;


namespace Connection
{
    internal class Publisher : IDisposable
    {
        private string _topic;
        private string _ip;
        private PublisherSocket _pub;
        private DateTime lastTimeSent;

        /// <summary>
        /// Create a publisher
        /// </summary>
        /// <param name="topic">Topic to publish data</param>
        /// <param name="pubIp">Publisher ip format of "tcp://*:{port}"</param>
        public Publisher(string topic, string pubIp)
        {
            _pub = new PublisherSocket();
            _ip = pubIp;
            _topic = topic;
            _pub.Bind(pubIp);
        }

        public void Publish(IMessage message)
        {
            byte[] data = new byte[message.CalculateSize()];
            CodedOutputStream stream = new CodedOutputStream(data);
            message.WriteTo(stream);
            _pub.SendMoreFrame(_topic).SendFrame(data);
            lastTimeSent = DateTime.Now;

        }

        public void Dispose()
        {
            _pub.Dispose();
        }

    }
}