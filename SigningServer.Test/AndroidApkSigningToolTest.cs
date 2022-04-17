﻿using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SigningServer.Android;
using SigningServer.Android.Com.Android.Apksig;
using SigningServer.Contracts;

namespace SigningServer.Test;

[TestClass]
public class AndroidApkSigningToolTest : UnitTestBase
{
    [TestMethod]
    public void IsFileSigned_UnsignedFile_ReturnsFalse()
    {
        var signingTool = new AndroidApkSigningTool();
        Assert.IsTrue(File.Exists("TestFiles/unsigned/unsigned-aligned.apk"));
        Assert.IsFalse(signingTool.IsFileSigned("TestFiles/unsigned/unsigned-aligned.apk"));
    }

    [TestMethod]
    public void IsFileSigned_SignedFile_ReturnsTrue()
    {
        var signingTool = new AndroidApkSigningTool();
        Assert.IsTrue(File.Exists("TestFiles/signed/signed-aligned.apk"));
        Assert.IsTrue(signingTool.IsFileSigned("TestFiles/signed/signed-aligned.apk"));
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "SignFile_Works")]
    public void SignFile_Unsigned_Jar_Works()
    {
        CanSign(new AndroidApkSigningTool(), "SignFile_Works/unsigned/unsigned-aligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "SignFile_Works")]
    public void SignFile_Unsigned_ApkAligned_Works()
    {
        CanSign(new AndroidApkSigningTool(), "SignFile_Works/unsigned/unsigned-aligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "SignFile_Works")]
    public void SignFile_Unsigned_ApkUnaligned_Works()
    {
        CanSign(new AndroidApkSigningTool(), "SignFile_Works/unsigned/unsigned-unaligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "NoResign_Fails")]
    public void SignFile_Signed_ApkUnaligned_NoResign_Fails()
    {
        CannotResign(new AndroidApkSigningTool(), "NoResign_Fails/signed/signed-unaligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "NoResign_Fails")]
    public void SignFile_Signed_ApkAligned_NoResign_Fails()
    {
        CannotResign(new AndroidApkSigningTool(), "NoResign_Fails/signed/signed-aligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "Resign_Works")]
    public void SignFile_Signed_ApkAligned_Resign_Works()
    {
        CanResign(new AndroidApkSigningTool(), "Resign_Works/signed/signed-aligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "Resign_Works")]
    public void SignFile_Signed_ApkUnaligned_Resign_Works()
    {
        CannotResign(new AndroidApkSigningTool(), "Resign_Works/signed/signed-unaligned.apk");
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "ApkAligned_Verifies")]
    public void SignFile_ApkAligned_Verifies()
    {
        TestWithVerify("ApkAligned_Verifies/unsigned/unsigned-aligned.apk", result =>
        {
            if (!result.IsVerified())
            {
                Assert.Fail(string.Join(Environment.NewLine, result.GetAllErrors()));
            }
            result.IsVerifiedUsingV1Scheme().Should().BeTrue();
            result.IsVerifiedUsingV2Scheme().Should().BeTrue();
            result.IsVerifiedUsingV3Scheme().Should().BeTrue();
            result.IsVerifiedUsingV4Scheme().Should().BeFalse();
        });
    }

    [TestMethod]
    [DeploymentItem("TestFiles", "ApkUnaligned_Verifies")]
    public void SignFile_ApkUnaligned_Verifies()
    {
        TestWithVerify("ApkUnaligned_Verifies/unsigned/unsigned-unaligned.apk", result =>
        {
            if (!result.IsVerified())
            {
                Assert.Fail(string.Join(Environment.NewLine, result.GetAllErrors()));
            }
            result.IsVerifiedUsingV1Scheme().Should().BeTrue();
            result.IsVerifiedUsingV2Scheme().Should().BeTrue();
            result.IsVerifiedUsingV3Scheme().Should().BeTrue();
            result.IsVerifiedUsingV4Scheme().Should().BeFalse();
        });
    }

    private void TestWithVerify(string fileName, Action<ApkVerifier.Result> action)
    {
        var signingTool = new AndroidApkSigningTool();
        signingTool.IsFileSupported(fileName).Should().BeTrue();

        var response = new SignFileResponse();
        var request = new SignFileRequest
        {
            FileName = fileName,
            OverwriteSignature = false
        };
        signingTool.SignFile(fileName, AssemblyEvents.Certificate,
            AssemblyEvents.PrivateKey,
            TimestampServer, request, response);

        Trace.WriteLine(response);
        try
        {
            response.Result.Should().Be(SignFileResponseResult.FileSigned);
            signingTool.IsFileSigned(fileName).Should().BeTrue();

            var builder = new ApkVerifier.Builder(new FileInfo(fileName));
            var result = builder.Build().Verify();

            action(result);
        }
        finally
        {
            response.Dispose();
        }
    }
}