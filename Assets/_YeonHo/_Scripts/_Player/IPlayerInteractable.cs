using UnityEngine;

public interface IPlayerInteractable
{
    string PromptText { get; }

    bool CanInteract(GameObject playerObject);

    void Interact(GameObject playerObject);
}