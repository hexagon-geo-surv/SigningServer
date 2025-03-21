// <auto-generated>
// This code was auto-generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>

/*
 * Copyright (C) 2022 Daniel Kuschny (C# port)
 * Copyright (C) 2016 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1
{
    /// <summary>
    /// Digest algorithm used with JAR signing (aka v1 signing scheme).
    /// </summary>
    public class DigestAlgorithm
    {
        public static readonly SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm SHA1 = new SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm("SHA-1", 0);
        
        public const int SHA1_CASE = 0;
        
        public static readonly SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm SHA256 = new SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm("SHA-256", 1);
        
        public const int SHA256_CASE = 1;
        
        internal readonly string mJcaMessageDigestAlgorithm;
        
        internal DigestAlgorithm(string jcaMessageDigestAlgoritm, int caseValue)
        {
            mJcaMessageDigestAlgorithm = jcaMessageDigestAlgoritm;
            Case = caseValue;
        }
        
        /// <summary>
        /// Returns the {@link java.security.MessageDigest} algorithm represented by this digest
        /// algorithm.
        /// </summary>
        public virtual string GetJcaMessageDigestAlgorithm()
        {
            return mJcaMessageDigestAlgorithm;
        }
        
        public static System.Collections.Generic.IComparer<SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm> BY_STRENGTH_COMPARATOR = new SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.StrengthComparator();
        
        internal class StrengthComparator: System.Collections.Generic.IComparer<SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm>
        {
            public int Compare(SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm a1, SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm a2)
            {
                switch (a1.Case)
                {
                    case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA1_CASE:
                        switch (a2.Case)
                        {
                            case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA1_CASE:
                                return 0;
                            case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA256_CASE:
                                return -1;
                        }
                        throw new SigningServer.Android.Core.RuntimeException("Unsupported algorithm: " + a2);
                    case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA256_CASE:
                        switch (a2.Case)
                        {
                            case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA1_CASE:
                                return 1;
                            case SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm.SHA256_CASE:
                                return 0;
                        }
                        throw new SigningServer.Android.Core.RuntimeException("Unsupported algorithm: " + a2);
                    default:
                        throw new SigningServer.Android.Core.RuntimeException("Unsupported algorithm: " + a1);
                }
            }
            
        }
        
        public int Case
        {
            get;
        }
        
        internal static readonly SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm[] _values = {SHA1, SHA256};
        
        public static SigningServer.Android.Com.Android.Apksig.Internal.Apk.V1.DigestAlgorithm[] Values()
        {
            return _values;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            if (!(obj is DigestAlgorithm d))
            {
                return false;
            }

            return Case == d.Case;
        }

        public override int GetHashCode()
        {
            return Case;
        }
    }
    
}
