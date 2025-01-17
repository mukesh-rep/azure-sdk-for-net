﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Azure.Core.TestFramework
{
    public class MockResponse : Response
    {
        private readonly Dictionary<string, List<string>> _headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public MockResponse(int status, string reasonPhrase = null)
        {
            Status = status;
            ReasonPhrase = reasonPhrase;
        }

        public override int Status { get; }

        public override string ReasonPhrase { get; }

        public override Stream ContentStream { get; set; }

        public override string ClientRequestId { get; set; }

        private bool? _isError;
        public override bool IsError { get => _isError ?? base.IsError; }
        public void SetIsError(bool value) => _isError = value;

        public bool IsDisposed { get; private set; }

        public void SetContent(byte[] content)
        {
            ContentStream = new MemoryStream(content, 0, content.Length, false, true);
        }

        public MockResponse SetContent(string content)
        {
            SetContent(Encoding.UTF8.GetBytes(content));
            return this;
        }

        public MockResponse AddHeader(string name, string value)
        {
            return AddHeader(new HttpHeader(name, value));
        }

        public MockResponse AddHeader(HttpHeader header)
        {
            if (!_headers.TryGetValue(header.Name, out List<string> values))
            {
                _headers[header.Name] = values = new List<string>();
            }

            values.Add(header.Value);
            return this;
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool TryGetHeader(string name, out string value)
        {
            if (_headers.TryGetValue(name, out List<string> values))
            {
                value = JoinHeaderValue(values);
                return true;
            }

            value = null;
            return false;
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
        {
            var result = _headers.TryGetValue(name, out List<string> valuesList);
            values = valuesList;
            return result;
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool ContainsHeader(string name)
        {
            return TryGetHeaderValues(name, out _);
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override IEnumerable<HttpHeader> EnumerateHeaders() => _headers.Select(h => new HttpHeader(h.Key, JoinHeaderValue(h.Value)));

        private static string JoinHeaderValue(IEnumerable<string> values)
        {
            return string.Join(",", values);
        }

        public override void Dispose()
        {
            IsDisposed = true;
        }
    }
}
