using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;
using System;
using System.Collections.Generic;

namespace Sitecore.Support.Links
{
  public class LinkProvider: Sitecore.Links.LinkProvider
  {
    protected new LinkBuilder CreateLinkBuilder(Sitecore.Links.UrlOptions options) =>
            new LinkBuilder(options);

    public override string GetItemUrl(Item item, UrlOptions options)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(options, "options");
      string itemUrl = this.CreateLinkBuilder(options).GetItemUrl(item);
      if (options.LowercaseUrls)
      {
        itemUrl = itemUrl.ToLowerInvariant();
      }
      return itemUrl;
    }

    public new class LinkBuilder : Sitecore.Links.LinkProvider.LinkBuilder
    {
      public LinkBuilder(Sitecore.Links.UrlOptions options) : base(options)
      {
      }

      protected override string GetServerUrlElement(SiteInfo siteInfo)
      {
        SiteContext site = Context.Site;
        string str = (site != null) ? site.Name : string.Empty;
        string hostName = this.GetHostName();
        string str3 = this.AlwaysIncludeServerUrl ? WebUtil.GetServerUrl() : string.Empty;
        if (siteInfo == null)
        {
          return str3;
        }
        string str4 = ((!string.IsNullOrEmpty(siteInfo.HostName) && !string.IsNullOrEmpty(hostName)) && DoesHostNameMatchSiteInfo(hostName, siteInfo))/*siteInfo.Matches(hostName)) sitecore.support.94463.177155*/ ? hostName : StringUtil.GetString(new string[] { this.GetTargetHostName(siteInfo), hostName });
        if ((!this.AlwaysIncludeServerUrl && siteInfo.Name.Equals(str, StringComparison.OrdinalIgnoreCase)) && hostName.Equals(str4, StringComparison.OrdinalIgnoreCase))
        {
          return str3;
        }
        if ((str4 == string.Empty) || (str4.IndexOf('*') >= 0))
        {
          return str3;
        }
        string str5 = StringUtil.GetString(new string[] { siteInfo.Scheme, this.GetScheme() });
        int @int = MainUtil.GetInt(siteInfo.Port, WebUtil.GetPort());
        int port = WebUtil.GetPort();
        string scheme = this.GetScheme();
        StringComparison ordinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
        if ((str4.Equals(hostName, ordinalIgnoreCase) && (@int == port)) && str5.Equals(scheme, ordinalIgnoreCase))
        {
          return str3;
        }
        string str7 = str5 + "://" + str4;
        if ((@int > 0) && (@int != 80))
        {
          str7 = str7 + ":" + @int;
        }
        return str7;
      }
      internal virtual string GetScheme()
      {
        return WebUtil.GetScheme();
      }

      internal virtual string GetHostName()
      {
        return WebUtil.GetHostName();
      }

      //substitutes siteInfo.Matches(hostName)) sitecore.support.94463.177155
      private bool DoesHostNameMatchSiteInfo(string host, SiteInfo siteInfo)
      {
        Assert.ArgumentNotNull(host, "host");
        if ((host.Length == 0) || (siteInfo.HostName.Length == 0))
        {
          return true;
        }
        host = host.ToLowerInvariant();
        foreach (string[] strArray in GetHostNamePatterns(siteInfo.HostName))
        {
          if (WildCarserParserMatches(host, strArray))
          {
            return true;
          }
        }
        return false;
      }

      //substitutes WildCardParser.Matches(host, strArray) sitecore.support.94463.177155
      private bool WildCarserParserMatches(string value, string[] matchParts)
      {
        Assert.ArgumentNotNull(value, "value");
        Assert.ArgumentNotNull(matchParts, "matchParts");
        if ((value.Length > 0) && (matchParts.Length > 0))
        {
          bool flag = false;
          for (int i = 0; i < matchParts.Length; i++)
          {
            string str = matchParts[i];
            if (str.Length > 0)
            {
              if (str[0] == '*')
              {
                flag = true;
              }
              else
              {
                int index = value.IndexOf(str, StringComparison.InvariantCulture);
                //sitecore.support.94463.177155
                int reverseIndex = str.IndexOf(value, StringComparison.InvariantCulture);
                if ((index < 0) || (reverseIndex < 0) || ((index > 0) && !flag))
                //if ((index < 0) || ((index > 0) && !flag))
                //end of sitecore.support.94463.177155
                {
                  return false;
                }
                value = value.Substring(index + str.Length);
              }
            }
          }
        }
        return true;

      }

      //substitutes this.hostNamePatterns from SiteInfo class sitecore.support.94463.177155
      private List<string[]> GetHostNamePatterns(string hostName)
      {
        var hostNamePatterns = new List<string[]>();
        foreach (string str in hostName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
        {
          hostNamePatterns.Add(WildCardParser.GetParts(str));
        }
        return hostNamePatterns;
      }
    }
  }
}