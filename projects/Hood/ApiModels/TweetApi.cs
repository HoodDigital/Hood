using Hood.Extensions;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models.Api
{
    public partial class TweetApi
    {
        public Annotation Annotation { get; set; }
        public List<Contributor> Contributors { get; set; }
        public Coordinate Coordinates { get; set; }
        public int Count { get; set; }
        public DateTime CreatedAt { get; set; }
        public ulong CurrentUserRetweet { get; set; }
        public long Cursor { get; set; }
        public Cursors CursorMovement { get; set; }
        public EmbeddedStatus EmbeddedStatus { get; set; }
        public Entities Entities { get; set; }
        public bool ExcludeReplies { get; set; }
        public Entities ExtendedEntities { get; set; }
        public int? FavoriteCount { get; set; }
        public bool Favorited { get; set; }
        public FilterLevel FilterLevel { get; set; }
        public ulong ID { get; set; }
        public bool IncludeContributorDetails { get; set; }
        public bool IncludeEntities { get; set; }
        public bool IncludeMyRetweet { get; set; }
        public bool IncludeRetweets { get; set; }
        public bool IncludeUserEntities { get; set; }
        public string InReplyToScreenName { get; set; }
        public ulong InReplyToStatusID { get; set; }
        public ulong InReplyToUserID { get; set; }
        public string Lang { get; set; }
        public bool Map { get; set; }
        public ulong MaxID { get; set; }
        public StatusMetaData MetaData { get; set; }
        public EmbeddedStatusAlignment OEmbedAlign { get; set; }
        public bool OEmbedHideMedia { get; set; }
        public bool OEmbedHideThread { get; set; }
        public string OEmbedLanguage { get; set; }
        public int OEmbedMaxWidth { get; set; }
        public bool OEmbedOmitScript { get; set; }
        public string OEmbedRelated { get; set; }
        public string OEmbedUrl { get; set; }
        public Place Place { get; set; }
        public bool PossiblySensitive { get; set; }
        public int RetweetCount { get; set; }
        public bool Retweeted { get; set; }
        public Status RetweetedStatus { get; set; }
        public Dictionary<string, string> Scopes { get; set; }
        public string ScreenName { get; set; }
        public ulong SinceID { get; set; }
        public string Source { get; set; }
        public ulong StatusID { get; set; }
        public string Text { get; set; }
        public bool TrimUser { get; set; }
        public bool Truncated { get; set; }
        public string TweetIDs { get; set; }
        public StatusType Type { get; set; }
        public User User { get; set; }
        public ulong UserID { get; set; }
        public List<ulong> Users { get; set; }
        public bool WithheldCopyright { get; set; }
        public List<string> WithheldInCountries { get; set; }
        public string WithheldScope { get; set; }
        public string sCurrentUserRetweet { get; set; }
        public string sStatusID { get; set; }
        public string sSinceID { get; set; }
        public string sUserID { get; set; }
        public string sMaxID { get; set; }
        public string sID { get; set; }
        public string AvatarUrl { get; set; }

        public TweetApi(LinqToTwitter.Status tweet)
        {
            if (tweet == null)
                return;
            tweet.CopyProperties(this);
            sCurrentUserRetweet = tweet.CurrentUserRetweet.ToString();
            sStatusID = tweet.StatusID.ToString();
            sSinceID = tweet.SinceID.ToString();
            sUserID = tweet.UserID.ToString();
            sMaxID = tweet.MaxID.ToString();
            sID = tweet.ID.ToString();
            AvatarUrl = "/images/twitter.jpg";
            if (tweet.Entities.MediaEntities.Count() > 0)
            {
                AvatarUrl = tweet.Entities.MediaEntities[0].MediaUrlHttps;
            }
            if (string.IsNullOrEmpty(ScreenName))
            {
                ScreenName = tweet.User.ScreenName;
            }
            if (string.IsNullOrEmpty(ScreenName))
            {
                ScreenName = tweet.User.ScreenNameResponse;
            }
        }

    }
}
