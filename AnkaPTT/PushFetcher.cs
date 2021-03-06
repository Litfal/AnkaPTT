﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;

namespace AnkaPTT
{
    class PushFetcher
    {
        string injectScript;
        object _lck = new object();
        CancellationTokenSource _cts;
        int delayMs = Properties.Settings.Default.FetchPushInterval;

        public event EventHandler<PushFetchedEventArgs> PushFetched;

        public PushFetcher()
        {
            injectScript = Properties.Resources.injection;
        }

        internal void Start(IBrowser browser)
        {
            Stop();
            _cts = new CancellationTokenSource();
            CancellationToken ct = _cts.Token;
            Task.Factory.StartNew(() =>
            {
                browser.MainFrame.EvaluateScriptAsync(injectScript + "clickToggleAuto();").Wait();
                while (true)
                {
                    //ct.ThrowIfCancellationRequested();
                    if (ct.IsCancellationRequested) return;
                    var response = browser.MainFrame.EvaluateScriptAsync("fetch();").Result;
                    if (response.Success && response.Result != null)
                    {
                        dynamic r = response.Result;
                        // ct.ThrowIfCancellationRequested();
                        if (ct.IsCancellationRequested) return;
                        OnGotResponse(r);
                    }
                    Thread.Sleep(delayMs);
                }
            });
        }


        internal void FetchOnce(IBrowser browser)
        {
            var response = browser.MainFrame.EvaluateScriptAsync("fetch();").Result;
            if (response.Success && response.Result != null)
            {
                dynamic r = response.Result;
                OnGotResponse(r);
            }
        }

        private void OnGotResponse(dynamic result)
        {
            bool? fatalError = result.fatalerror;
            var pushes = Push.MapByDynamicList(result.pushes as IEnumerable<dynamic>).ToList();
            PushFetched?.Invoke(this, new PushFetchedEventArgs(fatalError, pushes));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }
    }

    class PushFetchedEventArgs : EventArgs
    {
        // ture = push auto refresh error, need manual refresh
        // false = push auto refresh OK
        // null = cloud't find push auto refresh status element
        public bool? FatalError { get; set; }   
        public List<Push> Pushes { get; private set; }
        public PushFetchedEventArgs(bool? fatalError, List<Push> pushes)
        {
            FatalError = fatalError;
            this.Pushes = pushes;
        }
    }
}
