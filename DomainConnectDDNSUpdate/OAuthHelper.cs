﻿using System;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Net;
using System.Collections.Generic;
using DnsClient;

namespace OAuthHelper
{
    public class OAuthHelper
    {
        const string providerId = "exampleservice.domainconnect.org";

        //
        // GetTokens
        //
        // Given an input of either a response code from an oauth authorization, or a refresh token,
        // will fetch the access_token and a refresh_token using oauth
        //
        static public bool GetTokens(string code, string domain, string host, string dns_provider, string urlAPI, bool use_refresh, out string access_token, out string refresh_token, out int expires_in, out int iat)
        {
            int status = 0;
            iat = 0;
            expires_in = 0;
            string grant = "authorization_code";
            string code_key = "code";
            if (use_refresh)
            {
                grant = "refresh_token";
                code_key = "refresh_token";
            }

            refresh_token = null;
            access_token = null;

            string redirect_url = "http://exampleservice.domainconnect.org/async_oauth_response?domain=" + domain + "&hosts=" + host + "&dns_provider=" + dns_provider;

            string url = urlAPI + "/v2/oauth/access_token?" + code_key + "=" + code + "&grant_type=" + grant + "&client_id=" + providerId + "&client_secret=DomainConnectGeheimnisSecretString&redirect_uri=" + WebUtility.UrlEncode(redirect_url);

            string json = RestAPIHelper.RestAPIHelper.POST(url, null, out status);
            if (status >= 300)
            {
                return false;
            }
            
            var jss = new JavaScriptSerializer();
            var table = jss.Deserialize<dynamic>(json);


            access_token = table["access_token"];
            refresh_token = table["refresh_token"];
            expires_in = table["expires_in"];
            iat = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return true;
        }

        static public string GetDomainConnectRecord(string host)
        {
            var client = new LookupClient();
            client.UseCache = true;

            var result = client.Query("_domainconnect." + host, QueryType.TXT);

            foreach (var answer in result.Answers)
            {
                if (answer.RecordType == DnsClient.Protocol.ResourceRecordType.TXT)
                {
                    DnsClient.Protocol.TxtRecord txtRecord = (DnsClient.Protocol.TxtRecord)answer;

                    return ((string[])txtRecord.Text)[0];
                }
            }

            return null;

        }

        static public bool GetConfig(string domain, out string providerName, out string urlAPI, out string urlAsyncUX)
        {
            providerName = null;
            urlAPI = null;
            urlAsyncUX = null;

            string dcr = GetDomainConnectRecord(domain);

            if (dcr != null)
            {
                string url = "https://" + dcr + "/v2/" + "arnoldblinn.com" + "/settings";
                int status = 0;
                string json = RestAPIHelper.RestAPIHelper.GET(url, out status);
                if (json != null && status >= 200 && status < 300)
                {
                    var jss = new JavaScriptSerializer();
                    var table = jss.Deserialize<dynamic>(json);

                    providerName = table["providerName"];
                    urlAPI = table["urlAPI"];
                    urlAsyncUX = table["urlAsyncUX"];



                    return true;
                }
            }

            return false;
        }
    }
}
