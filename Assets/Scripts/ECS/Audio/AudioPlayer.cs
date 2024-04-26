using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class AudioPlayer : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity audioBufferEntity = Entity.Null;

    public UnityEvent SeedlingAAttack;
    public UnityEvent SeedlingBAttack;
    public UnityEvent SeedlingCAttack;
    public UnityEvent Harvest;
    public UnityEvent ChangeSelection;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(AudioTag));
        while (!query.TryGetSingletonEntity<AudioTag>(out audioBufferEntity))
            yield return new WaitForSeconds(0.2f);
    }


    void Update()
    {
        if (audioBufferEntity == Entity.Null)
            return;
        var audioEventBuffer = entityManager.GetBuffer<AudioEventBuffer>(audioBufferEntity);
        foreach (var audioEvent in audioEventBuffer)
        {
            switch (audioEvent.Type)
            {
                case AudioEventType.SeedlingAShoot:
                    SeedlingAAttack.Invoke();
                    break;
                case AudioEventType.SeedlingBShoot:
                    SeedlingBAttack.Invoke();
                    break;
                case AudioEventType.SeedlingCShoot:
                    SeedlingCAttack.Invoke();
                    break;
                case AudioEventType.Harvest:
                    Harvest.Invoke();
                    break;
                case AudioEventType.ChangeSelection:
                    ChangeSelection.Invoke();
                    break;
                default:
                    break;
            }
        }

        audioEventBuffer.Clear();
    }
}