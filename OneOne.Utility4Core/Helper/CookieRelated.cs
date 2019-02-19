using System;
using System.Configuration;
using System.Web;

public class CookieRelated
{
    // Fields
    private static int _exprise = 10;
    public static readonly string Domain = ConfigurationManager.AppSettings["Domain"];

    // Methods
    public static void DeleteCookies(string strCookiesName)
    {
        HttpCookie cookie = HttpContext.Current.Request.Cookies[strCookiesName];
        if (cookie != null)
        {
            TimeSpan span = new TimeSpan(-1, 0, 0, 0);
            cookie.Expires = DateTime.Now.Add(span);
            HttpContext.Current.Response.AppendCookie(cookie);
        }
    }

    public static void DeleteCookies(string strCookiesName, string strName)
    {
        HttpCookie cookie = HttpContext.Current.Request.Cookies[strCookiesName];
        if (cookie != null)
        {
            cookie.Values.Remove(strName);
            HttpContext.Current.Response.AppendCookie(cookie);
        }
    }

    public static string GetCookie(string strName)
    {
        if (HttpContext.Current.Request.Cookies[strName] != null)
        {
            return HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies[strName].Value);
        }
        return string.Empty;
    }

    public static bool IsCookies(string strCookiesName)
    {
        return (HttpContext.Current.Request.Cookies[strCookiesName] == null);
    }

    public static string ReadCookies(string strCookieName, string strName)
    {
        if (HttpContext.Current.Request.Cookies[strCookieName] != null)
        {
            return HttpContext.Current.Request.Cookies[strCookieName][strName];
        }
        return "";
    }

    public static void SetCookie(string name, string value, int expiresDays)
    {
        HttpCookie cookie;
        if (HttpContext.Current.Request.Cookies[name] == null)
        {
            cookie = new HttpCookie(name);
        }
        else
        {
            cookie = HttpContext.Current.Request.Cookies[name];
        }
        cookie.Domain = Domain;
        cookie.Value = HttpUtility.UrlEncode(value);
        cookie.Expires = DateTime.Now.AddDays((double) expiresDays);
        HttpContext.Current.Response.AppendCookie(cookie);
    }

    public static void UpdateCookies(string strCookieName, string strName, string strValue)
    {
        if (!string.IsNullOrEmpty(strCookieName))
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strCookieName];
            if (cookie != null)
            {
                cookie.Values.Set(strName, strValue);
                HttpContext.Current.Response.AppendCookie(cookie);
            }
        }
    }

    public static void WriteCookies(string cookiesName, string strName, string strValue)
    {
        WriteCookies(cookiesName, strName, strValue, Exprise, false);
    }

    public static void WriteCookies(string CookiesName, string strName, string strValue, int expires, bool isAdd)
    {
        if (!string.IsNullOrEmpty(CookiesName) && !string.IsNullOrEmpty(strName))
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[CookiesName];
            if (cookie == null)
            {
                cookie = new HttpCookie(CookiesName);
            }
            if (!isAdd)
            {
                cookie.Expires = DateTime.Now.AddMinutes((double) expires);
            }
            else
            {
                cookie.Expires = DateTime.MaxValue;
            }
            cookie.Domain = Domain;
            cookie.Values.Add(strName, strValue);
            HttpContext.Current.Response.AppendCookie(cookie);
        }
    }

    /// <summary>
    /// 设置Cookie的有效域
    /// </summary>
    /// <param name="response"> </param>
    /// <param name="cookieName"></param>
    /// <param name="value"> </param>
    /// <param name="expire">过期时间 </param>
    /// <param name="mainDomianCookie">是否写到主域名下 </param>
    public static void SetCookieDomain(HttpResponseBase response, string cookieName, string value, DateTime expire = default(DateTime), bool mainDomianCookie = true)
    {
        var cookie = GetCookie(cookieName, value, expire, mainDomianCookie);
        response.Cookies.Add(cookie);
    }

    /// <summary>
    /// 设置Cookie的有效域
    /// </summary>
    /// <param name="response"> </param>
    /// <param name="cookieName"></param>
    /// <param name="value"> </param>
    /// <param name="expire">过期时间 </param>
    /// <param name="mainDomianCookie">是否写到主域名下 </param>
    public static void SetCookieDomain(HttpResponse response, string cookieName, string value, DateTime expire = default(DateTime), bool mainDomianCookie = true)
    {
        var cookie = GetCookie(cookieName, value, expire, mainDomianCookie);
        response.Cookies.Add(cookie);
    }

    /// <summary>
    /// 获取Cookie的对象
    /// </summary>
    /// <param name="cookieName">Cookie的名字</param>
    /// <param name="value">Cookie的值</param>
    /// <param name="expire">Cookie的过期时间</param>
    /// <param name="mainDomianCookie">是否写到主域名下</param>
    /// <returns>Cookie的对象</returns>
    public static HttpCookie GetCookie(string cookieName, string value, DateTime expire = default(DateTime), bool mainDomianCookie = true)
    {
        if (string.IsNullOrEmpty(cookieName))
        {
            return null;
        }
        var cookie = new HttpCookie(cookieName);
        if (!string.IsNullOrEmpty(value))
        {
            cookie.Value = HttpUtility.UrlEncode(value);
        }
        if (expire != default(DateTime))
        {
            cookie.Expires = expire;
        }
        if (mainDomianCookie)
        {
            if (!string.IsNullOrEmpty(Domain))
            {
                cookie.Domain = Domain;
            }
        }
        return cookie;
    }

    // Properties
    public static int Exprise
    {
        get
        {
            return _exprise;
        }
        set
        {
            _exprise = value;
        }
    }
}

 
 
