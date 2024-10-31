using System;
using NUnit.Framework;

public class VivoxTokenProviderTests
{
    [Test]
    public void VivoxChannelInfo_CreateFromUri()
    {
        VivoxTokenProvider.VivoxChannelInfo channelInfo = VivoxTokenProvider.VivoxChannelInfo.CreateFromUri(
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com");

        Assert.AreEqual("confctl", channelInfo.ChannelPrefix);
        Assert.AreEqual("g", channelInfo.ChannelType);
        Assert.AreEqual("blindmelon-AppName-dev", channelInfo.Issuer);
        Assert.AreEqual("testchannel", channelInfo.ChannelId);
        Assert.AreEqual("tla.vivox.com", channelInfo.Domain);
    }

    [Test]
    public void VivoxUserInfo_CreateFromUri()
    {
        VivoxTokenProvider.VivoxUserInfo userInfo = VivoxTokenProvider.VivoxUserInfo.CreateFromUri(
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com");

        Assert.AreEqual("blindmelon-AppName-dev", userInfo.Issuer);
        Assert.AreEqual("beef", userInfo.UserId);
        Assert.AreEqual("tla.vivox.com", userInfo.Domain);
    }

    [Test]
    public void CreateTokenRequestV1()
    {
        VivoxTokenProvider tokenProvider = new VivoxTokenProvider("http://127.0.0.1:8000/v1/token");

        // login
        VivoxTokenProvider.VivoxTokenRequestV1 tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            null,
            "login",
            null,
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.IsNull(tokenRequestV1.ChannelType);
        Assert.IsNull(tokenRequestV1.ChannelId);
        Assert.IsNull(tokenRequestV1.TargetUsername);
        Assert.AreEqual("login", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);

        // join
        tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            null,
            "join",
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com",
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.AreEqual("nonpositional", tokenRequestV1.ChannelType);
        Assert.AreEqual("testchannel", tokenRequestV1.ChannelId);
        Assert.IsNull(tokenRequestV1.TargetUsername);
        Assert.AreEqual("join", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);

        // join muted
        tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            null,
            "join_muted",
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com",
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.AreEqual("nonpositional", tokenRequestV1.ChannelType);
        Assert.AreEqual("testchannel", tokenRequestV1.ChannelId);
        Assert.IsNull(tokenRequestV1.TargetUsername);
        Assert.AreEqual("join_muted", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);

        // signed in user kicking other user from a channel
        tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            "sip:.blindmelon-AppName-dev.jerky.@tla.vivox.com",
            "kick",
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com",
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.AreEqual("nonpositional", tokenRequestV1.ChannelType);
        Assert.AreEqual("testchannel", tokenRequestV1.ChannelId);
        Assert.AreEqual("jerky", tokenRequestV1.TargetUsername);
        Assert.AreEqual("kick", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);

        // admin user kicking other user from a channel (not-supported)

        // admin user kicking other user from a server (not-supported)

        // kick all (not-supported)

        // signed in user muting other user in a channel
        tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            "sip:.blindmelon-AppName-dev.jerky.@tla.vivox.com",
            "mute",
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com",
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.AreEqual("nonpositional", tokenRequestV1.ChannelType);
        Assert.AreEqual("testchannel", tokenRequestV1.ChannelId);
        Assert.AreEqual("jerky", tokenRequestV1.TargetUsername);
        Assert.AreEqual("mute", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);

        // admin user muting other user in a channel (not-supported)

        // mute all (not-supported)

        // transcription
        tokenRequestV1 = tokenProvider.CreateTokenRequestV1(
            "blindmelon-AppName-dev",
            TimeSpan.FromSeconds(1704067200),
            null,
            "trxn",
            "sip:confctl-g-blindmelon-AppName-dev.testchannel@tla.vivox.com",
            "sip:.blindmelon-AppName-dev.beef.@tla.vivox.com",
            null);
        Assert.IsNotNull(tokenRequestV1);
        Assert.AreEqual("nonpositional", tokenRequestV1.ChannelType);
        Assert.AreEqual("testchannel", tokenRequestV1.ChannelId);
        Assert.IsNull(tokenRequestV1.TargetUsername);
        Assert.AreEqual("trxn", tokenRequestV1.Type);
        Assert.AreEqual("beef", tokenRequestV1.Username);
    }
}