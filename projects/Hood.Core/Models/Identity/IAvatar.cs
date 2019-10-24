namespace Hood.Interfaces
{
    public interface IAvatar
    {
        string AvatarJson { get; set; }
        IMediaObject Avatar { get; set; }
    }
}