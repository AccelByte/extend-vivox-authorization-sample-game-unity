// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Vivox;
using UnityEngine.Networking;

public class VivoxTokenProvider : IVivoxTokenProvider
{
    public class VivoxChannelInfo
    {
        public static readonly Regex Regex = new Regex(@"([a-zA-Z]+):(.+)-([a-z])-(.+)\.(.+)@(.+)");

        public static VivoxChannelInfo CreateFromUri(string uri)
        {
            string channelPrefix = null;
            string channelId = null;
            string channelType = null;
            string issuer = null;
            string domain = null;

            if (!string.IsNullOrWhiteSpace(uri))
            {
                Match match = Regex.Match(input: uri);

                if (match.Success)
                {
                    channelPrefix = match.Groups[2].Value;
                    channelId = match.Groups[5].Value;
                    channelType = match.Groups[3].Value;
                    issuer = match.Groups[4].Value;
                    domain = match.Groups[6].Value;
                }
            }

            return new VivoxChannelInfo
            {
                ChannelPrefix = channelPrefix,
                ChannelId = channelId,
                ChannelType = channelType,
                Issuer = issuer,
                Domain = domain,
            };
        }

        [JsonProperty(propertyName: "channelPrefix", Required = Required.Always)]
        public string ChannelPrefix { get; set; }

        [JsonProperty(propertyName: "channelId", Required = Required.Always)]
        public string ChannelId { get; set; }

        [JsonProperty(propertyName: "channelType", Required = Required.Always)]
        public string ChannelType { get; set; }

        [JsonProperty(propertyName: "issuer", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string Issuer { get; set; }

        [JsonProperty(propertyName: "domain", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string Domain { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ChannelPrefix) &&
                               !string.IsNullOrWhiteSpace(ChannelId) &&
                               !string.IsNullOrWhiteSpace(ChannelType);

        public string ChannelTypeName
        {
            get
            {
                switch (ChannelType)
                {
                    case "e":
                        return "echo";
                    case "d":
                        return "positional";
                    case "g":
                        return "nonpositional";
                    default:
                        throw new Exception($"unknown channel type: {ChannelType}");
                }
            }
        }
    }

    public class VivoxUserInfo
    {
        public static readonly Regex Regex = new Regex(@"([a-zA-Z]+):\.(.+)\.(.+)\.@(.+)");

        public static VivoxUserInfo CreateFromUri(string uri)
        {
            string userId = null;
            string issuer = null;
            string domain = null;

            if (!string.IsNullOrWhiteSpace(uri))
            {
                Match match = Regex.Match(input: uri);

                if (match.Success)
                {
                    userId = match.Groups[3].Value;
                    issuer = match.Groups[2].Value;
                    domain = match.Groups[4].Value;
                }
            }

            return new VivoxUserInfo
            {
                UserId = userId,
                Issuer = issuer,
                Domain = domain,
            };
        }

        [JsonProperty(propertyName: "userId", Required = Required.Always)]
        public string UserId { get; set; }

        [JsonProperty(propertyName: "issuer", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string Issuer { get; set; }

        [JsonProperty(propertyName: "domain", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string Domain { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(UserId);
    }

    public class VivoxTokenRequestV1
    {
        [JsonProperty(propertyName: "type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(propertyName: "username", Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty(propertyName: "channelId", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string ChannelId { get; set; }

        [JsonProperty(propertyName: "channelType", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string ChannelType { get; set; }

        [JsonProperty(propertyName: "targetUsername", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string TargetUsername{ get; set; }
    }

    public class VivoxTokenResponseV1
    {
        [JsonProperty(propertyName: "accessToken", Required = Required.Always)]
        public string AccessToken { get; set; }

        [JsonProperty(propertyName: "uri", NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
        public string Uri { get; set; }
    }

    private readonly string _url;

    public VivoxTokenProvider(string url)
    {
        _url = url;
    }

    public VivoxTokenRequestV1 CreateTokenRequestV1(
        string issuer = null, TimeSpan? expiration = null, string targetUserUri = null,
        string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
    {
        VivoxChannelInfo channelInfo = VivoxChannelInfo.CreateFromUri(uri: channelUri);
        VivoxUserInfo fromUserInfo = VivoxUserInfo.CreateFromUri(uri: fromUserUri);
        VivoxUserInfo targetUserInfo = VivoxUserInfo.CreateFromUri(uri: targetUserUri);

        if (!fromUserInfo.IsValid)
        {
            throw new Exception($"unable to extract user ID from uri: {fromUserUri}");
        }

        VivoxTokenRequestV1 tokenRequestV1 = new VivoxTokenRequestV1
        {
            Type = action,
            Username = fromUserInfo.UserId,
            TargetUsername = targetUserUri,
        };

        if (channelInfo.IsValid)
        {
            tokenRequestV1.ChannelId = channelInfo.ChannelId;
            tokenRequestV1.ChannelType = channelInfo.ChannelTypeName;
        }

        if (targetUserInfo.IsValid)
        {
            tokenRequestV1.TargetUsername = targetUserInfo.UserId;
        }

        return tokenRequestV1;
    }

    public async Task<string> GetTokenAsync(
        string issuer = null, TimeSpan? expiration = null, string targetUserUri = null,
        string action = null, string channelUri = null, string fromUserUri = null, string realm = null)
    {
        VivoxTokenRequestV1 tokenRequestV1 = CreateTokenRequestV1(
            issuer, expiration, targetUserUri,
            action, channelUri, fromUserUri, realm);

        using (UnityWebRequest request = new UnityWebRequest(url: _url, method: UnityWebRequest.kHttpVerbPOST))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            string bodyJson = JsonConvert.SerializeObject(tokenRequestV1);

            byte[] bodyBytes = new System.Text.UTF8Encoding().GetBytes(bodyJson);

            request.uploadHandler = new UploadHandlerRaw(bodyBytes);

            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            string responseText = request.downloadHandler.text;

            try
            {
                VivoxTokenResponseV1 response =
                    JsonConvert.DeserializeObject<VivoxTokenResponseV1>(value: responseText);

                return response.AccessToken;
            }
            catch (JsonSerializationException exception)
            {
                throw new Exception(responseText, exception);
            }
        }
    }
}
