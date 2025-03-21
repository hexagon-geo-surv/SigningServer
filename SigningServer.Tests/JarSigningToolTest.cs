﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SigningServer.Android;
using SigningServer.Android.Com.Android.Apksig;
using SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1;
using SigningServer.Android.Com.Android.Apksig.Internal.Jar;
using SigningServer.Android.Com.Android.Apksig.Internal.Util;
using SigningServer.Core;

namespace SigningServer.Test;

public class JarSigningToolTest : UnitTestBase
{
    #region Jar

    [Test]
    public async Task IsFileSigned_UnsignedFile_Jar_ReturnsFalse()
    {
        var signingTool = new JarSigningTool();
        File.Exists("TestFiles/unsigned/unsigned.jar").Should().BeTrue();
        (await signingTool.IsFileSignedAsync("TestFiles/unsigned/unsigned.jar", CancellationToken.None)).Should()
            .BeFalse();
    }

    [Test]
    public async Task IsFileSigned_SignedFile_Jar_ReturnsTrue()
    {
        var signingTool = new JarSigningTool();
        File.Exists("TestFiles/signed/signed.jar").Should().BeTrue();
        (await signingTool.IsFileSignedAsync("TestFiles/signed/signed.jar", CancellationToken.None)).Should().BeTrue();
    }

    [Test]
    [DeploymentItem("TestFiles", "SignFile_Works")]
    public async Task SignFile_Unsigned_Jar_Works()
    {
        await CanSignAsync(new JarSigningTool(), "SignFile_Works/unsigned/unsigned.jar");
    }

    [Test]
    [DeploymentItem("TestFiles", "NoResign_Fails")]
    public async Task SignFile_Signed_Jar_NoResign_Fails()
    {
        await CannotResignAsync(new JarSigningTool(), "NoResign_Fails/signed/signed.jar");
    }

    [Test]
    [DeploymentItem("TestFiles", "Resign_Works")]
    public async Task SignFile_Signed_Jar_Resign_Works()
    {
        await CanResignAsync(new JarSigningTool(), "Resign_Works/signed/signed.jar");
    }

    [Test]
    [DeploymentItem("TestFiles", "Jar_Verifies")]
    public async Task SignFile_Jar_Verifies()
    {
        var (result, _) = await TestWithVerifyAsync("Jar_Verifies/unsigned/unsigned.jar");
        if (!result.IsVerified())
        {
            Assert.Fail(string.Join(Environment.NewLine, result.GetAllErrors()));
        }

        result.IsVerifiedUsingV1Scheme().Should().BeTrue();
        result.IsVerifiedUsingV2Scheme().Should().BeFalse();
        result.IsVerifiedUsingV3Scheme().Should().BeFalse();
        result.IsVerifiedUsingV4Scheme().Should().BeFalse();
    }

    [TestCase("unsigned.jar", "SHA1", DigestAlgorithm.SHA1_CASE)]
    [TestCase("unsigned.jar", "SHA256", DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.jar", "Invalid", DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.jar", null, DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.jar", "", DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.aab", "SHA1", DigestAlgorithm.SHA1_CASE)]
    [TestCase("unsigned.aab", "SHA256", DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.aab", "Invalid", DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.aab", null, DigestAlgorithm.SHA256_CASE)]
    [TestCase("unsigned.aab", "", DigestAlgorithm.SHA256_CASE)]
    [DeploymentItem("TestFiles", "Jar_Verifies")]
    public async Task SignFile_RespectsHashAlgorithm(string inputFile, string? hashAlgorithm,
        int expectedDigestAlgorithmCase)
    {
        DigestAlgorithm expectedDigestAlgorithm;
        switch (expectedDigestAlgorithmCase)
        {
            case DigestAlgorithm.SHA1_CASE:
                expectedDigestAlgorithm = DigestAlgorithm.SHA1;
                break;
            case DigestAlgorithm.SHA256_CASE:
                expectedDigestAlgorithm = DigestAlgorithm.SHA256;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expectedDigestAlgorithmCase));
        }

        var request = new SignFileRequest(
            $"Jar_Verifies/unsigned/{inputFile}",
            AssemblyEvents.Certificate,
            AssemblyEvents.PrivateKey,
            string.Empty,
            TimestampServer,
            hashAlgorithm,
            false
        );
        var (result, response) = await TestWithVerifyAsync(request);
        if (!result.IsVerified())
        {
            Assert.Fail(string.Join(Environment.NewLine, result.GetAllErrors()));
        }

        result.IsVerifiedUsingV1Scheme().Should().BeTrue();
        
        // read the manifest from the APK and check the digest algorithm
        using var apkFile = new Android.IO.RandomAccessFile(new FileInfo(response.ResultFiles![0].OutputFilePath), "r");
        var apk = Android.Com.Android.Apksig.Util.DataSources.AsDataSource(apkFile, 0, apkFile.Length());
        var apkSections = Android.Com.Android.Apksig.Apk.ApkUtils.FindZipSections(apk);
        var cdStartOffset = apkSections.GetZipCentralDirectoryOffset();
        var cdRecords = V1SchemeVerifier.ParseZipCentralDirectory(apk, apkSections);
        var manifestEntry = cdRecords.First(r => V1SchemeConstants.MANIFEST_ENTRY_NAME.Equals(r.GetName()));
        
        var manifestBytes = Android.Com.Android.Apksig.Internal.Zip.LocalFileRecord.GetUncompressedData(apk, manifestEntry, cdStartOffset);
        var manifest = new ManifestParser(manifestBytes);
        var manifestIndividualSections = manifest.ReadAllSections();
        
        var digestName = V1SchemeSigner.GetEntryDigestAttributeName(expectedDigestAlgorithm);
        foreach (var section in manifestIndividualSections)
        {
            AssertDigest(digestName, section);
        }
    }
    
    private static void AssertDigest(string digestName, ManifestParser.Section section)
    {
        foreach (var attribute in section.GetAttributes())
        {
            var attributeName = attribute.GetName();
            if (attributeName.Contains("-Digest"))
            {
                Assert.That(digestName, Is.EqualTo(attributeName),
                    section.GetName() + " has wrong digest type " + attributeName);
            }
        }
    }

    #endregion

    #region Aab

    [Test]
    public async Task IsFileSigned_UnsignedFile_Aab_ReturnsFalse()
    {
        var signingTool = new JarSigningTool();
        File.Exists("TestFiles/unsigned/unsigned.aab").Should().BeTrue();
        (await signingTool.IsFileSignedAsync("TestFiles/unsigned/unsigned.aab", CancellationToken.None)).Should()
            .BeFalse();
    }

    [Test]
    public async Task IsFileSigned_SignedFile_Aab_ReturnsTrue()
    {
        var signingTool = new JarSigningTool();
        File.Exists("TestFiles/signed/signed.aab").Should().BeTrue();
        (await signingTool.IsFileSignedAsync("TestFiles/signed/signed.aab", CancellationToken.None)).Should().BeTrue();
    }

    [Test]
    [DeploymentItem("TestFiles", "SignFile_Works")]
    public async Task SignFile_Unsigned_Aab_Works()
    {
        await CanSignAsync(new JarSigningTool(), "SignFile_Works/unsigned/unsigned.aab");
    }

    [Test]
    [DeploymentItem("TestFiles", "NoResign_Fails")]
    public async Task SignFile_Signed_Aab_NoResign_Fails()
    {
        await CannotResignAsync(new JarSigningTool(), "NoResign_Fails/signed/signed.aab");
    }

    [Test]
    [DeploymentItem("TestFiles", "Resign_Works")]
    public async Task SignFile_Signed_Aab_Resign_Works()
    {
        await CanResignAsync(new JarSigningTool(), "Resign_Works/signed/signed.aab");
    }

    [Test]
    [DeploymentItem("TestFiles", "Aab_Verifies")]
    public async Task SignFile_Aab_Verifies()
    {
        var (result, _) = await TestWithVerifyAsync("Aab_Verifies/unsigned/unsigned.aab");
        if (!result.IsVerified())
        {
            Assert.Fail(string.Join(Environment.NewLine, result.GetAllErrors()));
        }

        result.IsVerifiedUsingV1Scheme().Should().BeTrue();
        result.IsVerifiedUsingV2Scheme().Should().BeFalse();
        result.IsVerifiedUsingV3Scheme().Should().BeFalse();
        result.IsVerifiedUsingV4Scheme().Should().BeFalse();
    }

    #endregion

    private Task<(ApkVerifier.Result verifyResult, SignFileResponse signFileResponse)> TestWithVerifyAsync(
        string fileName)
    {
        var request = new SignFileRequest(
            fileName,
            AssemblyEvents.Certificate,
            AssemblyEvents.PrivateKey,
            string.Empty,
            TimestampServer,
            null,
            false
        );
        return TestWithVerifyAsync(request);
    }


    private async Task<(ApkVerifier.Result verifyResult, SignFileResponse signFileResponse)> TestWithVerifyAsync(
        SignFileRequest request)
    {
        var signingTool = new JarSigningTool();
        signingTool.IsFileSupported(request.InputFilePath).Should().BeTrue();

        var response = await signingTool.SignFileAsync(request, CancellationToken.None);
        response.Status.Should().Be(SignFileResponseStatus.FileSigned);
        (await signingTool.IsFileSignedAsync(response.ResultFiles![0].OutputFilePath, CancellationToken.None)).Should()
            .BeTrue();

        var builder = new ApkVerifier.Builder(new FileInfo(response.ResultFiles[0].OutputFilePath))
            .SetMinCheckedPlatformVersion(AndroidSdkVersion.JELLY_BEAN_MR2)
            .SetMaxCheckedPlatformVersion(AndroidSdkVersion.JELLY_BEAN_MR2);
        var result = builder.Build().Verify();

        return (result, response);
    }
}
