using System;
using System.Collections.Generic;

namespace Ingvilt.Core {
    public class DependencyInjectionContainer {
        private Dictionary<Type, object> instances;

        public static readonly DependencyInjectionContainer Container = new DependencyInjectionContainer();

        private T CreateInstance<T>() {
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);
            instances[type] = instance;
            return (T)instance;
        }

        public DependencyInjectionContainer() {
            instances = new Dictionary<Type, object>();
        }

        public void RegisterInstance<T>(T instance) {
            var type = typeof(T);
            instances[type] = instance;
        }

        public T Resolve<T>() {
            var type = typeof(T);
            if (instances.ContainsKey(type)) {
                return (T)instances[type];
            }

            return CreateInstance<T>();
        }
    }
}
