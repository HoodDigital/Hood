using Hood.Interfaces;

namespace Hood.Models
{
    public interface IAvatar
    {
        string AvatarJson { get; set; }
        IMediaObject Avatar { get; set; }
    }
}