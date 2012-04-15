using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Company.NotchCS
{
    static class IPyConstants
    {
        public const string NotchCpuFileExtension = ".cn";
    }

    static class GuidList
    {
        public const string guidNotchCpuProjectPkgString = "03f90a9b-8387-4577-aa0f-98f5163589b0";
        public const string guidNotchCpuProjectCmdSetString = "49df650d-70c7-4234-a85e-874dd6cdcb96";

        public const string guidNotchCpuProjectFactoryString = "AF48B115-53DB-4e4f-A04C-CF2B83C29EE3";

        public const string guidNotchCpuLanguageString = "ae8ce01a-b3ff-4c19-8c80-54669c197f2c";

        public const string libraryManagerGuidString = "F12CCE33-BE39-4605-9B3E-5801E026D8A8";
        public const string libraryManagerServiceGuidString = "3F8033D8-BB88-47ec-9430-03D8B55A393E";

        public static readonly Guid guidNotchCSCmdSet = new Guid(guidNotchCpuProjectCmdSetString);
    }
}
