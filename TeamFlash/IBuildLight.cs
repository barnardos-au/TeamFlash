namespace TeamFlash
{
    public interface IBuildLight
    {
        void Success();
        void Warning();
        void Fail();
        void Off();
    }
}
