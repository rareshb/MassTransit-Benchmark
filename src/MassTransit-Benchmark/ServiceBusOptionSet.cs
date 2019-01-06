namespace MassTransitBenchmark
{
    using System;
#if !NETCOREAPP2_2
    using MassTransit.AzureServiceBusTransport;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceBus.Messaging.Amqp;
#else
    using MassTransit.Azure.ServiceBus.Core;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Primitives;
#endif
    using NDesk.Options;


    class ServiceBusOptionSet :
        OptionSet,
        ServiceBusHostSettings
    {
        readonly TokenScope _tokenScope;
        readonly TimeSpan _tokenTimeToLive;
        string _accessKey;
        string _keyName;

        public ServiceBusOptionSet()
        {
            Add<string>("ns=", "The service bus namespace",
                x =>
                {
                    ServiceUri =
                        new UriBuilder()
                        {
                            Scheme = "sb",
                            Host = $"{x}.servicebus.windows.net",
                            Path = "Benchmark"
                        }.Uri;
                });
            Add<string>("keyname=", "The access key name", x => _keyName = x);
            Add<string>("key=", "The access key", x => _accessKey = x);
            Add<int>("connections=", "The number of connections to configure for the service point manager",
                x => DefaultConnections = x);

            _tokenTimeToLive = TimeSpan.FromDays(1);
            _tokenScope = TokenScope.Namespace;
            TransportType = TransportType.Amqp;
#if !NETCOREAPP2_2
            AmqpTransportSettings = new AmqpTransportSettings
            {
                BatchFlushInterval = TimeSpan.FromMilliseconds(3.0)
            };
            NetMessagingTransportSettings = new NetMessagingTransportSettings
            {
                BatchFlushInterval = TimeSpan.FromMilliseconds(3.0)
            };
#endif
            OperationTimeout = TimeSpan.FromSeconds(60.0);
            RetryMinBackoff = TimeSpan.FromMilliseconds(100.0);
            RetryMaxBackoff = TimeSpan.FromSeconds(20.0);
            RetryLimit = 10;

            DefaultConnections = System.Net.ServicePointManager.DefaultConnectionLimit;
        }

        public int DefaultConnections { get; set; }

        public Uri ServiceUri { get; private set; }

#if !NETCOREAPP2_2
         TokenProvider ServiceBusHostSettings.TokenProvider =>
            TokenProvider.CreateSharedAccessSignatureTokenProvider(_keyName, _accessKey, _tokenTimeToLive, _tokenScope);
#else
         ITokenProvider ServiceBusHostSettings.TokenProvider =>
            Microsoft.Azure.ServiceBus.Primitives.TokenProvider.CreateSharedAccessSignatureTokenProvider(_keyName, _accessKey, _tokenTimeToLive, _tokenScope);
#endif
        public TransportType TransportType { get; }

#if !NETCOREAPP2_2
        public AmqpTransportSettings AmqpTransportSettings { get; }

        public NetMessagingTransportSettings NetMessagingTransportSettings { get; }
#endif

        public TimeSpan OperationTimeout { get; }

        public TimeSpan RetryMinBackoff { get; }

        public TimeSpan RetryMaxBackoff { get; }

        public int RetryLimit { get; }

        public void ShowOptions()
        {
            Console.WriteLine("Service URI: {0}", ServiceUri);
            Console.WriteLine("Key Name: {0}", _keyName);
            Console.WriteLine("Access Key: {0}", new string('*', (_accessKey ?? "default").Length));
            Console.WriteLine("Service Point Manager.Default Connections: {0}", DefaultConnections);
        }
    }
}