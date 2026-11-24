namespace IGOR.Interface.Game
{
    public interface IGameAi
    {
        TNode Best<TNode>(TNode root, uint searchDepth) where TNode : INode;
    }
}