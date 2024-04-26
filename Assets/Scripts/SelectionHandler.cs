using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SelectionHandler : MonoBehaviour
{
    public static SelectionHandler Instance;
    public UIDocument MainUIDocument;
    public Texture2D SelectedSlotTexture;
    public Texture2D UnselectedSlotTexture;
    public Texture2D SlotATexture;
    public Texture2D SlotBTexture;
    public Texture2D SlotCTexture;

    void Start()
    {
        Instance = this;
        MainUIDocument = GetComponent<UIDocument>();
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(typeof(UIReady)); // Signal to CanvasSystem
    }
}


struct UIReady : IComponentData { }

partial struct CanvasSystem : ISystem, ISystemStartStop
{
    class SingletonManaged : IComponentData
    {
        public VisualElement Root;
        public VisualElement SlotA;
        public VisualElement SlotB;
        public VisualElement SlotC;

        public IStyle SlotAHolsterStyle;
        public IStyle SlotBHolsterStyle;
        public IStyle SlotCHolsterStyle;

        public Label SlotALabel;
        public Label SlotBLabel;
        public Label SlotCLabel;
    }

    public struct Singleton : IComponentData
    {
        public SeedlingType SlotSelected;
        public SeedlingType LastSlotSelected;
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UIReady>(); // OnStartRunning will be called when UIReady is added
        state.EntityManager.AddComponentData(state.SystemHandle,
            new Singleton()
            {
                SlotSelected = SeedlingType.A,
                LastSlotSelected = SeedlingType.B
            });
    }

    public void OnStartRunning(ref SystemState state)
    {
        var mainUI = SelectionHandler.Instance.MainUIDocument;
        var singletonManaged = new SingletonManaged
        {
            Root = mainUI.rootVisualElement,
            SlotA = mainUI.rootVisualElement.Q<VisualElement>("SlotA"),
            SlotB = mainUI.rootVisualElement.Q<VisualElement>("SlotB"),
            SlotC = mainUI.rootVisualElement.Q<VisualElement>("SlotC")
        };
        state.EntityManager.AddComponentObject(state.SystemHandle, singletonManaged);
        singletonManaged.SlotA.Q<VisualElement>("Image").style.backgroundImage = SelectionHandler.Instance.SlotATexture;
        singletonManaged.SlotB.Q<VisualElement>("Image").style.backgroundImage = SelectionHandler.Instance.SlotBTexture;
        singletonManaged.SlotC.Q<VisualElement>("Image").style.backgroundImage = SelectionHandler.Instance.SlotCTexture;

        singletonManaged.SlotALabel = singletonManaged.SlotA.Q<Label>("Amount");
        singletonManaged.SlotBLabel = singletonManaged.SlotB.Q<Label>("Amount");
        singletonManaged.SlotCLabel = singletonManaged.SlotC.Q<Label>("Amount");

        singletonManaged.SlotAHolsterStyle = singletonManaged.SlotA.Q<VisualElement>("Holster").style;
        singletonManaged.SlotBHolsterStyle = singletonManaged.SlotB.Q<VisualElement>("Holster").style;
        singletonManaged.SlotCHolsterStyle = singletonManaged.SlotC.Q<VisualElement>("Holster").style;
    }

    public void OnUpdate(ref SystemState state)
    {
        var     singletonManaged = SystemAPI.ManagedAPI.GetComponent<SingletonManaged>(state.SystemHandle);
        ref var singleton        = ref SystemAPI.GetComponentRW<Singleton>(state.SystemHandle).ValueRW;

        // Update slot counts
        var seedlingQuery = SystemAPI.QueryBuilder().WithAll<SeedlingTypeData>().Build();
        seedlingQuery.SetSharedComponentFilter(new SeedlingTypeData {Type = SeedlingType.A});
        singletonManaged.SlotALabel.text = $"x{seedlingQuery.CalculateEntityCount()}";

        seedlingQuery.SetSharedComponentFilter(new SeedlingTypeData {Type = SeedlingType.B});
        singletonManaged.SlotBLabel.text = $"x{seedlingQuery.CalculateEntityCount()}";

        seedlingQuery.SetSharedComponentFilter(new SeedlingTypeData {Type = SeedlingType.C});
        singletonManaged.SlotCLabel.text = $"x{seedlingQuery.CalculateEntityCount()}";


        var managedDataOnSystem = SystemAPI.ManagedAPI.GetSingleton<PlayerInputSingleton>();
        if (managedDataOnSystem.input.Gameplay.SeedlingNext.WasPressedThisFrame())
        {
            singleton.SlotSelected = (SeedlingType) (((int) singleton.SlotSelected + 1) % 3);
            
            var audioBuffer = SystemAPI.GetSingletonBuffer<AudioEventBuffer>();
            audioBuffer.Add(new AudioEventBuffer()
            {
                Type = AudioEventType.ChangeSelection,
            });
        }
        if (managedDataOnSystem.input.Gameplay.SeedlingPrevious.WasPressedThisFrame())
        {
            singleton.SlotSelected = singleton.SlotSelected == SeedlingType.A ? SeedlingType.C : (SeedlingType) ((int) singleton.SlotSelected - 1);
            
            var audioBuffer = SystemAPI.GetSingletonBuffer<AudioEventBuffer>();
            audioBuffer.Add(new AudioEventBuffer()
            {
                Type = AudioEventType.ChangeSelection,
            });
        }

        // Select after scroll
        if (singleton.SlotSelected != singleton.LastSlotSelected)
        {
            singleton.LastSlotSelected = singleton.SlotSelected;
            switch (singleton.SlotSelected)
            {
                case SeedlingType.A:
                    singletonManaged.SlotAHolsterStyle.backgroundImage = SelectionHandler.Instance.SelectedSlotTexture;
                    singletonManaged.SlotBHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    singletonManaged.SlotCHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    break;
                case SeedlingType.B:
                    singletonManaged.SlotAHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    singletonManaged.SlotBHolsterStyle.backgroundImage = SelectionHandler.Instance.SelectedSlotTexture;
                    singletonManaged.SlotCHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    break;
                case SeedlingType.C:
                    singletonManaged.SlotAHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    singletonManaged.SlotBHolsterStyle.backgroundImage = SelectionHandler.Instance.UnselectedSlotTexture;
                    singletonManaged.SlotCHolsterStyle.backgroundImage = SelectionHandler.Instance.SelectedSlotTexture;
                    break;
            }
        }
    }


    public void OnStopRunning(ref SystemState state) { }
}