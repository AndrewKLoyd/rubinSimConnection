using Google.Protobuf;
using Connection.ConnTypes;
using RubinComm;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Connection
{
    public static class Conversation
    {

        /// <summary>
        /// Event that fires up every time messge arrived.
        /// <para>
        /// !!! Please check the sub value at the callback  !!!
        /// </para>
        /// </summary>
        public static event Action<Sub, IMessage> MessageArrived = (Sub sub, IMessage message) => { DisplaySuccses($"Message arrived: {sub.ToString()}"); };

        /// <summary>
        /// Perform 
        /// </summary>
        /// <value></value>
        public static bool CanTransmit { get; private set; } = false;

        /// <summary>
        /// Use to stop/start displaying mesg on the screen
        /// </summary>
        /// <value></value>
        public static bool ShowHelp { get; set; } = true;
        private static bool _isCreating = false;    // Just a flag to not init more than one connections simultaniously
        private static Dictionary<Pub, Publisher> _pubs = new Dictionary<Pub, Publisher>();
        private static Dictionary<Sub, Subscriber> _subs = new Dictionary<Sub, Subscriber>();
        private static List<IDisposable> _disposables = new List<IDisposable>();


        /// <summary>
        /// Initializing connection asyncly
        /// </summary>
        /// <param name="onInitted">Action that would happaened after initialize</param>
        public static async void InitAsync(Action onInitted)
        {
            if (_isCreating)
            {
                DisplayWarning("You've already trying to init connection!!!\nWait for prev init to finish!!!");
                return;
            }
            if (CanTransmit)
            {
                DisplayWarning("Connection already established\n Do not call init more than once!!!");
                return;
            }

            _isCreating = true;

            await Task.Run(() =>
            {
                // Init pubs
                foreach (Pub pub in Enum.GetValues(typeof(Pub)))
                {
                    Publisher publisher = new Publisher(pub.ToString(), Addresses.GetPubIp(pub));
                    _pubs.Add(pub, publisher);
                    DisplaySuccses($"{pub} publisher binded on topic {pub} Addr:{Addresses.GetPubIp(pub)}");
                    _disposables.Add(publisher);
                }

                // Init subs
                foreach (Sub sub in Enum.GetValues(typeof(Sub)))
                {
                    Subscriber subscriber = new Subscriber(sub.ToString(), Addresses.GetSubIp(sub));
                    DisplaySuccses($"{sub} subscriber inited on topic {sub} Addr:{Addresses.GetSubIp(sub)}\nUse SubOn() and MessageArrived to retrieve msgs");
                    DisplayWarning($"If you want to change subscriber ip change it on: {Addresses.pathToJSONFile}");
                    _subs.Add(sub, subscriber);
                    _disposables.Add(subscriber);
                }
                Thread.Sleep(2000);
            });

            CanTransmit = true;
            onInitted?.Invoke();
            DisplaySuccses("Connection inited!!!");
            _isCreating = false;
        }


        /// <summary>
        /// Publishing message sync to main thread
        /// </summary>
        /// <param name="pub">Type of data you publish</param>
        /// <param name="message"></param>
        public static void Publish(Pub pub, IMessage message)
        {
            if (!CanTransmit)
            {
                DisplayWarning("Connection wasn\'t inited! \n Try use InitAsync first or check CanTransmit value");
                return;
            }
            _pubs[pub].Publish(message);
        }


        /// <summary>
        /// Don't use
        /// Out of order
        /// </summary>
        /// <param name="pub">Type of data you publish</param>
        /// <param name="message"></param>
        public async static void PublishAsync(Pub pub, IMessage message)
        {
            DisplayWarning("That method does not work right now");
            await Task.Run(() => { });
        }

        /// <summary>
        /// Изменяет IP подписчика
        /// </summary>
        /// <param name="sub">Тип подписчика</param>
        /// <param name="ip">Новый IP подписчика</param>
        /// <returns>Результат изменения IP адресса</returns>
        public static bool ChangeSubsriberIp(Sub sub, string ip = "127.0.0.1")
        {
            if (!_subs.ContainsKey(sub)) return false;

            string[] splittedSubIp = Addresses.GetSubIp(sub).Split(':');

            if (splittedSubIp.Length != 3) return false;

            string port = splittedSubIp[2];
            string finAddress = $"tcp://{ip}:{port}";

            Subscriber subscriber = new Subscriber(sub.ToString(), finAddress);
            _subs[sub] = subscriber;
            _disposables.Add(subscriber);
            Addresses.SetSubIp(sub, finAddress);
            SubOn(sub);
            return true;
        }



        /// <summary>
        /// Sub on msg
        /// <para>
        /// Do not forget to add ur callback to MessageArrived event
        /// </para>
        /// </summary>
        /// <param name="sub"></param>
        public static void SubOn(Sub sub)
        {
            if (!CanTransmit)
            {
                DisplayWarning("Connection wasn\'t inited! \n Try use InitAsync first and check CanTransmit value");
                return;
            }
            _subs[sub].RunSub((byte[] data) =>
            {

                switch (sub)
                {
                    //TODO: Modify when new sub was added
                    case Sub.CustomSGRU:
                        MessageArrived?.Invoke(sub, Custom.Parser.ParseFrom(data));
                        break;
                    case Sub.RegulatorComplex:
                        MessageArrived?.Invoke(sub, RegulatorComplex.Parser.ParseFrom(data));
                        break;
                    case Sub.SimInit:
                        MessageArrived?.Invoke(sub, SimInit.Parser.ParseFrom(data));
                        break;
                    case Sub.Mission:
                        MessageArrived?.Invoke(sub, Mission.Parser.ParseFrom(data));
                        break;
                    case Sub.GroupTrajectory:
                        MessageArrived?.Invoke(sub, SGRUGroupTrajectory.Parser.ParseFrom(data));
                        break;
                    case Sub.MathModelSwitch:
                        MessageArrived?.Invoke(sub, AnpaDinamicModel.Parser.ParseFrom(data));
                        break;
                    case Sub.CustomSGRUEvent:
                        MessageArrived?.Invoke(sub, Custom.Parser.ParseFrom(data));
                        break;
                    default:
                        MessageArrived?.Invoke(sub, Custom.Parser.ParseFrom(data));
                        break;
                }
            });
        }


        private static void DisplayWarning(string msg)
        {
            if (!ShowHelp) return;
            Console.WriteLine("\n------------------------------------\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
            Console.WriteLine("\n------------------------------------\n");

        }

        private static void DisplaySuccses(string msg)
        {
            if (!ShowHelp) return;
            Console.WriteLine("\n------------------------------------\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ResetColor();
            Console.WriteLine("\n------------------------------------\n");
        }

        public static void CloseConnections()
        {
            _disposables.ForEach(item => item.Dispose());
            NetMQ.NetMQConfig.Cleanup();
        }
    }
}