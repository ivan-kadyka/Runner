using UnityEngine;
using Zenject;

namespace Controllers.TopPanel.Installers
{
    public class TopPanelInstaller : MonoInstaller
    {
        [SerializeField]
        private GameObject _topPanelPrefab;
    
        [SerializeField]
        private GameObject _canvas;
        
        public override void InstallBindings()
        {
            Container.Bind<ITopPanelView>()
                .To<TopPanelView>()
                .FromComponentInNewPrefab(_topPanelPrefab)
                .UnderTransform(_canvas.transform)
                .AsSingle();
        
            Container.Bind<IController>().WithId("TopPanelController").To<TopPanelController>()
                .AsSingle();
        }
    }
}