using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GitRepoTracker
{
    public class SafeFormUrlEncodedContent : ByteArrayContent
    {
        public SafeFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection, Encoding encoding)
            : base(GetContentByteArray(nameValueCollection, null, encoding))
        {
            base.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }

        public SafeFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection
            , Dictionary<string, List<string>> multivalued
            , Encoding encoding)
            : base(GetContentByteArray(nameValueCollection, multivalued, encoding))
        {
            base.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection
            , Dictionary<string, List<string>> multivalued
            , Encoding encoding = null)
        {
            if (nameValueCollection == null)
            {
                throw new ArgumentNullException("nameValueCollection");
            }
            //Regular post parameters
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> current in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append('&');

                stringBuilder.Append(Encode(current.Key, encoding));
                stringBuilder.Append('=');
                stringBuilder.Append(Encode(current.Value, encoding));
            }
            //Multivalued post parameters
            if (multivalued != null)
            {
                foreach (string key in multivalued.Keys)
                {
                    foreach (string value in multivalued[key])
                    {
                        if (stringBuilder.Length > 0)
                            stringBuilder.Append('&');
                        stringBuilder.Append(Encode(key, encoding));
                        stringBuilder.Append('=');
                        stringBuilder.Append(Encode(value, encoding));
                    }
                }
            }

            if (encoding == null)
                encoding = Encoding.UTF8;
            return encoding.GetBytes(stringBuilder.ToString());
        }
        private static string Encode(string data, Encoding encoding)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            if (encoding == null)
                encoding = Encoding.UTF8;

            return System.Web.HttpUtility.UrlEncode(data, encoding).Replace("%20", "+");
        }
    }
}
