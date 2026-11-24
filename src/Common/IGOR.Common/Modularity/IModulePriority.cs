namespace IGOR.Common.Modularity
{
    public interface IModulePriority
    {
        ModuleType ModuleType { get; }
        int Priority { get; }
    }
}