using System.ComponentModel.DataAnnotations;
using Hood.BaseTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class ConnectAccountModel : SaveableModel
    {
        public string LocalPicture { get; set; }
        public string RemotePicture { get; set; }

        [FromForm(Name = "returnUrl")]
        public string ReturnUrl { get; set; }
    }
    public class DisconnectAccountModel : SaveableModel
    {
        public string LocalPicture { get; set; }
        public string RemotePicture { get; set; }

        [FromForm(Name = "accountId")]
        public string AccountId { get; set; }
    }
}
