using UnityEngine;

namespace AresResurgence.Interaction
{
    /// <summary>
    /// Core interface for any world object the astronaut can interact with.
    /// </summary>
    public interface IInteractable
    {
        string PromptText { get; }
        bool CanInteract { get; }
        void Interact(GameObject interactor);
    }
}
