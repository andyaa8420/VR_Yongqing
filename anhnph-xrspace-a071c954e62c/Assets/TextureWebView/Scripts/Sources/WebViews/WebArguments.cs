using UnityEngine;

namespace TWV
{
    internal class WebArguments
    {
        private bool _useNativeWeb;
        private Vector2 _fixedPageSize;

        /// <summary>
        /// Use native web view of current platform if supported
        /// </summary>
        public bool UseNativePlayer
        {
            get { return _useNativeWeb; }
            set { _useNativeWeb = value; }
        }

        /// <summary>
        /// Fixed page size
        /// </summary>
        public Vector2 FixedPageSize
        {
            get { return _fixedPageSize; }
            set { _fixedPageSize = value; }
        }
    }
}
