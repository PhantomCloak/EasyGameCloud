using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.Abstractions
{
    public interface IGenericPool<T>
    {
        public T Get();
        public void Pool(T obj);
    }

    public class ChannelPool : IGenericPool<IModel>
    {
        private Queue<IModel> _pool;

        public ChannelPool(IConnection connection, int count)
        {
            _pool = new Queue<IModel>();
            for (int i = 0; i < count; i++)
            {
                Pool(connection.CreateModel());
            }
        }

        public IModel Get()
        {
            IModel pooledObj;
    
            lock (_pool)
            {
                pooledObj = _pool.Dequeue();
            }

            return pooledObj;
        }

        public void Pool(IModel obj)
        {
            lock (_pool)
            {
                _pool.Enqueue(obj);
            }
        }
    }

    public static class SimpleEventBus<T>
    {
        public delegate void SimpleEventDelegate(object sender, T arg);

        private static readonly Dictionary<Type, SimpleEventDelegate> EventDictionary;

        private static IConnection Connection { get; }
        private static IGenericPool<IModel> channelPool { get; set; }

        static SimpleEventBus()
        {
            if (Connection == null)
            {
                Connection = (new ConnectionFactory {HostName = "localhost"}).CreateConnection();
            }

            channelPool = new ChannelPool(Connection, Environment.ProcessorCount * 2);
            EventDictionary = new Dictionary<Type, SimpleEventDelegate>();
        }

        public static void Subscribe(string channelName)
        {
            var channel = channelPool.Get();

            channel.QueueDeclare(queue: "worker",
                durable: false,
                exclusive: false,
                autoDelete: true,
                arguments: null);
            
            channel.ExchangeDeclare(channelName, "direct",
                durable: false,
                autoDelete: true,
                arguments: null);
            
            channel.QueueBind("worker", channelName, "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonStr = Encoding.UTF8.GetString(body);

                var inst = EventDictionary[typeof(T)];

                var fo = JsonConvert.DeserializeObject<T>(jsonStr);
                inst.DynamicInvoke(model, fo);
            };
            channel.BasicConsume("worker", true, consumer);

            channelPool.Pool(channel);
        }

        private static void UnSubscribe()
        {
            EventDictionary.Remove(typeof(T));
        }
        public static void Publish(T obj, string channelName)
        {
            var channel = channelPool.Get();

            var foo = JsonConvert.SerializeObject(obj);
            var baa = Encoding.UTF8.GetBytes(foo);

            channel.BasicPublish(channelName, "", true, null, baa);

            channelPool.Pool(channel);
        }

        public static SimpleEventDelegate GetHandler
        {
            get
            {
                if (EventDictionary.ContainsKey(typeof(T)))
                    return EventDictionary[typeof(T)];

                return null;
            }
            set => EventDictionary.Add(typeof(T), value);
        }
    }
}