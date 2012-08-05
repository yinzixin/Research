﻿using System;

namespace System.Web.WebPages {

    internal class DisposableAction : IDisposable {
        private Action _action;
        private bool _hasDisposed;

        public DisposableAction(Action action) {
            if (action == null) {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // If we were disposed by the finalizer it's because the user didn't use a "using" block, so don't do anything!
            if (disposing) {
                lock (this) {
                    if (!_hasDisposed) {
                        _hasDisposed = true;
                        _action();
                    }
                }
            }
        }
    }
}
