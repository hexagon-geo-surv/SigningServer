// <auto-generated>
// This code was auto-generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>

/*
 * Copyright (C) 2022 Daniel Kuschny (C# port)
 * Copyright (C) 2019 The Android Open Source Project
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
using System.Linq;
using System.Threading.Tasks;

namespace SigningServer.Android.Com.Android.Apksig.Util
{
    public delegate void RunnablesExecutor(SigningServer.Android.Com.Android.Apksig.Util.RunnablesProvider provider);
    
    public class RunnablesExecutors
    {
        public static readonly RunnablesExecutor SINGLE_THREADED = provider =>
        {
            provider()();
        };

        public static readonly RunnablesExecutor MULTI_THREADED = provider =>
        {
            var tasks = Enumerable.Range(0, Environment.ProcessorCount)
                .Select(_ => Task.Factory.StartNew(() =>
                {
                    var r = provider();
                    r();
                }));

            Task.WaitAll(tasks.ToArray());
        };
    }
    
}
