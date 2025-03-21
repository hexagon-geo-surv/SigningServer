﻿using SigningServer.Core;

namespace SigningServer.Dtos;

public class SignFileResponseDto
{
    /// <summary>
    /// The result of the signing
    /// </summary>
    public SignFileResponseStatus Status { get; set; }

    /// <summary>
    /// The number of milliseconds it took to fully accept the file
    /// (starting from when the request was received).
    /// </summary>
    public long UploadTimeInMilliseconds { get; set; }

    /// <summary>
    /// The number of milliseconds it took to effectively sign the file.
    /// </summary>
    public long SignTimeInMilliseconds { get; set; }

    /// <summary>
    /// The detailed error message in case <see cref="Status"/> is set to <see cref="SignFileResponseStatus.FileNotSignedError"/>
    /// </summary>
    public string? ErrorMessage { get; set; }
}
