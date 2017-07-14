using Hood.Extensions;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models.Api
{
    public partial class TweetApi
    {
        public string StatusId { get; set; }
        public string DisplayName { get; set; }
        public string Handle { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AvatarUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool Retweet { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }

        public TweetApi(LinqToTwitter.Status tweet)
        {
            if (tweet == null)
                return;
            StatusId = tweet.StatusID.ToString();
            Text = tweet.Text;
            Html = tweet.Text.ParseURL().ParseUsername().ParseHashtag();
            Retweet = tweet.RetweetedStatus != null && tweet.RetweetedStatus.StatusID != 0;
            CreatedAt = tweet.CreatedAt;

            AvatarUrl = "/images/twitter.jpg";
            if (tweet.Entities.MediaEntities.Count() > 0)
            {
                AvatarUrl = tweet.Entities.MediaEntities[0].MediaUrlHttps;
            }
            else
            {
                if (Retweet)
                {
                    if (tweet.RetweetedStatus.Entities.MediaEntities.Count() > 0)
                    {
                        AvatarUrl = tweet.Entities.MediaEntities[0].MediaUrlHttps;
                    }
                    else
                        AvatarUrl = tweet.RetweetedStatus.User.ProfileBannerUrl;
                }
                else
                {
                    AvatarUrl = tweet.User.ProfileBannerUrl;
                }
            }

            if (Retweet)
            {
                Handle = tweet.RetweetedStatus.ScreenName;
                ProfileImageUrl = tweet.RetweetedStatus.User.ProfileImageUrlHttps;
            }
            else
            {
                Handle = tweet.ScreenName;
                ProfileImageUrl = tweet.User.ProfileImageUrlHttps;
            }
        }

    }
}
