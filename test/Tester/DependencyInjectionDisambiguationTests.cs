﻿
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Orleans.Runtime;
using System.Collections.Generic;

namespace UnitTests.General
{
    [TestCategory("DI")]
    public class DependencyInjectionDisambiguationTests
    {
        [Fact, TestCategory("BVT"), TestCategory("Functional")]
        public void DisambiguateByKeyTest()
        {
            IServiceProvider services = ConfigureServices();
            int actual0 = services.GetServiceByKey<int, IValue<int>>(0).Value;
            Assert.StrictEqual(0, actual0);
            int actual1 = services.GetServiceByKey<int, IValue<int>>(1).Value;
            Assert.StrictEqual(1, actual1);
        }

        [Fact, TestCategory("BVT"), TestCategory("Functional")]
        public void DisambiguateByNameTest()
        {
            IServiceProvider services = ConfigureServices();
            string actualThis = services.GetServiceByName<IValue<string>>("this").Value;
            Assert.Equal("this", actualThis);
            string actualThat = services.GetServiceByName<IValue<string>>("that").Value;
            Assert.Equal("that", actualThat);
        }

        private interface IValue<out TValue>
        {
            TValue Value { get; }
        }

        private class SomeValue<TValue> : IValue<TValue>
        {
            public TValue Value { get; set; }
        }

        private class ValueServiceCollection<TValue> : IKeyedServiceCollection<TValue, IValue<TValue>>
        {
            public IValue<TValue> GetService(IServiceProvider serviceProvider, TValue name)
            {
                return new SomeValue<TValue> { Value = name };
            }

            public IEnumerable<IKeyedService<TValue, IValue<TValue>>> GetServices(IServiceProvider services)
            {
                return Enumerable.Empty<IKeyedService<TValue, IValue<TValue>>>();
            }
        }

        private IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // add services by Key;
            services.AddSingleton<IKeyedServiceCollection<int, IValue<int>>, ValueServiceCollection<int>>();

            // add named services
            services.AddTransient<IKeyedServiceCollection<string, IValue<string>>, ValueServiceCollection<string>>();

            return services.BuildServiceProvider();
        }
    }
}
