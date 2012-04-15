using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace NotchCpu.Emulator
{
    public static class Mef
    {
        private static object _MefLock = new object();
        private static CompositionContainer _Container;

        public static T GetExportedValue<T>()
        {
            InitMef();
            return _Container.GetExportedValue<T>();
        }

        private static void InitMef()
        {
            lock (_MefLock)
            {
                if (_Container != null)
                    return;

                var catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()), new AssemblyCatalog(Assembly.GetEntryAssembly()));
                _Container = new CompositionContainer(catalog);
            }
        }

        public static IEmulator GetEmulator()
        {
            return GetExportedValue<IEmulator>();
        }
    }
}
