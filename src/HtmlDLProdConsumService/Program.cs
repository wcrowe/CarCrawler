﻿using Atlas;
using Autofac;
using System;
using NLog;
namespace HtmlDLProdConsumService
{
    public class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                var configuration =
                    Host.UseAppConfig<DLService>()
                        .AllowMultipleInstances()
                        .WithRegistrations(b => b.RegisterModule(new MyAutofacModule()))
                        .WithArguments(args);

                Host.Start(configuration);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }

}
