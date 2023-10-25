using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.DataAccess;

public interface IPcService
{
    void Hibernate();
    void Sleep();
}

public class PcService : IPcService
{
    [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

    public void Sleep() {
        SetSuspendState(false, true, true);
    }

    public void Hibernate() {
        SetSuspendState(true, true, true);
    }
}
