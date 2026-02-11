// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net.Http;

namespace GooglePay.PaymentDataCryptography
{
    /// <summary>
    /// Configuration options for <see cref="GoogleKeyProvider"/>.
    /// </summary>
    public class GoogleKeyProviderOptions
    {
        /// <summary>
        /// When true, uses Google's test key endpoint instead of production.
        /// Ignored if <see cref="Url"/> is set.
        /// </summary>
        public bool IsTest { get; set; }

        /// <summary>
        /// Custom URL for fetching signing keys. Overrides <see cref="IsTest"/> when set.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Override the cache duration for fetched keys. When null (default),
        /// the server's Cache-Control max-age header is used, falling back to 7 days.
        /// </summary>
        public TimeSpan? CacheDuration { get; set; }

        /// <summary>
        /// Custom HTTP message handler for the underlying <see cref="HttpClient"/>.
        /// Use this to configure certificate validation, logging, retries, etc.
        /// The handler is not owned by the provider and will not be disposed.
        /// </summary>
        public HttpMessageHandler MessageHandler { get; set; }
    }
}
