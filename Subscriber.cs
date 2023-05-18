using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;
using System;

namespace Connection
{
    internal class Subscriber : IDisposable
    {
        private SubscriberSocket _sub;
        public event Action<byte[]> MessageArrived;
        public Subscriber(string topic, string ip)
        {
            MessageArrived = (byte[] data) => { };
            _sub = new SubscriberSocket();
            _sub.Connect(ip);
            _sub.Subscribe(topic);


        }


        public async void RunSub(Action<byte[]> onMsgArrived)
        {
            byte[] data = new byte[0];
            await Task.Run(() =>
            {
                string topic = _sub.ReceiveFrameString();
                Console.WriteLine($"Message on topic arrived: {topic}");
                data = _sub.ReceiveFrameBytes();
            });

            onMsgArrived?.Invoke(data);
            if (onMsgArrived == null) return;
            RunSub(onMsgArrived);
        }

        public void Dispose()
        {
            _sub.Dispose();
        }
    }
}