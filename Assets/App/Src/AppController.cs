using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infra.Controllers;
using Infra.Controllers.Base;
using UnityEngine;

namespace App
{
    public class AppController : ControllerBase
    {
        private readonly IController _topPanelController;
        private readonly IController _roundController;
        private readonly IController _retryPopupController;

        public AppController(
            IController topPanelController,
            IController roundController,
            IController retryPopupController)
        {
            _topPanelController = topPanelController;
            _roundController = roundController;
            _retryPopupController = retryPopupController;
        }

        protected override async UniTask OnStarted(CancellationToken token = default)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _topPanelController.Start(token);
                    await _roundController.Run(token);
                    await _topPanelController.Stop(token);

                    await _retryPopupController.Run(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //here high level generic handling logic for unhandled exceptions.
                //for example: show popup "Something went wrong" and send logs to analytics
            }
        }
    }
}