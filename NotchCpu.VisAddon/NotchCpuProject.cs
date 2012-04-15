using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;

namespace Company.NotchCS
{
    [CLSCompliant(false)]
    [ComVisible(true)]
    [Guid("7B61EA22-333F-4b4e-A000-FB847CDD4A99")]
    class NotchCpuProject : ProjectNode
    {
        public override Guid ProjectGuid
        {
            get { return typeof(NotchCpuProjectFactory).GUID; }
        }

        public override string ProjectType
        {
            get { return this.GetType().Name; }
        }
    }
}
