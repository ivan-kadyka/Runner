using App.GameCore;
using Infra.Controllers;
using UnityEngine;
using Zenject;

namespace App.Character.Dino
{
    public class CharacterInstaller : MonoInstaller
    {
        [SerializeField] 
        private GameObject CharacterPrefab;
        
        public override void InstallBindings()
        {
            Container.Bind<IInputCharacterController>()
                .To<InputCharacterController>()
                .FromComponentsInHierarchy()
                .AsSingle();
            
            Container.Bind<Character>().AsSingle();
            Container.Bind<ICharacter>().FromMethod(it => it.Container.Resolve<Character>()).AsSingle();
            Container.Bind<IGameContext>().FromMethod(it => it.Container.Resolve<Character>()).AsSingle();
            
            Container.Bind<ICharacterPhysics>().To<CharacterView>().FromComponentInNewPrefab(CharacterPrefab).AsSingle();
        
            Container.Bind<CharacterController>().AsSingle();
            Container.Bind<IController>()
                .WithId("CharacterController")
                .FromMethod(it => it.Container.Resolve<CharacterController>());
            Container.Bind<ICharacterBehaviorContext>()
                .FromMethod(it => it.Container.Resolve<CharacterController>());

            Container.Bind<ICharacterBehaviorFactory>().To<CharacterBehaviorFactory>().AsSingle();
            Container.Bind<IJumpBehaviorFactory>().To<JumpBehaviorFactory>().AsSingle();
            Container.BindInstance(new CharacterSettings());
        }

    }
}