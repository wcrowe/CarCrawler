﻿
using Atlas;
using Autofac;
using Quartz;
using Quartz.Impl;

namespace HtmlDLProdConsumService
{
    public class MyAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            LoadQuartz(builder);
            LoadServices(builder);
        }

        private static void LoadQuartz(ContainerBuilder builder)
        {
            builder.Register(c => new StdSchedulerFactory().GetScheduler()).As<IScheduler>().InstancePerLifetimeScope();
            builder.Register(c => new AutofacJobListener(ContainerProvider.Instance)).As<IJobListener>();
        }

        private static void LoadServices(ContainerBuilder builder)
        {
            builder.RegisterType<DlService>().As<IAmAHostedProcess>().PropertiesAutowired();
        }
    }
}
