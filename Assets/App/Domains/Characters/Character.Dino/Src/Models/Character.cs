using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infra.Components.Tickable;
using Infra.Observable;
using Infra.Observable.UniRx;
using Types;
using UniRx;

namespace App.Character.Dino
{
    internal class Character : DisposableBase, ICharacter
    {
        public IObservableValue<IReadOnlyCollection<CharacterEffect>> Effects => _effectsSubject;
        public IObservable<EffectUpdateOptions> Updated => _updateSubject;

        public float Speed => _currentBehavior.Speed;
        
        private UniTaskCompletionSource _runTaskSource = new UniTaskCompletionSource();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private readonly ICharacterSounds _sounds;

        private readonly ITickableContext _tickableContext;
        private readonly ICharacterBehaviorFactory _behaviorFactory;

        private ICharacterBehavior _defaultBehavior;
        private ICharacterBehavior _currentBehavior = new IdleCharacterBehavior();
        
        private readonly ObservableValue<List<CharacterEffect>> _effectsSubject = new ObservableValue<List<CharacterEffect>>(new List<CharacterEffect>());
        private readonly ObservableValue<EffectUpdateOptions> _updateSubject = new ObservableValue<EffectUpdateOptions>(new EffectUpdateOptions(CharacterEffect.Default, TimeSpan.Zero));
        
        private readonly SerialDisposable _timerDisposable = new SerialDisposable();
        
        public Character(
            ICharacterSounds sounds,
            ITickableContext tickableContext,
            ICharacterBehaviorFactory behaviorFactory)
        {
            _sounds = sounds;
            _behaviorFactory = behaviorFactory;
            _tickableContext = tickableContext;
            
            _disposables.Add( tickableContext.Updated.Subscribe(Update));
            _disposables.Add(_timerDisposable);
        }

        private void ChangeBehavior(ICharacterBehavior behavior, CharacterEffect effect)
        {
            if (_currentBehavior == behavior) 
                return;
            
            _currentBehavior = behavior;
            _effectsSubject.OnNext(new List<CharacterEffect>() { effect});
        }

        public async UniTask Run(CancellationToken token = default)
        {
            var type = CharacterEffect.Default;
            _defaultBehavior = _behaviorFactory.Create(type);
            ChangeBehavior(_defaultBehavior, type);
            
            _runTaskSource = new UniTaskCompletionSource();
            
            await _runTaskSource.Task;
        }

        public async UniTask Idle(CancellationToken token = default)
        {
            _timerDisposable.Disposable = default;
            
            var type = CharacterEffect.Idle;
            var newBehavior = _behaviorFactory.Create(type);
            ChangeBehavior(newBehavior, CharacterEffect.Idle);
            
            _sounds.Play(CharacterSoundType.Idle);
            _runTaskSource.TrySetResult();
            
            await _currentBehavior.Execute(token);
        }

        public UniTask ApplyEffectBehavior(ICharacterBehavior behavior, EffectOptions options, CancellationToken token = default)
        {
            _sounds.Play(CharacterSoundType.Effect);
                  
            ChangeBehavior(behavior, options.Type);
            
            _timerDisposable.Disposable = _tickableContext.Updated.Subscribe(OnEffectTimer);
            _updateSubject.OnNext(new EffectUpdateOptions(options.Type, options.Duration));

            return UniTask.CompletedTask;
        }

        public async UniTask Jump(CancellationToken token = default)
        {
            await _currentBehavior.Execute(token);
        }

        private void OnEffectTimer(float deltaTime)
        {
            var prevOptions = _updateSubject.Value;
            
            var timeLeft = prevOptions.TimeLeft - TimeSpan.FromMilliseconds(deltaTime * 1000);

            if (timeLeft.TotalMilliseconds > 0)
            {
                _updateSubject.OnNext(new EffectUpdateOptions(prevOptions.Effect, timeLeft));
            }
            else
            {
                _timerDisposable.Disposable = default;
                ChangeBehavior(_defaultBehavior, CharacterEffect.Default);
            }
        }
        
        private void Update(float deltaTime)
        {
            _currentBehavior.Update(deltaTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _runTaskSource.TrySetCanceled();
                _disposables.Dispose();
            }
        }
    }
}