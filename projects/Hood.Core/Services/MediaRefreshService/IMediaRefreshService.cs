using Hood.Models;
using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IMediaRefreshService
    {
        bool IsComplete();
        bool RunUpdate(HttpContext context);
        void Kill();
        MediaRefreshReport Report();
    }

}