using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeServices();
    }

    private void InitializeServices()
    {
        // Базовые сервисы
        ServiceLocator.Register<ISaveSystem>(new SaveSystem());
        ServiceLocator.Register<ILocalizationSystem>(new LocalizationSystem());
        ServiceLocator.Register<IEventBus>(new EventBus());
        
        // Игровые системы
        ServiceLocator.Register<ITutorialSystem>(new TutorialSystem());
        ServiceLocator.Register<ILevelGenerator>(new LevelGenerator());
        ServiceLocator.Register<IConvoyFactory>(new ConvoyFactory());
        ServiceLocator.Register<IRouteSystem>(new RouteSystem());
        ServiceLocator.Register<ITriggerSystem>(new TriggerSystem());
        ServiceLocator.Register<ISimulationSystem>(new SimulationSystem());
        ServiceLocator.Register<IInputSystem>(new MobileInputSystem());

        // Загрузка данных
        var saveSystem = ServiceLocator.Get<ISaveSystem>();
        saveSystem.Load();
        
        ServiceLocator.Get<ILocalizationSystem>().SetLanguage(saveSystem.GetData().Language);
    }

    private void Update()
    {
        ServiceLocator.Get<IInputSystem>().Update();
        
        // Обновляем симуляцию
        var simulation = ServiceLocator.Get<ISimulationSystem>();
        if (simulation.State == SimulationState.Running)
        {
            simulation.Update(Time.deltaTime);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            ServiceLocator.Get<ISimulationSystem>().PauseSimulation();
            ServiceLocator.Get<ISaveSystem>().Save();
        }
    }

    private void OnApplicationQuit()
    {
        ServiceLocator.Get<ISaveSystem>().Save();
    }
}