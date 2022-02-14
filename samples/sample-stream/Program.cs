﻿using Confluent.Kafka;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Streamiz.Kafka.Net.Crosscutting;
using System.Threading.Tasks;

namespace sample_stream
{
    /// <summary>
    /// Sample program with a passtrought stream, instanciate and dispose with CTRL+ C console event.
    /// If you want an example with token source passed to startasync, see <see cref="ProgramToken"/> class.
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test-app2";
            config.BootstrapServers = "localhost:9092";
            config.CommitIntervalMs = (long)TimeSpan.FromSeconds(5).TotalMilliseconds;
            config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            config.StateDir = Path.Combine(".");

            config.Logger = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddLog4Net();
            });
            
            StreamBuilder builder = new StreamBuilder();

            var stream1 = builder
                .Stream<string, string>("topic1");

            builder
                .Stream<string, string>("topic2")
                .Join(
                    stream1,
                    (s, v) => $"{s}-{v}",
                    JoinWindowOptions.Of(TimeSpan.FromHours(1)))
                .To("output-join");

            Topology t = builder.Build();
            KafkaStream stream = new KafkaStream(t, config);
            
            Console.CancelKeyPress += (o, e) => stream.Dispose();

            await stream.StartAsync();
        }
    }
}