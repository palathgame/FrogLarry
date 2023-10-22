using System.Collections.Generic;
using UnityEngine;

public class InternalRepeaterScript 
{
    private List<string> commands;
    private List<InternalRepeaterScript>  internalRepeaters;
    private int repeat;

    public InternalRepeaterScript(List<string> commands, List<InternalRepeaterScript> internalRepeaters, int repeat)
    {
        this.commands = commands;
        this.internalRepeaters = internalRepeaters;
        this.repeat = repeat;
    }

    public List<string> GetCommands() {
   
        return commands;
    }
    public List<InternalRepeaterScript> GetInternalRepeaters() {  return internalRepeaters; }
    public int GetRepeat() { return repeat; }

}
