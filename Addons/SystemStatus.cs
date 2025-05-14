
using System.ComponentModel;

namespace PURRNext.NextSystems
{
    enum StatusEnum
    {
        ERROR, //The program contains unsolved errors that need to be solved
        RUNNING, //The program is running fine
        HALTED //The program is halted, either because it doesn't have anything to do or was paused by the user.
    }
    public class SystemStatus
    {

    }
}