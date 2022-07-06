using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    static class BarebonesPatchCollection
    {
        // Partially taken from TLAC and PD Loader
        public static readonly Patch[] EssentialPatches = new Patch[] {
            new Patch("Disable USB device presence check", (0x000000014066E820), 0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3),
            new Patch("Free play", (0x0000000140393610), 0xB0, 0x01, 0xC3, 0x90, 0x90, 0x90),
            new Patch("No JVS", (0x140192B70), 0x33, 0xC0, 0xC3, 0x90, 0x90),
            new Patch("Disable NW", (0x00000001406717B1),  0xE9, 0x22, 0x03, 0x00),
            new Patch("Disable DHCP timer", (0x00000001406724E7),  0x00, 0x00 ),
            new Patch("Disable Location server", (0x00000001406732A2), Patch.Nops(2)),
            new Patch("Hide volume control", 0x0000000140A85F10, 0xE0, 0x50),
            new Patch("Hide SE", 0x00000001409A4D60, 0xC0, 0xD3),
            new Patch("Don't hide cursor", 0x000000014019341B, 0x00),

            // No OPD patch by Skyth (Turkish)
            new Patch("Disable OPD 1", 0x1404728C0, 0xC3),
            new Patch("Disable OPD 2", 0x1404728C3, 0x0D),

            new Patch("To Stereo", 0x0000000140A860C0, 0x02),
            new Patch("No input poll", 0x000000014018CBB0, 0xC3),

            new Patch("On Error Resume Next", (0x00000001403F5080), 0xC3),
             new Patch("Disable app error task-1", (0x00000001403F73A7), Patch.Nops(2)),
             new Patch("Disable app error task-2", (0x00000001403F73C3), 0x89, 0xD1, 0x90),
        };

        // Partially taken from TLAC and PD Loader
        public static readonly Patch[] RenderUsagePatches = new Patch[] {
            new Patch("Menu skip 1", 0x0000000140578EA9, 0xE9, 0xF1, 0x00, 0x00, 0x00),
            new Patch("Menu skip 2", 0x0000000140578E9D, 0xC7, 0x47, 0x68, 0x28, 0x00, 0x00, 0x00)
        };
    }
}
