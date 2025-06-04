namespace Project.Core
{
    public delegate void OnSwipeDetectedDelegate(ISwipeData data);

    public interface ISwipesManager
    {
        public event OnSwipeDetectedDelegate SwipeDetected;
    }
}
