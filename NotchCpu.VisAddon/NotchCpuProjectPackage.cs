using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Package;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;

namespace Company.NotchCS
{

    [ProvideProjectFactory(typeof(NotchCpuProjectFactory), "NotchCpu", "NotchCpu Project Files (*.nproj);*.nproj", "nproj", "nproj", @"..\..\Templates\Projects", LanguageVsTemplate = "NotchCpu")]
    [ProvideMSBuildTargets("NotchCpuCompilerTask", @"$PackageFolder$\NotchCpu.targets")]
    //[ProvideLanguageExtension(typeof(NotchCpuLanguageExtension), ".cn")]
    public class NotchCpuProjectPackage : ProjectPackage
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new NotchCpuProjectFactory(this));
        }

        public override string ProductUserContext
        {
            get { throw new NotImplementedException(); }
        }
    }


}
