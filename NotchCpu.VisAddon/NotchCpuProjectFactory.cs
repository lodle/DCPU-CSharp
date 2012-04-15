using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Company.NotchCS
{
    /// <summary>
    /// Creates Python Projects
    /// </summary>
    [Guid(GuidList.guidNotchCpuProjectFactoryString)]
    public class NotchCpuProjectFactory : ProjectFactory
    {
        /// <summary>
        /// Constructor for PythonProjectFactory
        /// </summary>
        /// <param name="package">the package who created this object</param>
        public NotchCpuProjectFactory(NotchCpuProjectPackage package)
            : base(package)
        {
        }

        /// <summary>
        /// Creates the Python Project node
        /// </summary>
        /// <returns>the new instance of the Python Project node</returns>
        protected override ProjectNode CreateProject()
        {
            NotchCpuProject project = new NotchCpuProject();

            project.SetSite((IOleServiceProvider)(((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider))));
            return project;
        }
    }
}
