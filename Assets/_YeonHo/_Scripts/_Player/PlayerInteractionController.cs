using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    private readonly List<IPlayerInteractable> interactables =
        new List<IPlayerInteractable>();

    // 현재 상호작용 가능한 대상이 있는지 반환합니다
    public bool HasInteractable
    {
        get
        {
            CleanupInvalidInteractables();
            return GetBestInteractable() != null;
        }
    }

    // 플레이어 범위 안에 들어온 상호작용 대상을 등록합니다
    public void RegisterInteractable(
        IPlayerInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        if (interactables.Contains(interactable))
        {
            return;
        }

        interactables.Add(interactable);
    }

    // 플레이어 범위에서 벗어난 상호작용 대상을 제거합니다
    public void UnregisterInteractable(
        IPlayerInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        interactables.Remove(interactable);
    }

    // 현재 가장 적절한 대상과 상호작용을 시도합니다
    public bool TryInteract()
    {
        IPlayerInteractable interactable =
            GetBestInteractable();

        if (interactable == null)
        {
            return false;
        }

        if (!interactable.CanInteract(gameObject))
        {
            return false;
        }

        interactable.Interact(gameObject);
        return true;
    }

    // 현재 플레이어에게 가장 가까운 상호작용 대상을 찾습니다
    private IPlayerInteractable GetBestInteractable()
    {
        CleanupInvalidInteractables();

        IPlayerInteractable bestInteractable = null;
        float bestSqrDistance = float.MaxValue;

        for (int i = 0; i < interactables.Count; i++)
        {
            IPlayerInteractable candidate =
                interactables[i];

            if (candidate == null)
            {
                continue;
            }

            if (!candidate.CanInteract(gameObject))
            {
                continue;
            }

            Component candidateComponent =
                candidate as Component;

            if (candidateComponent == null)
            {
                continue;
            }

            float sqrDistance =
                (candidateComponent.transform.position -
                 transform.position).sqrMagnitude;

            if (sqrDistance >= bestSqrDistance)
            {
                continue;
            }

            bestSqrDistance = sqrDistance;
            bestInteractable = candidate;
        }

        return bestInteractable;
    }

    // 파괴되었거나 유효하지 않은 상호작용 대상을 목록에서 제거합니다
    private void CleanupInvalidInteractables()
    {
        for (int i = interactables.Count - 1;
             i >= 0;
             i--)
        {
            IPlayerInteractable interactable =
                interactables[i];

            if (interactable == null)
            {
                interactables.RemoveAt(i);
                continue;
            }

            Object unityObject = interactable as Object;

            if (unityObject == null)
            {
                interactables.RemoveAt(i);
            }
        }
    }
}